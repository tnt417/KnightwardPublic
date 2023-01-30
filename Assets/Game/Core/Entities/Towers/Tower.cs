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
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TonyDev.Game.Core.Entities.Towers
{
    public class Tower : GameEntity
    {
        //Editor variables
        [SerializeField] protected TowerAnimator towerAnimator;
        [SerializeField] public float targetRadius;
        //

        [HideInInspector] [SyncVar] public Item myItem;

        [HideInInspector] [SyncVar(hook=nameof(DurabilityHook))] public int durability;
        
        [Command(requiresAuthority = false)]
        public void CmdSetTowerItem(Item newItem)
        {
            myItem = newItem;
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
            _interactableButton.AddInteractKey(Key.R, InteractType.Repair);
            _interactableButton.onInteract.AddListener((type) =>
            {
                Debug.Log("Inteact");
                if (this != null && type == InteractType.Interact) CmdRequestPickup();
                if (this != null && type == InteractType.Repair)
                {
                    if (GameManager.Essence >= RepairCost)
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
        }

        public const string DurabilityNegationSource = "DurabilityNegated";

        private void DurabilityHook(int oldDur, int newDur)
        {
            SetBrokenMaterial(durability <= 0);
        }

        [Server]
        public void SubtractDurability(int amount)
        {
            var sb = myItem.statBonuses.ToList();
            sb.Add(new StatBonus(StatType.Flat, Stat.Health, -amount, DurabilityNegationSource, true));
            
            myItem.statBonuses = sb.ToArray();

            CmdSetDurability(durability - amount);
        }

        [Command(requiresAuthority = false)]
        private void CmdSetDurability(int dur)
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
                sr.material.mainTexture = sr.sprite.texture;
            }
        }

        public void NotifyMouseUnhover()
        {
            _interactableButton.overrideCurrent = false;
            Interactable.Current = null;
            _interactableButton.Active = false;
        }

        protected void Start()
        {
            Init();

            var coll = gameObject.AddComponent<BoxCollider2D>();
            coll.size = new Vector2(0.2f, 0.2f);
            coll.isTrigger = true;

            if (!EntityOwnership) return;

            if (myItem.statBonuses != null)
            {
                foreach (var sb in myItem.statBonuses)
                {
                    Stats.AddStatBonus(sb.statType, sb.stat, sb.strength, myItem.itemName, sb.hidden);
                }
            }

            durability = MaxHealth;

            foreach (var ge in myItem.itemEffects)
            {
                CmdAddEffect(ge, this);
            }
        }

        public override Team Team => Team.Player;
        public override bool IsInvulnerable => true;
        public override bool IsTangible => false;
        public float MaxDurability => myItem.statBonuses.First(sb => sb.stat == Stat.Health && sb.source != DurabilityNegationSource).strength;
        
        [Command(requiresAuthority = false)]
        public void CmdRequestRepair(NetworkConnectionToClient sender = null)
        {
            if (_pickupPending || this == null) return;

            if (MaxDurability < 1000000 && durability < MaxDurability)
            {
                TargetConfirmRepair(sender, RepairCost);
                SubtractDurability((int)-Mathf.Clamp(durability + MaxDurability * 0.25f, 0, MaxDurability-durability));
            }
        }
        
        [TargetRpc]
        private void TargetConfirmRepair(NetworkConnection target, int cost)
        {
            GameManager.Essence -= cost;
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