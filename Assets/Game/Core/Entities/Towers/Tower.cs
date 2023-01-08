using System;
using System.Linq;
using Mirror;
using TonyDev.Game.Core.Attacks;
using TonyDev.Game.Core.Entities.Enemies;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Core.Items;
using TonyDev.Game.Global;
using TonyDev.Game.Level.Decorations;
using TonyDev.Game.Level.Decorations.Crystal;
using UnityEditor;
using UnityEngine;

namespace TonyDev.Game.Core.Entities.Towers
{
    public class Tower : GameEntity
    {
        //Editor variables
        [SerializeField] protected TowerAnimator towerAnimator;
        [SerializeField] public float targetRadius;
        //

        [HideInInspector] [SyncVar] public Item myItem;

        [Command(requiresAuthority = false)]
        public void CmdSetTowerItem(Item newItem)
        {
            myItem = newItem;
        }

        private new void Awake()
        {
            base.Awake();
            
            _interactableButton = gameObject.AddComponent<InteractableButton>();
            _interactableButton.onInteract.AddListener((type) =>
            {
                if (this != null && type == InteractType.Interact) CmdRequestPickup();
            });
        }
        
        private InteractableButton _interactableButton;

        public void NotifyMouseHover()
        {
            _interactableButton.overrideCurrent = true;
            _interactableButton.Active = true;
        }

        public void NotifyMouseUnhover()
        {
            _interactableButton.overrideCurrent = false;
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
                    Stats.AddStatBonus(sb.statType, sb.stat, sb.strength, myItem.itemName);
                }
            }

            foreach (var ge in myItem.itemEffects)
            {
                CmdAddEffect(ge, this);
            }
        }

        public override Team Team => Team.Player;
        public override bool IsInvulnerable => true;
        public override bool IsTangible => false;

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