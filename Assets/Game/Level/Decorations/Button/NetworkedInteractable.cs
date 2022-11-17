using System;
using Mirror;
using UnityEngine;
using UnityEngine.Events;

namespace TonyDev.Game.Level.Decorations.Button
{
    public class NetworkedInteractable : NetworkBehaviour
    {
        [SerializeField] private Interactable interactable;

        public UnityEvent<InteractType> onInteractGlobal = new ();
        public UnityEvent<InteractType> onInteractServer = new ();

        private void Awake()
        {
            if (interactable == null)
            {
                Debug.LogWarning("Please set the interactable before awake!");
                Destroy(this);
            }

            interactable.onInteract.AddListener(BroadcastInteract);
        }

        public void BroadcastInteract(InteractType type)
        {
            CmdBroadcastInteract(type);
        }
        
        [Command(requiresAuthority = false)]
        public void CmdBroadcastInteract(InteractType type)
        {
            onInteractServer?.Invoke(type);
            RpcBroadcastInteract(type);
        }

        [ClientRpc]
        public void RpcBroadcastInteract(InteractType type)
        {
            onInteractGlobal?.Invoke(type);
        }

        public void DestroyInteractableObjectAll()
        {
            CmdDestroyInteractable();
        }

        [Command(requiresAuthority = false)]
        private void CmdDestroyInteractable()
        {
            RpcDestroyInteractable();
        }

        [ClientRpc]
        private void RpcDestroyInteractable()
        {
            if (interactable != null && interactable.gameObject != null) Destroy(interactable.gameObject);
            else this.enabled = false;
        }
    }
}
