using Mirror;
using TonyDev.Game.Core.Items;
using TonyDev.Game.Global;
using TonyDev.Game.Global.Network;
using UnityEngine;
using UnityEngine.Events;

namespace TonyDev.Game.Level.Decorations.Chests
{
    public class Chest : NetworkBehaviour, IHideable
    {
        [SerializeField] public int rarityBoost;
        [SerializeField] private Animator chestAnimator;
        [SerializeField] private ParticleSystem openParticles;

        public UnityEvent onOpenServer;
        public UnityEvent onOpenGlobal;

        [SyncVar(hook=nameof(OpenHook))] public bool opened;

        private void OpenHook(bool oldState, bool newState)
        {
            chestAnimator.SetBool("open", newState);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player") && other.isTrigger)
            {
                var id = other.GetComponent<NetworkIdentity>();
                
                if (id == null || !id.isLocalPlayer) return;
                
                CmdStartOpen(); //Drop items when player touches the chest
            }
        }

        public void AnimationEventOpen()
        {
            openParticles.Play();
            if (!isServer) return;
            DropItems();
        }

        [Command(requiresAuthority = false)]
        private void CmdStartOpen()
        {
            if (opened) return;
            opened = true;
            RpcSetOpen();
        }

        [ClientRpc]
        private void RpcSetOpen()
        {
            onOpenGlobal?.Invoke();
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
