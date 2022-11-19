using System;
using System.Linq;
using Mirror;
using TonyDev.Game.Core.Attacks;
using TonyDev.Game.Core.Entities.Enemies;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Core.Items;
using TonyDev.Game.Global;
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

        protected void Start()
        {
            Init();

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

        public void Pickup()
        {
            Die();
            GameManager.Instance.OccupiedTowerSpots.Remove(new Vector2Int((int)transform.position.x, (int)transform.position.y));
            NetworkServer.Destroy(gameObject);
            PlayerInventory.Instance.InsertItem(myItem);
        }
    }
}