using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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
using TonyDev.Game.UI.Menu.GameOver;
using TonyDev.Game.UI.Tower;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
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
        
        public const bool IsDemo = false;
        
        // Set true to skip menu and speed up transitions
        public const bool QuickTestMode = false;

        #region Items

        [SerializeField] private List<ItemData> itemData;
        [SerializeField] private Vector2 arenaSpawnPos;
        public static List<ItemData> AllItems = new();

        public static Vector2 MousePosWorld => MainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        
        public static int Money = 0;

        //public static int Essence = 0;
        public static float MoneyDropBonusFactor;

        [SyncVar] public int timeSeconds;

        [GameCommand(Keyword = "money", PermissionLevel = PermissionLevel.Cheat, SuccessMessage = "Added money.")]
        public void AddMoney(int amount)
        {
            Money += amount;
        }

        // [GameCommand(Keyword = "essence", PermissionLevel = PermissionLevel.Cheat, SuccessMessage = "Added essence.")]
        // public void AddEssence(int amount)
        // {
        //     Essence += amount;
        // }

        [GameCommand(Keyword = "insertitem", PermissionLevel = PermissionLevel.Cheat,
            SuccessMessage = "Inserted item.")]
        public static void InsertItemCommand(string itemType, string itemRarityOrName)
        {
            var it = Enum.Parse<ItemType>(itemType, true);

            itemRarityOrName = itemRarityOrName.Replace("_", " ");
            itemRarityOrName = itemRarityOrName.ToLower();

            ItemRarity rarity = ItemRarity.Common;

            string itemName = "";
            
            switch (itemRarityOrName)
            {
                case "common": rarity = ItemRarity.Common; break;
                case "uncommon": rarity = ItemRarity.Uncommon; break;
                case "rare": rarity = ItemRarity.Rare; break;
                case "unique": rarity = ItemRarity.Unique; break;
                default: itemName = itemRarityOrName; break;
            }

            var item = itemName == "" ? ItemGenerator.GenerateItemOfType(it, rarity) : ItemGenerator.GenerateItemFromData(UnlocksManager.UnlockedItems.FirstOrDefault(item => item.item.itemName.ToLower() == itemName));
            
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

        public static IEnumerable<GameEntity> GetEntitiesInRange(Vector2 pos, float range)
        {
            return EntitiesReadonly.Where(e => Vector2.Distance(e.transform.position, pos) < range);
        }

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
                e.UpdateTargets();
            }
        }

        #endregion

        #region Game

        public static float EnemyDifficultyScale =>
            Timer.GameTimer / 60f * 0.3f + DungeonFloor * 0.7f; //Enemy difficulty scale. Goes up by 1 every minute.

        [SyncVar] [HideInInspector] public int dungeonFloor = 1;
        public static int DungeonFloor => Instance.dungeonFloor;
        public static GamePhase GamePhase;

        [SerializeField] private Canvas uiCanvas;

        [SyncVar] public float waveProgress;
        [SyncVar] public float wave;

        [GameCommand(Keyword = "ui", PermissionLevel = PermissionLevel.Default, SuccessMessage = "Toggled UI")]
        public void SetUi(string onoff)
        {
            if (onoff == "")
            {
                uiCanvas.enabled = !uiCanvas.enabled;
                return;
            }
            
            var e = onoff == "on";

            uiCanvas.enabled = e;
        }
        
        [Command(requiresAuthority = false)]
        public void CmdSetWaveProgress(int newWave, float newProgress)
        {
            waveProgress = newProgress;
            wave = newWave;
        }

        [Server]
        public void SpawnHealthPickupOnEach(Vector2 location, NetworkIdentity parent)
        {
            RpcSpawnHealthPickup(location, parent);
        }

        [ClientRpc]
        private void RpcSpawnHealthPickup(Vector2 location, NetworkIdentity parent)
        {
            ObjectSpawner.SpawnHealthPickupLocal(location, parent);
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

        #endregion

        #region Input

        public static bool GameControlsActive => !GameConsole.IsTyping && !PauseController.Paused;
        public static Camera MainCamera;

        public static Vector2 MouseDirection =>
            MainCamera != null ? (MainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue()) -
                      Player.LocalInstance.transform.position).normalized : Vector2.zero;

        public static Vector2 MouseDirectionLow =>
            ((Vector2) MainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue()) -
             ((Vector2) Player.LocalInstance.transform.position - new Vector2(0, 0.4f))).normalized;
        
        public static Vector2 MouseDirectionHigh =>
            ((Vector2) MainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue()) -
             ((Vector2) Player.LocalInstance.transform.position + new Vector2(0, 0.4f))).normalized;

        [Command(requiresAuthority = false)]
        public void CmdWriteChatMessage(string message, CustomRoomPlayer localRoomPlayer)
        {
            if (localRoomPlayer == null) return;

            RpcWriteConsole($"[{localRoomPlayer.username}] {message}");
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
                    //Debug.LogWarning("Projectile has no attack component!");
                    continue;
                }

                if (att.identifier == identifier)
                {
                    Destroy(p);
                }
            }

            projectiles = projectiles.Where(go => go != null).ToList();
        }

        public readonly SyncList<Vector2Int> OccupiedTowerSpots = new();

        [SyncVar] private int _maxTowers = 100; //3;

        public int MaxTowers => _maxTowers;

        [Command(requiresAuthority = false)]
        public void CmdSetTowerLimit(int limit)
        {
            _maxTowers = limit;
        }

        private bool SpawnTower(Item towerItem, Vector2 pos, NetworkIdentity parent)
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

        public async UniTask<bool> SpawnTowerTask(Item towerItem, Vector2 pos, NetworkIdentity parent)
        {
            var occupied = Instance.OccupiedTowerSpots.Contains(new Vector2Int((int) pos.x, (int) pos.y));

            if (!occupied)
            {
                return SpawnTower(towerItem, pos, parent);
            }

            Item pickingUpItem = null;

            foreach (var ge in Entities.Where(ge => ge is Tower))
            {
                var tower = ge as Tower;

                if ((Vector2) tower.transform.position != pos) continue;

                pickingUpItem = tower.myItem;

                tower.CmdRequestPickup();
                break;
            }

            await UniTask.WaitUntil(() => TowerUIController.Instance.Towers.ContainsValue(pickingUpItem));

            return SpawnTower(towerItem, pos, parent);
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

        [Command(requiresAuthority = false)]
        public void CmdSpawnEssence(int amount, Vector2 position, NetworkIdentity parent)
        {
            RpcSpawnMoney(amount, position, parent);
        }

        [ClientRpc]
        private void RpcSpawnEssence(int amount, Vector2 position, NetworkIdentity parent)
        {
            ObjectSpawner.SpawnEssence(amount, position, parent);
        }

        [ClientRpc]
        public void RpcSpawnDmgPopup(Vector2 position, float value, bool isCrit, NetworkIdentity exclude, DamageType damageType)
        {
            if (NetworkClient.localPlayer == exclude) return;

            ObjectSpawner.SpawnDmgPopup(position, value, isCrit, damageType);
        }

        [Command(requiresAuthority = false)]
        public void CmdSpawnEnemy(string enemyName, Vector2 position, NetworkIdentity parentRoom, int count)
        {
            for (var i = 0; i < count; i++)
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
            if (!localOnly)
                Instance.CmdSpawnProjectile(owner.netIdentity, pos, direction, projectileData, identifier,
                    Player.LocalInstance.netId);
            return go;
        }

        [Command(requiresAuthority = false)]
        private void CmdSpawnProjectile(NetworkIdentity owner, Vector2 pos, Vector2 direction,
            ProjectileData projectileData, string identifier, uint senderPlayerNetId)
        {
            RpcSpawnProjectile(owner, pos, direction, projectileData, identifier, senderPlayerNetId);
        }

        [ClientRpc]
        private void RpcSpawnProjectile(NetworkIdentity owner, Vector2 pos, Vector2 direction,
            ProjectileData projectileData, string identifier, uint excludePlayer)
        {
            if (Player.LocalInstance.netId == excludePlayer) return;

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

        public static Action OnGameManagerAwake;

        public static int SessionPlayCount = 0;

        private void Awake()
        {
            Debug.Log("Game manager awake!");
            
            if (Instance == null) Instance = this;
            else Destroy(this);

            SessionPlayCount++;

            AllItems = Resources.LoadAll<ItemData>("Items").Select(Instantiate).ToList();

            foreach (var crp in FindObjectsOfType<CustomRoomPlayer>())
            {
                if (crp.isOwned) continue;
                foreach (var itemName in crp.UnlockedItemNames)
                {
                    UnlocksManager.Instance.AddUnlockSessionOnly(itemName);
                }
            }

            Random.InitState((int) DateTime.Now.Ticks);

            Player.OnLocalPlayerCreated += Init;

            Pathfinding.CreateArenaPathfinding(arenaWallTilemap);

            if (isServer)
            {
                netIdentity.AssignClientAuthority(NetworkServer.localConnection);
            }

            MainCamera = Camera.main;

            OnGameManagerAwake?.Invoke();
        }

        private void Init()
        {
            Debug.Log("Game manager init!");
            
            Crystal.Instance.OnDeathOwner += value => GameOver();
            EnterArenaPhase();

            var playerFrac = (CustomRoomPlayer.Local.playerNumber + 1) / 4f;
            
            Player.LocalInstance.transform.position = arenaSpawnPos + new Vector2(Mathf.Cos((((float)playerFrac) * 2 * Mathf.PI)) * 3.5f,
                Mathf.Sin((((float)playerFrac) * 2 * Mathf.PI)) * 2.5f);
            arenaSpawnPos = new Vector2(4980, -6f);
        }

        #endregion

        #region Gamestate Control

        public void GameWin()
        {
            GameWinTask().Forget();
        }

        private async UniTask GameWinTask()
        {
            
            CmdGameWin();

            await UniTask.Delay(TimeSpan.FromSeconds(2));

            NetworkManager.singleton.StopHost();
        }

        [Command(requiresAuthority = false)]
        private void CmdGameWin()
        {
            RpcGameWin();
        }
        
        [ClientRpc]
        private void RpcGameWin()
        {
            PlayStats.GameWon = true;
            PlayStats.FloorsCompleted = DungeonFloor;
            PlayStats.GameTimeSeconds = Timer.GameTimer;
            TransitionGameEnd(1).Forget();
        }

        public static void ResetGame()
        {
            if (Crystal.Instance != null) Crystal.Instance.SetHealth(5000f);

            Money = 0;
            //Essence = 0;
            MoneyDropBonusFactor = 0;
            Timer.GameTimer = 0;
            CustomRoomPlayer.NumPlayersServer = 0;

            AllItems?.Clear();
            Entities?.Clear();
            EnemySpawners?.Clear();

            if (SessionPlayCount > 0) Player.OnLocalPlayerCreated -= Instance.Init;

            if (PlayerInventory.Instance != null) PlayerInventory.Instance.Clear();
            if (PlayerStats.LocalStats != null) PlayerStats.LocalStats.ClearStatBonuses();
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
            //CmdReTargetEnemies();
            RoomManager.OnActiveRoomChanged.Invoke();
        }

        private void GameOver()
        {
            GameOverTask().Forget();
        }

        private async UniTask GameOverTask()
        {
            
            CmdGameOver();
            CmdFocusAllCamOnCrystal();

            await UniTask.Delay(TimeSpan.FromSeconds(5));

            NetworkManager.singleton.StopHost();
        }

        [Command(requiresAuthority = false)]
        private void CmdGameOver()
        {
            RpcGameOver();
        }
        
        [ClientRpc]
        private void RpcGameOver()
        {
            PlayStats.GameWon = false;
            PlayStats.FloorsCompleted = DungeonFloor;
            PlayStats.GameTimeSeconds = Timer.GameTimer;
            TransitionGameEnd(4).Forget();
        }
        
        private async UniTask TransitionGameEnd(float delaySeconds)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(delaySeconds));
            TransitionController.Instance.FadeOut();
            await UniTask.WaitUntil(() => SceneManager.GetActiveScene().name == "GameOver");
            TransitionController.Instance.FadeIn();
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
            RoomManager.Instance.TeleportPlayerToStart(); //Move the player to the starting room and activate it
            GamePhase = GamePhase.Dungeon;
            //CmdReTargetEnemies();
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
            RoomManager.Instance.GenerateTask().Forget();

            if (dungeonFloor > 1 && (dungeonFloor - 1) % 10 == 0)
            {
                RpcUnlockRandomItem();
            }

            _busyRegen = false;
        }

        [ClientRpc]
        private void RpcUnlockRandomItem()
        {
            UnlocksManager.Instance.UnlockRandomItem();
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