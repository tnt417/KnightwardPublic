using System.Collections.Generic;
using System.Linq;
using Mirror;
using TonyDev.Game.Core;
using TonyDev.Game.Core.Attacks;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Enemies;
using TonyDev.Game.Core.Entities.Enemies.ScriptableObjects;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Core.Items;
using TonyDev.Game.Global.Console;
using TonyDev.Game.Level;
using TonyDev.Game.Level.Rooms;
using TonyDev.Game.Level.Rooms.RoomControlScripts;
using TonyDev.Game.UI.Popups;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TonyDev.Game.Global
{
    public enum GamePhase
    {
        Arena,
        Dungeon
    }

    public class GameManager : NetworkBehaviour
    {
        public static GameManager Instance;
        
        //Editor variables
        [SerializeField] private List<ItemData> itemData;
        [SerializeField] private Camera mainCamera;
        //

        public static Camera MainCamera;
        public static readonly List<Item> AllItems = new();
        public static float CrystalHealth = 1000f;
        public static int Money = 0;
        public static List<GameEntity> Entities = new();
        public static readonly List<EnemySpawner> EnemySpawners = new();

        public static int EnemyDifficultyScale =>
            Mathf.CeilToInt(Timer.GameTimer / 60f); //Enemy difficulty scale. Goes up by 1 every minute.

        public static int DungeonFloor = 1;
        public static GamePhase GamePhase;
        public static bool GameControlsActive => !GameConsoleController.IsTyping;

        public static Vector2 MouseDirection =>
            (MainCamera.ScreenToWorldPoint(Input.mousePosition) - Player.LocalInstance.transform.position).normalized;

        [Command(requiresAuthority = false)]
        public void CmdDamageEntity(NetworkIdentity entityObject, float damage, bool isCrit)
        {
            var entity = entityObject.GetComponent<GameEntity>();
            var dmg = entity.ApplyDamage(damage);
            RpcSpawnDmgPopup(entity.transform.position, dmg, isCrit);
        }

        [ClientRpc]
        private void RpcSpawnDmgPopup(Vector2 position, float value, bool isCrit)
        {
            PopupManager.SpawnPopup(position, (int)value, isCrit);
        }

        [Command(requiresAuthority = false)]
        public void CmdSpawnEnemy(string enemyName, Vector2 position, Transform parent)
        {
            var enemyData = ObjectDictionaries.Enemies[enemyName];
            
            EnemySpawnManager.SpawnEnemy(enemyData, position, parent);
        }

        [Command(requiresAuthority = false)]
        public void CmdSpawnProjectile(NetworkIdentity owner, Vector2 direction, ProjectileData projectileData)
        {
            RpcSpawnProjectile(owner, direction, projectileData);
        }

        [ClientRpc]
        public void RpcSpawnProjectile(NetworkIdentity owner, Vector2 direction, ProjectileData projectileData)
        {
            AttackFactory.CreateProjectileAttack(owner.GetComponent<GameEntity>(), direction, projectileData);
        }

        public static void Reset()
        {
            CrystalHealth = 1000f;
            Money = 0;
            Timer.GameTimer = 0;
            AllItems.Clear();
            Entities.Clear();
            EnemySpawners.Clear();
            PlayerStats.Stats.ClearStatBonuses();
        }

        private void Awake()
        {
            if (Instance == null) Instance = this;

            DontDestroyOnLoad(gameObject); //Persist between scenes
            SceneManager.sceneLoaded += OnSceneLoaded;
            MainCamera = mainCamera;

            foreach (var id in itemData.Select(Instantiate))
            {
                AllItems.Add(id.item);
                id.item.Init();
            }
        }


        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.R) && GameControlsActive) TogglePhase(); //Toggle the phase when R is pressed
            if (CrystalHealth <= 0) GameOver(); //Lose the game when the crystal dies
            if (PlayerDeath.Dead && GamePhase == GamePhase.Dungeon) EnterArenaPhase();
        }

        //Switches back and forth between Arena and Dungeon phases
        private void TogglePhase()
        {
            if (GamePhase == GamePhase.Arena) EnterDungeonPhase();
            else if (RoomManager.Instance.InStartingRoom) EnterArenaPhase();
        }

//Teleports player to the arena and sets GamePhase to Arena.
        [GameCommand(Keyword = "arena", PermissionLevel = PermissionLevel.Cheat)]
        public void EnterArenaPhase()
        {
            RoomManager.Instance.DeactivateRoomPhase();
            GamePhase = GamePhase.Arena;
            Player.LocalInstance.gameObject.transform.position =
                GameObject.FindGameObjectWithTag("Castle").transform.position;
            ReTargetEnemies();
        }

//Teleports the player to the dungeon, sets the starting room as active, and sets the GamePhase to Dungeon.
        private void EnterDungeonPhase()
        {
            GamePhase = GamePhase.Dungeon;
            RoomManager.Instance.TeleportPlayerToStart();
            ReTargetEnemies();
        }

        private void ReTargetEnemies()
        {
            foreach (var e in Entities)
            {
                e.UpdateTarget();
            }
        }

        private void GameOver()
        {
            Destroy(Player.LocalInstance.gameObject);
            Destroy(gameObject);
            SceneManager.LoadScene("GameOver");
        }

        [GameCommand(Keyword = "money", PermissionLevel = PermissionLevel.Cheat, SuccessMessage = "Added money.")]
        public void AddMoney(int amount)
        {
            Money += amount;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "CastleScene") //When the castle scene loads, start the arena phase.
            {
                EnterArenaPhase();
            }
        }
    }
}