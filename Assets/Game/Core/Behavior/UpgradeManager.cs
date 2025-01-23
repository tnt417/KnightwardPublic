using System;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using TonyDev.Game.Core.Attacks;
using TonyDev.Game.Core.Effects;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Core.Items;
using TonyDev.Game.Core.Items.Relics.FlamingBoot;
using TonyDev.Game.Global;
using TonyDev.Game.Level.Decorations.Crystal;
using TonyDev.Game.UI.Tower;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace TonyDev.Game.Core.Behavior
{
    public class UpgradeData
    {
        public Sprite Icon;
        public string Name;
        public string Description;
        public Func<bool> IsPurchasable;
        public Action<UpgradeEntry, GameEntity, bool> OnPurchase;
        public UpgradeCategory Category;

        public UpgradeData(Sprite icon, string name, string description, Func<bool> isPurchasable,
            Action<UpgradeEntry, GameEntity, bool> onPurchase, UpgradeCategory category)
        {
            Icon = icon;
            Name = name;
            Description = description;
            IsPurchasable = isPurchasable;
            OnPurchase = onPurchase;
            Category = category;
        }
    }

    public class UpgradeManager : NetworkBehaviour
    {
        // Singleton to allow easy global access. Only one instance of this class should ever exist.
        public static UpgradeManager Instance;

        //Inspector variables

        [SerializeField] private GameObject upgradeEntryPrefab; // Prefab created in the UI for each upgrade entry
        [SerializeField] private GameObject upgradeEntryParent; // Parent of UI objects
        [SerializeField] private GameObject uiToggleObject; // Object used to toggle the UI

        /********************************/

        private readonly Dictionary<int, Action<UpgradeEntry, GameEntity, bool>>
            _onPurchaseActions = new(); // (id, purchase function): purchase function to
        // be called when an upgrade is purchased

        private readonly Dictionary<int, UpgradeEntry>
            _createdEntries = new(); // (id, UpgradeEntry): used to access classes corresponding to IDs

        private List<UpgradeData> _possibleUpgrades = new();

        private float _moveSinceActive; // Used to close the UI when the player tries to move

        public static Action OnUpgradeLocal;

        [FormerlySerializedAs("_filter")] public List<UpgradeCategory> filter;

        // public void ToggleUICrystal()
        // {
        //     ToggleUI(new List<UpgradeCategory> {UpgradeCategory.Crystal});
        // }

        public void ToggleUIPlayer()
        {
            ToggleUI(new List<UpgradeCategory>
                {UpgradeCategory.Crystal, UpgradeCategory.Defensive, UpgradeCategory.Offensive, UpgradeCategory.Utility});
        }

        // Toggle the UI object. 
        private void ToggleUI(List<UpgradeCategory> newFilter)
        {
            _moveSinceActive = 0f;

            filter = newFilter;

            uiToggleObject.SetActive(!uiToggleObject.activeSelf);
        }

        // Singleton code.
        public void Awake()
        {
            if (Instance != null) //Debug.LogWarning("Upgrade manager already exists!");
            Instance = this;
        }

        private List<string> _purchasedUpgrades = new(); // Track purchased upgrade names for upgrade chains

        private void Start()
        {
            Player.LocalInstance.playerMovement.OnPlayerMove +=
                (v) => _moveSinceActive += v.magnitude; // Subscribe event to track player distance moved

            // Register the upgrades on Start

            OnUpgradeLocal += QuickHideUpgrades;

            #region Upgrade Registering

            _possibleUpgrades.Add(new UpgradeData(ObjectFinder.GetSprite("crystal_0"), "Crystal Health I",
                "+100% crystal max HP.",
                () => true,
                (upgrade, entity, serverCall) =>
                {
                    if (serverCall)
                    {
                        Crystal.Instance.Stats.AddStatBonus(StatType.AdditivePercent, Stat.Health, 1.0f,
                            "Crystal Health I");
                        Crystal.Instance.FullHeal();
                    }
                }, UpgradeCategory.Crystal));

            _possibleUpgrades.Add(new UpgradeData(ObjectFinder.GetSprite("crystal_0"), "Crystal Health II",
                "+200% crystal max HP.", () => _purchasedUpgrades.Contains("Crystal Health I"),
                (upgrade, entity, serverCall) =>
                {
                    if (serverCall)
                    {
                        Crystal.Instance.Stats.AddStatBonus(StatType.AdditivePercent, Stat.Health, 2.0f,
                            "Crystal Health II");
                        Crystal.Instance.FullHeal();
                    }
                }, UpgradeCategory.Crystal));

            _possibleUpgrades.Add(new UpgradeData(ObjectFinder.GetSprite("crystal_0"), "Crystal Health III",
                "+400% crystal max HP.",
                () => _purchasedUpgrades.Contains("Crystal Health I") &&
                      _purchasedUpgrades.Contains("Crystal Health II"),
                (upgrade, entity, serverCall) =>
                {
                    if (serverCall)
                    {
                        Crystal.Instance.Stats.AddStatBonus(StatType.AdditivePercent, Stat.Health, 4.0f,
                            "Crystal Health III");
                        Crystal.Instance.FullHeal();
                    }
                }, UpgradeCategory.Crystal));

            _possibleUpgrades.Add(new UpgradeData(ObjectFinder.GetSprite("crystal_0"), "Ally Speed Aura I",
                "Allies within 7 tiles of the crystal gain +25% attack speed, frequency, and move speed.",
                () => true,
                (upgrade, entity, serverCall) =>
                {
                    if (serverCall)
                    {
                        Crystal.Instance.AddEffect(new AreaBuffEffect()
                        {
                            Radius = 7f, StatBonuses = new[]
                            {
                                new StatBonus(StatType.AdditivePercent, Stat.AttackSpeed, 0.25f, "Ally Speed Aura I"),
                                new StatBonus(StatType.AdditivePercent, Stat.MoveSpeed, 0.25f, "Ally Speed Aura I")
                            },
                            targetTeam = Team.Player
                        }, Crystal.Instance);
                    }
                }, UpgradeCategory.Crystal));

            _possibleUpgrades.Add(new UpgradeData(ObjectFinder.GetSprite("crystal_0"), "Ally Strength Aura I",
                "Allies within 7 tiles of the crystal gain +40% damage and potency.",
                () => true,
                (upgrade, entity, serverCall) =>
                {
                    if (serverCall)
                    {
                        Crystal.Instance.AddEffect(new AreaBuffEffect()
                        {
                            Radius = 7f, StatBonuses = new[]
                            {
                                new StatBonus(StatType.AdditivePercent, Stat.Damage, 0.4f, "Ally Strength Aura I"),
                            },
                            targetTeam = Team.Player
                        }, Crystal.Instance);
                    }
                }, UpgradeCategory.Crystal));

            _possibleUpgrades.Add(new UpgradeData(ObjectFinder.GetSprite("RegeneratorTower_0"), "Crystal Regen",
                "Give the crystal the ability to slowly repair itself over time.",
                () => true,
                (upgrade, entity, serverCall) =>
                {
                    if (serverCall)
                    {
                        Crystal.Instance.AddEffect(new PercentRegenEffect()
                        {
                            PercentRegen = 0.002f
                        }, Crystal.Instance);
                    }
                }, UpgradeCategory.Crystal));

            _possibleUpgrades.Add(new UpgradeData(ObjectFinder.GetSprite("essence"), "Tower Restoration",
                "The crystal slowly repairs nearby towers.",
                () => _purchasedUpgrades.Contains("Crystal Regen"),
                (upgrade, entity, serverCall) =>
                {
                    if (serverCall)
                    {
                        Crystal.Instance.AddEffect(new AreaRepairEffect()
                        {
                            Cooldown = 5,
                            Percent = 0.05f,
                            Range = 15f
                        }, Crystal.Instance);
                    }
                }, UpgradeCategory.Crystal));
            
            _possibleUpgrades.Add(new UpgradeData(ObjectFinder.GetSprite("inventoryIcons_3"), "Damage I",
                "x1.25 to player damage stat",
                () => true,
                (upgrade, entity, serverCall) =>
                {
                    if(!serverCall) entity.Stats.AddStatBonus(StatType.Multiplicative, Stat.Damage, 1.25f, "Damage I");
                }, UpgradeCategory.Offensive));

            _possibleUpgrades.Add(new UpgradeData(ObjectFinder.GetSprite("inventoryIcons_3"), "Damage II",
                "x1.25 to player damage stat",
                () => _purchasedUpgrades.Contains("Damage I"),
                (upgrade, entity, serverCall) =>
                {
                    if(!serverCall) entity.Stats.AddStatBonus(StatType.Multiplicative, Stat.Damage, 1.25f, "Damage II");
                }, UpgradeCategory.Offensive));

            _possibleUpgrades.Add(new UpgradeData(ObjectFinder.GetSprite("inventoryIcons_3"), "Damage III",
                "x1.25 to player damage stat",
                () => _purchasedUpgrades.Contains("Damage I") &&
                      _purchasedUpgrades.Contains("Damage II"),
                (upgrade, entity, serverCall) =>
                {
                    if(!serverCall) entity.Stats.AddStatBonus(StatType.Multiplicative, Stat.Damage, 1.25f, "Damage III");
                }, UpgradeCategory.Offensive));
            
            _possibleUpgrades.Add(new UpgradeData(ObjectFinder.GetSprite("inventoryIcons_11"), "Attack Speed I",
                "x1.15 to player attack speed stat",
                () => true,
                (upgrade, entity, serverCall) =>
                {
                    if(!serverCall) entity.Stats.AddStatBonus(StatType.Multiplicative, Stat.AttackSpeed, 1.15f, "Attack Speed I");
                }, UpgradeCategory.Offensive));

            _possibleUpgrades.Add(new UpgradeData(ObjectFinder.GetSprite("inventoryIcons_11"), "Attack Speed II",
                "x1.15 to player attack speed stat",
                () => _purchasedUpgrades.Contains("Attack Speed I"),
                (upgrade, entity, serverCall) =>
                {
                    if(!serverCall) entity.Stats.AddStatBonus(StatType.Multiplicative, Stat.AttackSpeed, 1.15f, "Attack Speed II");
                }, UpgradeCategory.Offensive));

            _possibleUpgrades.Add(new UpgradeData(ObjectFinder.GetSprite("inventoryIcons_11"), "Attack Speed III",
                "x1.15 to player attack speed stat",
                () => _purchasedUpgrades.Contains("Attack Speed I") &&
                      _purchasedUpgrades.Contains("Attack Speed II"),
                (upgrade, entity, serverCall) =>
                {
                    if(!serverCall) entity.Stats.AddStatBonus(StatType.Multiplicative, Stat.AttackSpeed, 1.15f, "Attack Speed III");
                }, UpgradeCategory.Offensive));
            
            _possibleUpgrades.Add(new UpgradeData(ObjectFinder.GetSprite("inventoryIcons_5"), "Better Regen",
                "x2 to player regen stat",
                () => true,
                (upgrade, entity, serverCall) =>
                {
                    if(!serverCall) entity.Stats.AddStatBonus(StatType.Multiplicative, Stat.HpRegen, 2f, "Better Regen");
                }, UpgradeCategory.Defensive));

            /*RegisterLocalUpgrade(1000, ObjectFinder.GetSprite("inventoryIcons_1"), "More Relics I",
                "Gain an extra relic slot.",
                () => true,
                (upgrade, entity) =>
                {
                    PlayerInventory.Instance.AddRelicSlot();
                }, UpgradeCategory.Utility);
            
            RegisterLocalUpgrade(3000, ObjectFinder.GetSprite("inventoryIcons_1"), "More Relics II",
                "Gain an extra relic slot.",
                () => _purchasedUpgrades.Contains("More Relics I"),
                (upgrade, entity) =>
                {
                    PlayerInventory.Instance.AddRelicSlot();
                }, UpgradeCategory.Utility);
            
            RegisterLocalUpgrade(5000, ObjectFinder.GetSprite("inventoryIcons_1"), "More Relics III",
                "Gain an extra relic slot.",
                () => _purchasedUpgrades.Contains("More Relics II"),
                (upgrade, entity) =>
                {
                    PlayerInventory.Instance.AddRelicSlot();
                }, UpgradeCategory.Utility);
            
            RegisterLocalUpgrade(7000, ObjectFinder.GetSprite("inventoryIcons_1"), "More Relics IV",
                "Gain an extra relic slot.",
                () => _purchasedUpgrades.Contains("More Relics III"),
                (upgrade, entity) =>
                {
                    PlayerInventory.Instance.AddRelicSlot();
                }, UpgradeCategory.Utility);
            
            RegisterLocalUpgrade(10000, ObjectFinder.GetSprite("inventoryIcons_1"), "More Relics V",
                "Gain an extra relic slot.",
                () => _purchasedUpgrades.Contains("More Relics IV"),
                (upgrade, entity) =>
                {
                    PlayerInventory.Instance.AddRelicSlot();
                }, UpgradeCategory.Utility);
            
            RegisterGlobalUpgrade(200, ObjectFinder.GetSprite("ballistaTurretUI"), "Tower Limit I",
                "Increase the tower limit by +1.",
                () => true,
                (upgrade, entity) =>
                {
                    GameManager.Instance.CmdSetTowerLimit(GameManager.Instance.MaxTowers+1);
                }, UpgradeCategory.Crystal);
            
            RegisterGlobalUpgrade(500, ObjectFinder.GetSprite("ballistaTurretUI"), "Tower Limit II",
                "Increase the tower limit by +1.",
                () => _purchasedUpgrades.Contains("Tower Limit I"),
                (upgrade, entity) =>
                {
                    GameManager.Instance.CmdSetTowerLimit(GameManager.Instance.MaxTowers+1);
                }, UpgradeCategory.Crystal);
            
            RegisterGlobalUpgrade(800, ObjectFinder.GetSprite("ballistaTurretUI"), "Tower Limit III",
                "Increase the tower limit by +1.",
                () => _purchasedUpgrades.Contains("Tower Limit II"),
                (upgrade, entity) =>
                {
                    GameManager.Instance.CmdSetTowerLimit(GameManager.Instance.MaxTowers+1);
                }, UpgradeCategory.Crystal);
            
            RegisterGlobalUpgrade(1000, ObjectFinder.GetSprite("ballistaTurretUI"), "Tower Limit IV",
                "Increase the tower limit by +1.",
                () => _purchasedUpgrades.Contains("Tower Limit III"),
                (upgrade, entity) =>
                {
                    GameManager.Instance.CmdSetTowerLimit(GameManager.Instance.MaxTowers+1);
                }, UpgradeCategory.Crystal);
            
            RegisterGlobalUpgrade(1500, ObjectFinder.GetSprite("ballistaTurretUI"), "Tower Limit V",
                "Increase the tower limit by +1.",
                () => _purchasedUpgrades.Contains("Tower Limit IV"),
                (upgrade, entity) =>
                {
                    GameManager.Instance.CmdSetTowerLimit(GameManager.Instance.MaxTowers+1);
                }, UpgradeCategory.Crystal);
            
            RegisterGlobalUpgrade(2000, ObjectFinder.GetSprite("ballistaTurretUI"), "Tower Limit VI",
                "Increase the tower limit by +1.",
                () => _purchasedUpgrades.Contains("Tower Limit V"),
                (upgrade, entity) =>
                {
                    GameManager.Instance.CmdSetTowerLimit(GameManager.Instance.MaxTowers+1);
                }, UpgradeCategory.Crystal);
            
            RegisterGlobalUpgrade(2000, ObjectFinder.GetSprite("ballistaTurretUI"), "Tower Limit VII",
                "Increase the tower limit by +1.",
                () => _purchasedUpgrades.Contains("Tower Limit VI"),
                (upgrade, entity) =>
                {
                    GameManager.Instance.CmdSetTowerLimit(GameManager.Instance.MaxTowers+1);
                }, UpgradeCategory.Crystal);
            
            RegisterGlobalUpgrade(2000, ObjectFinder.GetSprite("ballistaTurretUI"), "Tower Limit VIII",
                "Increase the tower limit by +1.",
                () => _purchasedUpgrades.Contains("Tower Limit VII"),
                (upgrade, entity) =>
                {
                    GameManager.Instance.CmdSetTowerLimit(GameManager.Instance.MaxTowers+1);
                }, UpgradeCategory.Crystal);*/

            #endregion
        }

        public void RollUpgrades(int count, bool replaceOld)
        {
            if (replaceOld) ClearUpgrades();

            for (var i = 0; i < count; i++)
            {
                var selected = GameTools.SelectRandom(_possibleUpgrades.Where(upgrade => upgrade != null && upgrade.IsPurchasable.Invoke()));
                if (selected != null)
                {
                    RegisterUpgrade(0, selected.Icon, selected.Name, selected.Description,
                        selected.IsPurchasable,
                        selected.OnPurchase, selected.Category, false, selected);
                }

                _possibleUpgrades.Remove(selected);
            }
        }

        private int
            _index = 0; // Index to create unique upgrade IDs. Note: This system requires that upgrades are registered in the same order on all clients.

        private void Update()
        {
            // Upon pressing 'Esc' or moving > 1 unit away, deactivate the UI

            if (!uiToggleObject.activeSelf) return; // Don't do redundant checks if already deactivated

            if (Keyboard.current[Key.Escape].wasPressedThisFrame || _moveSinceActive > 1f) // Check should deactivate
            {
                uiToggleObject.SetActive(false); // Deactivate
            }
        }

        private void QuickHideUpgrades()
        {
            foreach (var (_, value) in _createdEntries.ToList())
            {
                value.PurchasableFunc = () => false;
                //RemoveUpgrade(entry.Key);
            }
        }

        private void ClearUpgrades()
        {
            foreach (var entry in _createdEntries.ToList())
            {
                _possibleUpgrades.Add(entry.Value.OriginalData);
                RemoveUpgrade(entry.Key);
            }
        }

        [Command(requiresAuthority = false)] // Runs on the server
        public void
            CmdNotifyPurchaseGlobal(int id,
                GameEntity localPlayer) // Called in UpgradeEntry when a global purchase goes through.
        {
            if (!_onPurchaseActions.ContainsKey(id) || _onPurchaseActions[id] == null ||
                !_createdEntries.ContainsKey(id) || _createdEntries[id] == null)
            {
                //Debug.LogWarning("Trying to purchase an invalid upgrade!");
                return; // Check to prevent upgrades from being attempted to be bought twice or invalid upgrades. Prevent null pointers.
            }

            _onPurchaseActions[id]?.Invoke(_createdEntries[id], localPlayer, true); // Invoke the purchase method.

            RpcSetPurchased(
                id); // Notify to clients that the upgrade has been purchased so it can be removed from the UI
        }

        // Runs on the purchasing client
        public void NotifyPurchaseLocal(int id) // Called in UpgradeEntry when a local purchase goes through.
        {
            if (!_onPurchaseActions.ContainsKey(id) || _onPurchaseActions[id] == null ||
                !_createdEntries.ContainsKey(id) || _createdEntries[id] == null)
            {
                //Debug.LogWarning("Trying to purchase an invalid upgrade!");
                return; // Check to prevent upgrades from being attempted to be bought twice or invalid upgrades. Prevent null pointers.
            }

            _purchasedUpgrades.Add(_createdEntries[id].UpgradeName);

            _onPurchaseActions[id]?.Invoke(_createdEntries[id], Player.LocalInstance, false); // Invoke the purchase method.
            OnUpgradeLocal?.Invoke();

            //RemoveUpgrade(id); // Remove the upgrade locally
        }

        public void NotifyPurchase(int id)
        {
            NotifyPurchaseLocal(id);
            CmdNotifyPurchaseGlobal(id, Player.LocalInstance);
        }

        [ClientRpc] // Called by the server and runs on the client
        public void RpcSetPurchased(int id)
        {
            if (_createdEntries.ContainsKey(id))
            {
                _purchasedUpgrades.Add(_createdEntries[id]
                    .UpgradeName); // Add the upgrade's name to the list of purchased upgrades

                RemoveUpgrade(
                    id); // Remove the upgrade on purchase, TODO: May allow certain upgrades to be bought multiple times in the future
            }

            ClearUpgrades();
        }

        private void RemoveUpgrade(int id) // Handle all upgrade removal for a given id
        {
            if (!_createdEntries.ContainsKey(id)) return;
            
            Destroy(_createdEntries[id].gameObject); // Destroy this UI object

            // Remove corresponding dictionary entries
            _createdEntries.Remove(id);
            _onPurchaseActions.Remove(id);
        }

        /*
        private void RegisterLocalUpgrade(int scrapCost, Sprite icon, string upgradeName, string description,
            Func<bool> isPurchasable,
            Action<UpgradeEntry, GameEntity> onPurchaseClient, UpgradeCategory category) => RegisterUpgrade(scrapCost,
            icon, upgradeName,
            description, isPurchasable, onPurchaseClient, category, true);

        // Used instead of RegisterLocalUpgrade when the upgrade logic should only be called once and on the server. Ex. crystal upgrades
        private void RegisterGlobalUpgrade(int scrapCost, Sprite icon, string upgradeName, string description,
            Func<bool> isPurchasable,
            Action<UpgradeEntry, GameEntity> onPurchaseServer, UpgradeCategory category) => RegisterUpgrade(scrapCost,
            icon, upgradeName,
            description, isPurchasable, onPurchaseServer, category, false);
            */

        // Handle creation for an upgrade given supplied values
        private void RegisterUpgrade(int scrapCost, Sprite icon, string upgradeName, string description,
            Func<bool> isPurchasable,
            Action<UpgradeEntry, GameEntity, bool> onPurchase, UpgradeCategory category, bool local, UpgradeData data)
        {
            var id = _index; // Store corresponding ID
            _index++; // Increment to ensure unique ID next time

            // Add corresponding dictionary entries
            _onPurchaseActions.Add(id, onPurchase);
            _createdEntries.Add(id,
                CreateUIElement(scrapCost, icon, upgradeName, description, isPurchasable, id,
                    category, data)); // Create the UI element and add it to dictionary
        }

        // Handle instantiation of UI elements
        private UpgradeEntry CreateUIElement(int scrapCost, Sprite icon, string upgradeName, string description,
            Func<bool> isPurchasable, int callbackId, UpgradeCategory category, UpgradeData data)
        {
            var go = Instantiate(upgradeEntryPrefab,
                upgradeEntryParent.transform); // Instantiate UI object under UI transform

            var ue = go.GetComponent<UpgradeEntry>();
            ue.Set(scrapCost, icon, upgradeName, description, isPurchasable, callbackId,
                category, data); // Pass on values to the UpgradeEntry behavior

            return ue;
        }
    }
}