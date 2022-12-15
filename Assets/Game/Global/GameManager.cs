using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Mirror;
using Steamworks;
using TonyDev.Game.Core.Attacks;
using TonyDev.Game.Core.Effects;
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
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;
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

            var item = ItemGenerator.GenerateItemOfType(it, ir);

            if (item.itemEffects != null)
            {
                foreach (var ge in item.itemEffects)
                {
                    GameEffect.RegisterEffect(ge);
                }
            }

            PlayerInventory.Instance.InsertItem(item);
        }

        [Command(requiresAuthority = false)]
        public void CmdDropItem(Item item, GameEntity dropper)
        {
            ObjectSpawner.SpawnGroundItem(item, 0, dropper.transform.position, dropper.CurrentParentIdentity);
        }

        [Command(requiresAuthority = false)]
        public void CmdSpawnItem(Item item, int costMultiplier, Vector2 pos, NetworkIdentity id)
        {
            ObjectSpawner.SpawnGroundItem(item, costMultiplier, pos,
                id);
        }

        #endregion

        #region Entity

        private static readonly HashSet<GameEntity> Entities = new();
        public static HashSet<GameEntity> EntitiesReadonly => Entities.ToHashSet();

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
                e.CmdUpdateTarget();
            }
        }

        #endregion

        #region Game

        public static float EnemyDifficultyScale =>
            Timer.GameTimer / 60f * 0.3f + DungeonFloor * 0.7f; //Enemy difficulty scale. Goes up by 1 every minute.

        [SyncVar] [HideInInspector] public int dungeonFloor = 1;
        public static int DungeonFloor => Instance.dungeonFloor;
        public static GamePhase GamePhase;

        [SyncVar] public float waveProgress;
        [SyncVar] public float wave;

        [Command(requiresAuthority = false)]
        public void CmdSetWaveProgress(int newWave, float newProgress)
        {
            waveProgress = newProgress;
            wave = newWave;
        }

        [GameCommand(Keyword = "killall", PermissionLevel = PermissionLevel.Cheat,
            SuccessMessage = "Cleared all enemies.")]
        public static void KillAllEnemies()
        {
            Instance.CmdClearEnemies();
        }

        [Command(requiresAuthority = false)]
        private void CmdClearEnemies()
        {
            foreach (var e in Entities.Where(e => e != null && e is Enemy))
            {
                e.Die();
            }
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

        [SyncVar] private int _maxTowers = 5;

        public int MaxTowers => _maxTowers;

        [Command(requiresAuthority = false)]
        public void CmdSetTowerLimit(int limit)
        {
            _maxTowers = limit;
        }

        public bool SpawnTower(Item towerItem, Vector2 pos, NetworkIdentity parent)
        {
            if (parent == null &&
                Entities.Count(e => e is Tower && e.CurrentParentIdentity == null) >= MaxTowers)
            {
                ObjectSpawner.SpawnTextPopup(pos, "Tower limit reached!", Color.red, 0.7f);
                return false;
            }

            CmdSpawnTower(towerItem, pos, parent);

            return true;
        }

        [Command(requiresAuthority = false)]
        public void CmdSpawnTower(Item towerItem, Vector2 pos, NetworkIdentity parent)
        {
            if (parent == null &&
                Entities.Count(e => e is Tower && e.CurrentParentIdentity == null) >= MaxTowers) return;

            ObjectSpawner.SpawnTower(towerItem, pos, parent);
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
        public void CmdSpawnEnemy(string enemyName, Vector2 position, NetworkIdentity parentRoom, int count)
        {
            for(var i = 0; i < count; i++) ObjectSpawner.SpawnEnemy(ObjectFinder.GetPrefab(enemyName), position, parentRoom);
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

        public Tilemap arenaWallTilemap;
        
        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(this);

            AllItems = Resources.LoadAll<ItemData>("Items").Select(id => Instantiate(id)).ToList();

            Random.InitState((int) DateTime.Now.Ticks);

            Player.OnLocalPlayerCreated += Init;

            Pathfinding.CreateArenaPathfinding(arenaWallTilemap);
            
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

        public static void ResetGame()
        {
            if(Crystal.Instance != null) Crystal.Instance.SetHealth(5000f);
            
            Money = 0;
            Essence = 0;
            MoneyDropBonusFactor = 0;
            Timer.GameTimer = 0;

            AllItems?.Clear();
            Entities?.Clear();
            EnemySpawners?.Clear();
            
            if(PlayerInventory.Instance != null) PlayerInventory.Instance.Clear();
            if(PlayerStats.Stats != null) PlayerStats.Stats.ClearStatBonuses();
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
            RoomManager.OnActiveRoomChanged.Invoke();
        }

        private void GameOver()
        {
            GameOverTask().Forget();
        }

        private async UniTask GameOverTask()
        {
            CmdFocusAllCamOnCrystal();

            await UniTask.Delay(TimeSpan.FromSeconds(3));
            
            NetworkManager.singleton.StopHost();
        }

        public bool doCrystalFocusing = false;
        
        [Command(requiresAuthority = false)]
        private void CmdFocusAllCamOnCrystal()
        {
            RpcFocusAllCamOnCrystal();
        }

        [ClientRpc]
        private void RpcFocusAllCamOnCrystal()
        {
            doCrystalFocusing = true;
        }

        //Teleports the player to the dungeon, sets the starting room as active, and sets the GamePhase to Dungeon.
        private void EnterDungeonPhase()
        {
            GamePhase = GamePhase.Dungeon;
            RoomManager.Instance.TeleportPlayerToStart(); //Move the player to the starting room and activate it
            CmdReTargetEnemies();
        }

        private bool _busyRegen;


        [Command(requiresAuthority = false)]
        public void CmdProgressNextDungeonFloor()
        {
            if (_busyRegen) return;
            _busyRegen = true;

            NextDungeonFloor().Forget();
        }

        [GameCommand(Keyword = "floor", PermissionLevel = PermissionLevel.Cheat, SuccessMessage = "Set floor.")]
        public static void SetFloorAndRegen(int floor)
        {
            Instance.CmdSetDungeonFloor(floor - 1);
            Instance.CmdRegenMap();
        }

        [Command(requiresAuthority = false)]
        private void CmdSetDungeonFloor(int floor)
        {
            dungeonFloor = floor;
            SteamFriends.SetRichPresence("status", $"In game: Dungeon Floor {dungeonFloor}");
        }

        [Command(requiresAuthority = false)]
        private void CmdRegenMap()
        {
            dungeonFloor += 1;
            RoomManager.Instance.RpcResetRooms();
            RoomManager.Instance.ResetRooms();
            RoomManager.Instance.GenerateRooms();

            _busyRegen = false;
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