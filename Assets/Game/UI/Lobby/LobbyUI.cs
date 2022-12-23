using System;
using System.Collections.Generic;
using Mirror;
using Steamworks;
using TMPro;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Global.Network;
using UnityEngine;
using UnityEngine.UI;

namespace TonyDev
{
    public class LobbyUI : MonoBehaviour
    {
        public Transform playerObjectLayout;
        public TMP_Text lobbyName;
        public Button lobbyButton;
        public Button inviteButton;

        private List<GameObject> _children;

        private CustomNetworkManager _netManager;

        private void Start()
        {
            _netManager = NetworkManager.singleton as CustomNetworkManager;
            lobbyName.text = SteamLobbyManager.Singleton.GetLobbyName();
            lobbyButton.interactable = false;

            lobbyButton.onClick.AddListener(StartGame);
            inviteButton.onClick.AddListener(SteamLobbyManager.Singleton.PromptInvite);
        }

        private void StartGame()
        {
            _netManager.StartGame();
        }

        [ServerCallback]
        private void Update()
        {
            lobbyButton.interactable = _netManager.allPlayersReady;
        }

        public void AddPlayerObject(Transform t)
        {
            t.SetParent(playerObjectLayout);
        }
    }
}
