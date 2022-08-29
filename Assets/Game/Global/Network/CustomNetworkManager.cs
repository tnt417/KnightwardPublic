using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Mirror;
using TMPro;
using TonyDev.Game.Global.Console;
using TonyDev.Game.Level.Rooms;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TonyDev.Game.Global.Network
{
    public class CustomNetworkManager : NetworkManager
    {
        [Header("Custom")] [SerializeField] private GameObject connectUIObject;
        [SerializeField] private GameObject lobbyManagerPrefab;
        [SerializeField] private GameObject networkedManagersPrefab;

        private LobbyManager _lobbyManager;

        private TMP_InputField _ipAddressInputField;

        private new void Awake()
        {
            base.Awake();

            _ipAddressInputField = GetComponentInChildren<TMP_InputField>();

            SceneManager.sceneLoaded += (arg0, sceneMode) => { GameConsole.Log("Scene loaded: " + arg0.name); };
        }

        [Client]
        public override void OnClientConnect()
        {
            base.OnClientConnect();

            connectUIObject.SetActive(false);
        }

        private static bool AllClientsReady => NetworkServer.connections.Values.All(conn => conn.isReady);
        private static bool AllPlayersSpawned => NetworkServer.connections.Values.All(conn => conn.identity != null);
        public static Action OnAllPlayersSpawned;
        private static bool playerSpawnEventCalled = false;

        private void Update()
        {
            if (!playerSpawnEventCalled && AllPlayersSpawned)
            {
                playerSpawnEventCalled = true;
                OnAllPlayersSpawned?.Invoke();
            }
        }

        [Server]
        public override void OnServerReady(NetworkConnectionToClient conn)
        {
            base.OnServerReady(conn);
            
            GameConsole.Log($"Player {conn.connectionId} ready!");

            if (SceneManager.GetActiveScene().name == "CastleScene") return;

            if (_lobbyManager == null)
            {
                var lobbyManagerObject = Instantiate(lobbyManagerPrefab);

                _lobbyManager = lobbyManagerObject.GetComponent<LobbyManager>();

                NetworkServer.Spawn(_lobbyManager.gameObject, conn);
                
                _lobbyManager.netIdentity.AssignClientAuthority(conn);

                _lobbyManager.OnNewPlayerConnected(conn);
                
                _lobbyManager.OnPlay += () =>
                {
                    NetworkServer.maxConnections = numPlayers;
                    ServerChangeScene("CastleScene");
                };
            }
            else
            {
                NetworkServer.Spawn(_lobbyManager.gameObject, conn);

                _lobbyManager.OnNewPlayerConnected(conn);
            }
        }

        public override void OnClientSceneChanged()
        {
            if (SceneManager.GetActiveScene().name == "CastleScene")
            {
                NetworkClient.Ready();
                NetworkClient.AddPlayer();
            }
            
            base.OnClientSceneChanged();
        }

        public override void OnServerDisconnect(NetworkConnectionToClient conn)
        {
            base.OnServerDisconnect(conn);

            if (SceneManager.GetActiveScene().name == "CastleScene")
            {
                //Don't allow players to reconnect to the castle scene.
                NetworkServer.maxConnections--;
            }
        }

        public void JoinLobby() //Called when the Join button is pressed
        {
            StartClient(new Uri("kcp://" + _ipAddressInputField.text + ":7777"));
        }

        public void HostLobby() //Called when the Host button is pressed
        {
            StartHost();
        }
    }
}