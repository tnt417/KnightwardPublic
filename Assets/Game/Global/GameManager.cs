using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using JetBrains.Annotations;
using Mirror;
using TonyDev.Game.Core;
using TonyDev.Game.Core.Attacks;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Enemies;
using TonyDev.Game.Core.Entities.Enemies.ScriptableObjects;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Core.Items;
using TonyDev.Game.Global.Console;
using TonyDev.Game.Global.Network;
using TonyDev.Game.Level;
using TonyDev.Game.Level.Decorations.Crystal;
using TonyDev.Game.Level.Rooms;
using TonyDev.Game.Level.Rooms.RoomControlScripts;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

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

        #region Items

        [SerializeField] private List<ItemData> itemData;
        public static readonly List<Item> AllItems = new();
        public static int Money = 0;

        [GameCommand(Keyword = "money", PermissionLevel = PermissionLevel.Cheat, SuccessMessage = "Added money.")]
        public void AddMoney(int amount)
        {
            Money += amount;
        }

        [GameCommand(Keyword = "insertitem", PermissionLevel = PermissionLevel.Cheat,
            SuccessMessage = "Inserted item.")]
        public static void InsertItemCommand(string itemType, string itemRarity)
        {
            var it = Enum.Parse<ItemType>(itemType, true);
            var ir = Enum.Parse<ItemRarity>(itemRarity, true);
            
            PlayerInventory.Instance.InsertItem(ItemGenerator.GenerateItemOfType(it, ir));
        }
        
        #endregion

        #region Entity

        private static readonly List<GameEntity> Entities = new();
        public static IEnumerable<GameEntity> EntitiesReadonly => Entities.AsReadOnly();

        public static void AddEntity(GameEntity entity)
        {
            Entities.Add(entity);
            OnEnemyAdd?.Invoke(entity);
        }

        public static void RemoveEntity(GameEntity entity)
        {
            Entities.Remove(entity);
            OnEnemyRemove?.Invoke(entity);
        }
        
        public static Action<GameEntity> OnEnemyAdd;
        public static Action<GameEntity> OnEnemyRemove;
        public static readonly List<EnemySpawner> EnemySpawners = new();
        
        [Command(requiresAuthority = false)]
        public void CmdReTargetEnemies()
        {
            foreach (var e in Entities.Where(e => e is Enemy))
            {
                e.UpdateTarget();
            }
        }

        #endregion

        #region Game

        public static int EnemyDifficultyScale =>
            Mathf.CeilToInt(Timer.GameTimer / 60f); //Enemy difficulty scale. Goes up by 1 every minute.

        [SyncVar] [HideInInspector] public int dungeonFloor = 1;
        public static int DungeonFloor => Instance.dungeonFloor;
        public static GamePhase GamePhase;

        #endregion

        #region Input

        public static bool GameControlsActive => !GameConsole.IsTyping;
        private static Camera _mainCamera;

        public static Vector2 MouseDirection =>
            (_mainCamera.ScreenToWorldPoint(Input.mousePosition) - Player.LocalInstance.transform.position).normalized;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.R) && GameControlsActive && !Player.LocalInstance.playerDeath.dead) TogglePhase(); //Toggle the phase when R is pressed and we are alive
        }

        [Command(requiresAuthority = false)]
        public void CmdWriteChatMessage(string message, NetworkConnectionToClient sender = null)
        {
            if (sender == null) return;
            RpcWriteConsole($"[{LobbyManager.UsernameDict[sender.connectionId]}] {message}");
        }

        [ClientRpc]
        public void RpcWriteConsole(string message)
        {
            GameConsole.Log(message);
        }

        #endregion

        #region Projectiles

        public List<GameObject> projectiles = new();

        #endregion
        
        #region Mirror

        [Command(requiresAuthority = false)]
        public void CmdDestroyProjectile(string identifier)
        {
            RpcDestroyProjectile(identifier);
        }

        [ClientRpc]
        private void RpcDestroyProjectile(string identifier)
        {
            foreach (var p in projectiles)
            {
                if (p == null) continue;
                var att = p.GetComponent<AttackComponent>();
                if (att == null)
                {
                    Debug.LogWarning("Projectile has no attack component!");
                    continue;
                }

                if (att.identifier == identifier)
                {
                    Destroy(p);
                }
            }

            projectiles = projectiles.Where(go => go != null).ToList();
        }
        
        [Command(requiresAuthority = false)]
        public void CmdSpawnTower(string prefabName, Vector2 pos)
        {
            var go = Instantiate(ObjectFinder.GetPrefab(prefabName), pos, Quaternion.identity);
            NetworkServer.Spawn(go, NetworkServer.localConnection);
        }
        
        [Command(requiresAuthority = false)]
        public void CmdDamageEntity(NetworkIdentity entityObject, float damage, bool isCrit, NetworkIdentity exclude)
        {
            if (entityObject == null) return;
            
            var entity = entityObject.GetComponent<GameEntity>();

            if (entity == null)
            {
                Debug.LogWarning($"Net object {entityObject.gameObject.name} is not an entity!");
                return;
            }
            
            var dmg = entity is Player ? damage : entity.ApplyDamage(damage); //Players should have already been damaged on the client
            RpcSpawnDmgPopup(entity.transform.position, dmg, isCrit, exclude);
        }

        [Command(requiresAuthority = false)]
        public void CmdSpawnMoney(int amount, Vector2 position, NetworkIdentity parent)
        {
            RpcSpawnMoney(amount, position, parent);
        }

        [ClientRpc]
        private void RpcSpawnMoney(int amount, Vector2 position, NetworkIdentity parent)
        {
            ObjectSpawner.SpawnMoney(amount, position, parent);
        }

        [ClientRpc]
        private void RpcSpawnDmgPopup(Vector2 position, float value, bool isCrit, NetworkIdentity exclude)
        {
            if (NetworkClient.localPlayer == exclude) return;
            
            ObjectSpawner.SpawnPopup(position, (int) value, isCrit);
        }

        [Command(requiresAuthority = false)]
        public void CmdSpawnEnemy(string enemyName, Vector2 position, NetworkIdentity parentRoom)
        {
            var enemyData = ObjectFinder.GetEnemyData(enemyName);

            ObjectSpawner.SpawnEnemy(enemyData, position, parentRoom);
        }

        [Command(requiresAuthority = false)]
        public void CmdSpawnProjectile(NetworkIdentity owner, Vector2 pos, Vector2 direction, ProjectileData projectileData, string identifier)
        {
            RpcSpawnProjectile(owner, pos, direction, projectileData, identifier);
        }

        [ClientRpc]
        private void RpcSpawnProjectile(NetworkIdentity owner, Vector2 pos, Vector2 direction, ProjectileData projectileData, string identifier)
        {
            if (owner == null || owner == NetworkClient.localPlayer) return; //Projectiles should be spawned locally for the owner player of the projectile.
            
            var entity = owner.GetComponent<GameEntity>();
            if (!entity.VisibleToHost && isClient && isServer) return; //If we are the host and the entity is not visible to the host, return.
            AttackFactory.CreateProjectileAttack(entity, pos, direction, projectileData, identifier);
        }

        #endregion

        #region Initialization

        public static event Action OnInitializeGameServer;
        public static event Action OnInitializeGameClientOnly;
        public static event Action OnInitializeGame;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(this);

            Player.OnLocalPlayerCreated += Init;

            if (isServer)
            {
                netIdentity.AssignClientAuthority(NetworkServer.localConnection);
            }

            _mainCamera = Camera.main;

            foreach (var id in itemData.Select(Instantiate))
            {
                AllItems.Add(id.item);
                id.item.Init();
            }
        }

        private void Init()
        {
            OnInitializeGame?.Invoke();
            if (isServer) OnInitializeGameServer?.Invoke();
            if (isClientOnly) OnInitializeGameClientOnly?.Invoke();

            Crystal.Instance.OnDeath += value => GameOver();
            EnterArenaPhase();
        }

        #endregion

        #region Gamestate Control

        public static void ResetGame()
        {
            Crystal.Instance.SetHealth(5000f);
            Money = 0;
            Timer.GameTimer = 0;
            AllItems.Clear();
            Entities.Clear();
            EnemySpawners.Clear();
            PlayerStats.Stats.ClearStatBonuses();
        }

        //Switches back and forth between Arena and Dungeon phases
        private void TogglePhase()
        {
            if (GamePhase == GamePhase.Arena) StartCoroutine(PhaseTransition(GamePhase.Dungeon));
            else if (RoomManager.Instance.CanSwitchPhases) StartCoroutine(PhaseTransition(GamePhase.Arena));
        }

        private IEnumerator PhaseTransition(GamePhase targetPhase)
        {
            TransitionController.Instance.FadeInOut(); //Transition to make it less jarring.
            yield return
                new WaitUntil(() =>
                    TransitionController.Instance.OutTransitionDone); //Wait until the transition is over

            FindObjectOfType<SmoothCameraFollow>().FixateOnPlayer();
            
            switch (targetPhase)
            {
                case GamePhase.Arena:
                    EnterArenaPhase();
                    break;
                case GamePhase.Dungeon:
                    EnterDungeonPhase();
                    break;
            }
        }

        //Teleports player to the arena and sets GamePhase to Arena.
        [GameCommand(Keyword = "arena", PermissionLevel = PermissionLevel.Cheat)]
        public void EnterArenaPhase()
        {
            RoomManager.Instance.DeactivateRoomPhase();
            GamePhase = GamePhase.Arena;
            Player.LocalInstance.gameObject.transform.position =
                GameObject.FindGameObjectWithTag("Castle").transform.position;
            CmdReTargetEnemies();
        }

        private void GameOver()
        {
            Destroy(gameObject);
            NetworkServer.Shutdown();
            SceneManager.LoadScene("GameOver");
        }

        //Teleports the player to the dungeon, sets the starting room as active, and sets the GamePhase to Dungeon.
        private void EnterDungeonPhase()
        {
            GamePhase = GamePhase.Dungeon;
            RoomManager.Instance.TeleportPlayerToStart(); //Move the player to the starting room and activate it
            CmdReTargetEnemies();
        }

        [Command(requiresAuthority = false)]
        public void CmdProgressNextDungeonFloor()
        {
            dungeonFloor += 1;
            RoomManager.Instance.ResetRooms();
            RoomManager.Instance.GenerateRooms();
        }

        [Command(requiresAuthority = false)]
        public void CmdTeleportAllInDungeonToStart()
        {
            RpcTeleportToStart();
        }

        [ClientRpc]
        private void RpcTeleportToStart()
        {
            if (GamePhase == GamePhase.Dungeon)
            {
                RoomManager.Instance.TeleportPlayerToStart();
            }
        }

        #endregion
    }
}