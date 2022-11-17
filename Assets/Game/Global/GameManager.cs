using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Mirror;
using TonyDev.Game.Core.Attacks;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Enemies;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Core.Entities.Towers;
using TonyDev.Game.Core.Items;
using TonyDev.Game.Global.Console;
using TonyDev.Game.Global.Network;
using TonyDev.Game.Level;
using TonyDev.Game.Level.Decorations.Crystal;
using TonyDev.Game.Level.Rooms;
using TonyDev.Game.Level.Rooms.RoomControlScripts;
using TonyDev.Game.UI.GameInfo;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

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
        [SerializeField] private Vector2 arenaSpawnPos;
        public static List<ItemData> AllItems = new();
        public static int Money = 0;
        public static int Essence = 0;
        public static float MoneyDropBonusFactor;

        [SyncVar] public int timeSeconds;

        [GameCommand(Keyword = "money", PermissionLevel = PermissionLevel.Cheat, SuccessMessage = "Added money.")]
        public void AddMoney(int amount)
        {
            Money += amount;
        }

        [GameCommand(Keyword = "essence", PermissionLevel = PermissionLevel.Cheat, SuccessMessage = "Added essence.")]
        public void AddEssence(int amount)
        {
            Essence += amount;
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

        [SyncVar] public float waveProgress;

        [Command(requiresAuthority = false)]
        public void CmdSetWaveProgress(float value)
        {
            waveProgress = value;
        }

        [Command(requiresAuthority = false)]
        public void CmdSetBreakMultiplier(float value)
        {
            RpcSetBreakMultiplier(value);
        }

        [ClientRpc]
        private void RpcSetBreakMultiplier(float value)
        {
            WaveManager.BreakPassingMultiplier = value;
        }

        #endregion

        #region Input

        public static bool GameControlsActive => !GameConsole.IsTyping;
        public static Camera MainCamera;

        public static Vector2 MouseDirection =>
            (MainCamera.ScreenToWorldPoint(Input.mousePosition) - Player.LocalInstance.transform.position).normalized;

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

        public SyncList<Vector2Int> OccupiedTowerSpots = new();

        public int maxArenaTowers = 5;

        public bool SpawnTower(string prefabName, Vector2 pos, NetworkIdentity parent)
        {
            if (parent == null &&
                Entities.Count(e => e is Tower && e.CurrentParentIdentity == null) >= maxArenaTowers)
            {
                ObjectSpawner.SpawnTextPopup(pos, "Tower limit reached!", Color.red, 0.7f);
                return false;
            }

            CmdSpawnTower(prefabName, pos, parent);

            return true;
        }

        [Command(requiresAuthority = false)]
        public void CmdSpawnTower(string prefabName, Vector2 pos, NetworkIdentity parent)
        {
            if (parent == null &&
                Entities.Count(e => e is Tower && e.CurrentParentIdentity == null) >= maxArenaTowers) return;

            ObjectSpawner.SpawnTower(prefabName, pos, parent);
            OccupiedTowerSpots.Add(new Vector2Int((int) pos.x, (int) pos.y));
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
        public void RpcSpawnDmgPopup(Vector2 position, float value, bool isCrit, NetworkIdentity exclude)
        {
            if (NetworkClient.localPlayer == exclude) return;

            ObjectSpawner.SpawnDmgPopup(position, value, isCrit);
        }

        [Command(requiresAuthority = false)]
        public void CmdSpawnEnemy(string enemyName, Vector2 position, NetworkIdentity parentRoom)
        {
            ObjectSpawner.SpawnEnemy(ObjectFinder.GetPrefab(enemyName), position, parentRoom);
        }

        [ClientRpc]
        private void RpcSpawnStaticAttack(NetworkIdentity owner, Vector2 pos, Vector2 direction,
            ProjectileData projectileData, string identifier)
        {
            if (owner == null || owner == NetworkClient.localPlayer)
                return; //Projectiles should be spawned locally for the owner player of the projectile.

            var entity = owner.GetComponent<GameEntity>();
            if (!entity.VisibleToHost && isClient && isServer)
                return; //If we are the host and the entity is not visible to the host, return.
            AttackFactory.CreateProjectileAttack(entity, pos, direction, projectileData, identifier);
        }

        public GameObject SpawnProjectile(GameEntity owner, Vector2 pos, Vector2 direction,
            ProjectileData projectileData, bool localOnly = false)
        {
            var identifier = AttackComponent.GetUniqueIdentifier(owner);
            var go = AttackFactory.CreateProjectileAttack(owner, pos, direction, projectileData, identifier);
            if (!localOnly) Instance.CmdSpawnProjectile(owner.netIdentity, pos, direction, projectileData, identifier);
            return go;
        }

        [Command(requiresAuthority = false)]
        private void CmdSpawnProjectile(NetworkIdentity owner, Vector2 pos, Vector2 direction,
            ProjectileData projectileData, string identifier)
        {
            RpcSpawnProjectile(owner, pos, direction, projectileData, identifier);
        }

        [ClientRpc]
        private void RpcSpawnProjectile(NetworkIdentity owner, Vector2 pos, Vector2 direction,
            ProjectileData projectileData, string identifier)
        {
            if (owner == null || owner == NetworkClient.localPlayer)
                return; //Projectiles should be spawned locally for the owner player of the projectile.

            var entity = owner.GetComponent<GameEntity>();
            if (!entity.VisibleToHost && isClient && isServer)
                return; //If we are the host and the entity is not visible to the host, return.
            AttackFactory.CreateProjectileAttack(entity, pos, direction, projectileData, identifier);
        }

        #endregion

        #region Initialization

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(this);

            AllItems = Resources.LoadAll<ItemData>("Items").Select(id => Instantiate(id)).ToList();

            Random.InitState((int) DateTime.Now.Ticks);

            Player.OnLocalPlayerCreated += Init;

            if (isServer)
            {
                netIdentity.AssignClientAuthority(NetworkServer.localConnection);
            }

            MainCamera = Camera.main;
        }

        private void Init()
        {
            Crystal.Instance.OnDeathOwner += value => GameOver();
            EnterArenaPhase();
        }

        #endregion

        #region Gamestate Control

        private static void ResetGame()
        {
            Crystal.Instance.SetHealth(5000f);
            Money = 0;
            Timer.GameTimer = 0;
            AllItems.Clear();
            Entities.Clear();
            Destroy(FindObjectOfType<CustomNetworkManager>().gameObject);
            EnemySpawners.Clear();
            PlayerInventory.Instance.Clear();
            PlayerStats.Stats.ClearStatBonuses();
        }

        //Switches back and forth between Arena and Dungeon phases
        public void TogglePhase()
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
            Player.LocalInstance.gameObject.transform.position = arenaSpawnPos;
            CmdReTargetEnemies();
        }

        private void GameOver()
        {
            ResetGame();
            NetworkServer.Shutdown();
            NetworkClient.Shutdown();
            Destroy(gameObject);
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
            NextDungeonFloor().Forget();
        }

        [Command(requiresAuthority = false)]
        private void CmdRegenMap()
        {
            dungeonFloor += 1;
            RoomManager.Instance.RpcResetRooms();
            RoomManager.Instance.ResetRooms();
            RoomManager.Instance.GenerateRooms();
        }

        private async UniTask NextDungeonFloor()
        {
            CmdFadeOut();

            await UniTask.Delay(TimeSpan.FromSeconds(TransitionController.FadeOutTimeSeconds));

            CmdRegenMap();
        }

        [Command(requiresAuthority = false)]
        public void CmdFadeOut()
        {
            RpcFadeOut();
        }

        [ClientRpc]
        private void RpcFadeOut()
        {
            if (GamePhase == GamePhase.Dungeon) TransitionController.Instance.FadeOut();
        }

        #endregion
    }
}