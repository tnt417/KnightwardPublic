using System;
using System.Linq;
using Mirror;
using TonyDev.Game.Core.Attacks;
using TonyDev.Game.Core.Effects;
using TonyDev.Game.Core.Entities.Enemies;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Core.Items;
using TonyDev.Game.Global;
using TonyDev.Game.Level.Decorations;
using TonyDev.Game.Level.Decorations.Crystal;
using TonyDev.Game.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TonyDev.Game.Core.Entities.Towers
{
    public class Tower : GameEntity
    {
        //Editor variables
        [SerializeField] protected TowerAnimator towerAnimator;
        //[SerializeField] public float targetRadius;
        //

        [HideInInspector] [SyncVar] public Item myItem;

        [HideInInspector] [SyncVar(hook=nameof(DurabilityHook))] public int durability;

        private bool _itemSet = false;
        
        [Command(requiresAuthority = false)]
        public void CmdSetTowerItem(Item newItem)
        {
            if (_itemSet)
            {
                Debug.LogWarning("Set item called twice!");
                return;
            }
            
            Stats.ReadOnly = false;
            
            _itemSet = true;
            
            myItem = newItem;
            
            if (myItem.statBonuses != null)
            {
                foreach (var sb in myItem.statBonuses)
                {
                    Stats.AddStatBonus(sb.statType, sb.stat, sb.strength, myItem.itemName, sb.hidden);
                }
            }

            foreach (var ge in myItem.itemEffects)
            {
                AddEffect(ge, this);
            }
            
            durability = (int)Stats.GetStat(Stat.Health);
            SetNetworkHealth(durability, MaxHealth);
        }

        private new void Awake()
        {
            base.Awake();

            _brokenBlock = new MaterialPropertyBlock();
            _brokenBlock.SetColor("_OutlineColor", Color.red);
            _brokenBlock.SetFloat("_OutlineGlowIntensity", 0.7f);

            _normalBlock = new MaterialPropertyBlock();
            _normalBlock.SetColor("_OutlineColor", Color.black);
            _normalBlock.SetFloat("_OutlineGlowIntensity", 0f);

            _interactableButton = gameObject.AddComponent<InteractableButton>();
            _interactableButton.SetInteractKey(Key.R, InteractType.Repair);
            _interactableButton.onInteract.AddListener((type) =>
            {
                if (this != null && type == InteractType.Interact) CmdRequestPickup();
                if (this != null && type == InteractType.Repair)
                {
                    if (GameManager.Money >= RepairCost)
                    {
                        CmdRequestRepair();
                    }
                    else
                    {
                        ObjectSpawner.SpawnTextPopup(Player.Player.LocalInstance.transform.position, "Insufficient essence!", Color.red, 0.3f);
                    }
                }
            });
        }

        private int RepairCost => Mathf.CeilToInt(Mathf.Clamp(MaxDurability - durability, 0, MaxDurability * 0.25f) / MaxDurability * 100);
        
        private InteractableButton _interactableButton;

        public void NotifyMouseHover()
        {
            _interactableButton.overrideCurrent = true;
            Interactable.Current = _interactableButton;
            _interactableButton.Active = true;
            Indicator.Instance.UpdateCurrentInteractable(_interactableButton);
        }

        public const string DurabilityNegationSource = "DurabilityNegated";

        private bool _initialDurabilitySet = false;
        
        private void DurabilityHook(int oldDur, int newDur)
        {
            if (_initialDurabilitySet)
            {
                var sb = myItem.statBonuses.ToList();
                sb.Add(new StatBonus(StatType.Flat, Stat.Health, newDur - oldDur, DurabilityNegationSource, true));

                myItem.statBonuses = sb.ToArray();
            }

            _initialDurabilitySet = true;

            SetBrokenMaterial(durability <= 0);
        }

        [Server]
        public void SubtractDurability(int amount)
        {
            CmdSetDurability(durability - amount);
        }

        [Command(requiresAuthority = false)]
        protected void CmdSetDurability(int dur)
        {
            durability = Math.Clamp(dur, 0, (int)MaxDurability);

            if (durability <= 0)
            {
                Stats.AddStatBonus(StatType.Multiplicative, Stat.AttackSpeed, 0, "Dead", true);
            }
            else
            {
                Stats.RemoveStatBonuses("Dead");
            }
        }

        private MaterialPropertyBlock _brokenBlock;
        private MaterialPropertyBlock _normalBlock;
        
        private void SetBrokenMaterial(bool broken)
        {
            foreach (var sr in GetComponentsInChildren<SpriteRenderer>())
            {
                sr.SetPropertyBlock(broken ? _brokenBlock : _normalBlock);
                sr.material.mainTexture = sr.sprite != null ? sr.sprite.texture : null;
            }
        }

        public void NotifyMouseUnhover()
        {
            _interactableButton.overrideCurrent = false;
            Interactable.Current = null;
            _interactableButton.Active = false;
            Indicator.Instance.UpdateCurrentInteractable(null);
        }

        public new void Start()
        {
            base.Start();
            
            var coll = gameObject.AddComponent<BoxCollider2D>();
            coll.size = new Vector2(0.2f, 0.2f);
            coll.isTrigger = true;

            SetBrokenMaterial(false);
        }

        public override void OnStartAuthority()
        {
            base.OnStartAuthority();

            OnHealthChangedOwner += HpChangedHook;
        }

        public override void OnStopAuthority()
        {
            base.OnStopAuthority();
            
            OnHealthChangedOwner -= HpChangedHook;

            if (Interactable.Current == _interactableButton)
            {
                Indicator.Instance.UpdateCurrentInteractable(null);
            }
        }

        private void HpChangedHook(float newHp)
        {
            CmdSetDurability((int)newHp);
        }

        public override Team Team => Team.Player;
        public override bool IsInvulnerable => true;
        public override bool IsTangible => false;
        public int MaxDurability => (int) myItem.statBonuses.FirstOrDefault(sb => sb.stat == Stat.Health && sb.source != DurabilityNegationSource).strength;
        public override int MaxHealth => MaxDurability;
        
        public const int InfiniteDurabilityThreshold = 1000000;
        
        [Command(requiresAuthority = false)]
        public void CmdRequestRepair(NetworkConnectionToClient sender = null)
        {
            if (_pickupPending || this == null) return;

            if (MaxDurability < InfiniteDurabilityThreshold && durability < MaxDurability)
            {
                TargetConfirmRepair(sender, RepairCost);
                SubtractDurability((int)-Mathf.Clamp(MaxDurability * 0.25f, 0, MaxDurability-durability));
            }
        }
        
        [TargetRpc]
        private void TargetConfirmRepair(NetworkConnection target, int cost)
        {
            GameManager.Money -= cost;
            ObjectSpawner.SpawnTextPopup(Player.Player.LocalInstance.transform.position, "Repaired!", Color.white, 0.3f);
        }
        
        [Command(requiresAuthority = false)]
        public void CmdRequestPickup(NetworkConnectionToClient sender = null)
        {
            if (_pickupPending || this == null) return;
            
            _pickupPending = true;
            
            TargetConfirmPickup(sender);
        }

        private bool _pickupPending = false;
        
        [TargetRpc]
        private void TargetConfirmPickup(NetworkConnection target)
        {
            PlayerInventory.Instance.InsertItem(myItem);
            CmdConfirmPickup();
        }

        [Command(requiresAuthority = false)]
        private void CmdConfirmPickup()
        {
            GameManager.Instance.OccupiedTowerSpots.Remove(new Vector2Int((int)transform.position.x, (int)transform.position.y));
            Die();
        }
    }
}