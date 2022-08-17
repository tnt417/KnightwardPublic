using System;
using Mirror;
using TMPro;
using UnityEngine;

namespace TonyDev.Game.UI.Lobby
{
    public class ConnectedPlayerTile : NetworkBehaviour
    {
        [SyncVar(hook=nameof(UsernameHook))] public string username;
        
        public TMP_InputField usernameTextField;

        public void Awake()
        {
            usernameTextField.readOnly = true;
        }

        public override void OnStartAuthority()
        {
            usernameTextField.readOnly = false;
            usernameTextField.onEndEdit.AddListener(FinishEditUsername);
        }

        private void UsernameHook(string oldValue, string newValue)
        {
            usernameTextField.text = newValue;
        }

        private void FinishEditUsername(string newUsername)
        {
            if (hasAuthority)
            {
                CmdUpdateUsername(newUsername);
            }
        }

        [Command(requiresAuthority = false)]
        private void CmdUpdateUsername(string newUsername)
        {
            username = newUsername;
        }
    }
}
