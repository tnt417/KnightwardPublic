using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Mirror;
using TMPro;
using UnityEngine;

namespace TonyDev.Game.Global.Network
{
    public class CustomNetworkManager : NetworkManager
    {
        [Header("Custom")] [SerializeField] private GameObject connectUIObject;
        [SerializeField] private GameObject lobbyManagerPrefab;

        private LobbyManager _lobbyManager;

        private TMP_InputField _ipAddressInputField;

        private new void Awake()
        {
            base.Awake();

            _ipAddressInputField = GetComponentInChildren<TMP_InputField>();
        }

        [Client]
        public override void OnClientConnect()
        {
            base.OnClientConnect();

            connectUIObject.SetActive(false);
        }

        [Server]
        public override void OnServerReady(NetworkConnectionToClient conn)
        {
            base.OnServerReady(conn);

            Debug.Log($"Player {conn.connectionId} ready!");

            if (_lobbyManager == null)
            {
                var lobbyManagerObject = Instantiate(lobbyManagerPrefab);

                _lobbyManager = lobbyManagerObject.GetComponent<LobbyManager>();

                NetworkServer.Spawn(_lobbyManager.gameObject, conn);
                
                _lobbyManager.netIdentity.AssignClientAuthority(conn);

                _lobbyManager.OnNewPlayerConnected(conn);

                _lobbyManager.OnPlay += SpawnPlayers;
            }
            else
            {
                NetworkServer.Spawn(_lobbyManager.gameObject, conn);

                _lobbyManager.OnNewPlayerConnected(conn);
            }
        }

        public void SpawnPlayers()
        {
            foreach (var connection in NetworkServer.connections.Values)
            {
                OnServerAddPlayer(connection);
            }
        }
        

        public void JoinLobby()
        {
            StartClient(new Uri("kcp://" + _ipAddressInputField.text + ":7777"));
        }

        public void HostLobby()
        {
            StartHost();
        }
    }
}