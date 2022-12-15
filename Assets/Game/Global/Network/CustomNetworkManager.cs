using System;
using System.Linq;
using System.Text;
using Cysharp.Threading.Tasks;
using kcp2k;
using Mirror;
using Mirror.FizzySteam;
using Steamworks;
using TMPro;
using TonyDev.Game.Global.Console;
using TonyDev.Game.Level;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace TonyDev.Game.Global.Network
{
    public class CustomNetworkManager : NetworkManager
    {
        [Header("Custom")]
        //[SerializeField] private GameObject lobbyManagerPrefab;

        private LobbyManager _lobbyManager;

        private TMP_InputField _ipAddressInputField;

        // Callbacks
        protected Callback<LobbyCreated_t> LobbyCreated;
        protected Callback<GameLobbyJoinRequested_t> JoinRequest;
        protected Callback<LobbyEnter_t> LobbyEntered;

        // Variables
        public static ulong CurrentLobbyID;
        private const string HostAddressKey = "HostAddress";
        
        [Header("UI Objects")]
        [SerializeField] private GameObject connectUIObject;
        [SerializeField] private Button joinButton;
        [SerializeField] private Button hostButton;

        private void OnReset()
        {
            joinButton.interactable = true;
            hostButton.interactable = true;
            
            connectUIObject.SetActive(false);

            _playerSpawnEventCalled = false;
            CurrentLobbyID = default;
            
            GameManager.ResetGame();
        }

        private bool _beenInitialized = false;

        public override void OnClientDisconnect()
        {
            base.OnClientDisconnect();

            SceneManager.LoadScene("GameOver");
            
            LeaveLobby();
        }
        
        private new void Start()
        {
            if (_beenInitialized) return;

            OnReset();

            if (!SteamManager.Initialized) return;

            _ipAddressInputField = GetComponentInChildren<TMP_InputField>();

            LobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
            JoinRequest = Callback<GameLobbyJoinRequested_t>.Create(OnJoinRequest);
            LobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);

            _beenInitialized = true;
            
            // Check if we should connect to a lobby
            var args = Environment.GetCommandLineArgs();
            
            for (var i = 0; i < args.Length; i++)
            {
                if (args[i] != "+connect_lobby") continue;
                
                var idRaw = args[i + 1];

                var id = ulong.Parse(idRaw);
                    
                SteamMatchmaking.JoinLobby(new CSteamID(id));
                break;
            }
        }

        private void LeaveLobby()
        {
            Debug.Log("Leaving lobby...");
            
            SteamMatchmaking.LeaveLobby(new CSteamID(CurrentLobbyID));
            
            CurrentLobbyID = default;
            
            OnReset();
        }

        // public override void OnDestroy()
        // {
        //     base.OnDestroy();
        //     Debug.Log("Destroyed!");
        // }

        private void FixedUpdate()
        {
            connectUIObject.SetActive(SceneManager.GetSceneAt(0).name == "LobbyScene");
        }

        private void OnLobbyCreated(LobbyCreated_t callback)
        {
            if (callback.m_eResult != EResult.k_EResultOK)
            {
                hostButton.interactable = true;
                return;
            }
            
            Debug.Log("Lobby created successfully");
            
            StartHost();

            SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAddressKey, SteamUser.GetSteamID().ToString());
            SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), "name",
                SteamFriends.GetPersonaName() + "'s Lobby");
        }
        
        private void OnJoinRequest(GameLobbyJoinRequested_t callback)
        {
            Debug.Log("Request to join lobby.");
            SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
        }
        
        private void OnLobbyEntered(LobbyEnter_t callback)
        {
            joinButton.interactable = false;
            
            CurrentLobbyID = callback.m_ulSteamIDLobby; 
            if(GameConsole.Exists) GameConsole.Log("Entered lobby: " + SteamMatchmaking.GetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), "name"));

            if (NetworkServer.active) return;
            
            SceneManager.LoadScene("LobbyScene");

            networkAddress = SteamMatchmaking.GetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAddressKey);

            StartClient();
        }

        private void InitializeSteam()
        {
            var fizzy = gameObject.AddComponent<FizzySteamworks>();
            transport = fizzy;
        }

        private void InitializeKcp()
        {
            var kcp = gameObject.AddComponent<KcpTransport>();
            transport = kcp;
        }

        private static bool AllClientsReady => NetworkServer.connections.Values.All(conn => conn.isReady);
        private static bool AllPlayersSpawned => NetworkServer.connections.Values.All(conn => conn.identity != null);
        public static Action OnAllPlayersSpawned;
        private static bool _playerSpawnEventCalled;

        private void Update()
        {
            if (!_playerSpawnEventCalled && AllPlayersSpawned)
            {
                _playerSpawnEventCalled = true;
                OnAllPlayersSpawned?.Invoke();
            }
        }

        public static bool ReadyToStart;

        [Server]
        public override void OnServerConnect(NetworkConnectionToClient conn)
        {
            if (SceneManager.GetActiveScene().name == "CastleScene")
            {
                ReadyToStart = AllClientsReady;
                return;
            }

            if (_lobbyManager == null)
            {
                if(_lobbyManager == null) _lobbyManager = FindObjectOfType<LobbyManager>();

                _lobbyManager.OnNewPlayerConnected(conn).Forget();

                _lobbyManager.OnPlay += () =>
                {
                    NetworkServer.maxConnections = numPlayers;
                    ServerChangeScene("CastleScene");
                    SteamMatchmaking.SetLobbyJoinable(new CSteamID(CurrentLobbyID), false);
                };
            }
            else
            {
                _lobbyManager.OnNewPlayerConnected(conn).Forget();
            }
        }

        public override void OnClientChangeScene(string newSceneName, SceneOperation sceneOperation, bool customHandling)
        {
            if (newSceneName == "CastleScene")
            {
                TransitionController.Instance.FadeIn();
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

            _lobbyManager.OnPlayerLeave(conn);
        }

        public void JoinLobby() //Called when the Join button is pressed
        {
            //StartClient(new Uri("kcp://" + _ipAddressInputField.text + ":7777"));
            SteamFriends.ActivateGameOverlay("Friends");
        }

        public void HostLobby() //Called when the Host button is pressed
        {
            hostButton.interactable = false;
            SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, maxConnections);
        }
    }
}