using System;
using Mirror;
using TonyDev.Game.Core.Items;
using TonyDev.Game.Global;
using TonyDev.Game.Global.Console;
using TonyDev.Game.Global.Network;
using TonyDev.Game.Level.Rooms;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

namespace TonyDev.Game.Level.Decorations.Chests
{
    public class Chest : NetworkBehaviour, IHideable
    {
        [SerializeField] public int rarityBoost;
        [SerializeField] private Animator chestAnimator;

        public UnityEvent onOpenServer;

        [SyncVar] public bool opened;

        private void Start()
        {
            if (opened)
            {
                chestAnimator.Play("ChestOpen");
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player") && other.isTrigger)
            {
                var id = other.GetComponent<NetworkIdentity>();
                
                if (id == null || !id.isLocalPlayer) return;
                
                CmdDropItems(); //Drop items when player touches the chest
            }
        }

        [Command(requiresAuthority = false)]
        private void CmdDropItems()
        {
            if (opened) return;
            opened = true;
            DropItems();
            RpcSetOpen();
        }

        [ClientRpc]
        private void RpcSetOpen()
        {
            chestAnimator.Play("ChestOpen");
        }

        private Item _item = null;
        
        public void SetItem(Item newItem)
        {
            _item = newItem;
        }

        [Server]
        private void DropItems()
        {
            var dropItem = _item ?? ItemGenerator.GenerateItem(rarityBoost);

            //Spawn the item and change the chest sprite
            ObjectSpawner.SpawnGroundItem(dropItem, 0, transform.position, CurrentParentIdentity);
            onOpenServer.Invoke();
            //
        }

        public NetworkIdentity CurrentParentIdentity { get; set; }
    }
}
