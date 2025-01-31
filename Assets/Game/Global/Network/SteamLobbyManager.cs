using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using Cysharp.Threading.Tasks;
using kcp2k;
using Mirror;
using Mirror.FizzySteam;
using Steamworks;
using TMPro;
using TonyDev.Game.Global;
using TonyDev.Game.Global.Console;
using TonyDev.Game.Global.Network;
using TonyDev.Game.Level;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TonyDev
{
    public class SteamLobbyManager : MonoBehaviour
    {
        public static SteamLobbyManager Singleton;

        private bool _isSteamServer = false;
        public static bool IsSteamServer => Singleton._isSteamServer && !GameManager.IsDemo;
        
        public const int MaxConnections = 4;

        private void Awake()
        {
            if (Singleton != null)
            {
                return;
            }
            
            if (NetworkManager.singleton is CustomNetworkManager { TelepathyServer: false })
            {
                _isSteamServer = true;
            }

            Singleton = this;
        }

        public Action OnLobbyCreateSuccessful;

        // Callbacks
        protected Callback<LobbyCreated_t> LobbyCreated;
        protected Callback<GameLobbyJoinRequested_t> JoinRequest;
        protected Callback<LobbyEnter_t> LobbyEntered;

        // Variables
        public static ulong CurrentLobbyID;
        private const string HostAddressKey = "HostAddress";

        private void OnReset()
        {
            CurrentLobbyID = default;
        }

        public string GetLobbyName()
        {
            return _isSteamServer ? SteamMatchmaking.GetLobbyData(new CSteamID(CurrentLobbyID), "name") : "Lobby";
        }

        private void Start()
        {
            //if (_beenInitialized) return;

            OnReset();
            
            if (!_isSteamServer || !SteamManager.Initialized) return;

            LobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
            JoinRequest = Callback<GameLobbyJoinRequested_t>.Create(OnJoinRequest);
            LobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);

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

        public void LeaveLobby()
        {
            //Debug.Log("Leaving lobby...");

            if(_isSteamServer) SteamMatchmaking.LeaveLobby(new CSteamID(CurrentLobbyID));

            CurrentLobbyID = default;

            OnReset();
        }

        private void OnLobbyCreated(LobbyCreated_t callback)
        {
            if (callback.m_eResult != EResult.k_EResultOK)
            {
                return;
            }

            OnLobbyCreateSuccessful?.Invoke();

            //Debug.Log("Lobby created successfully");

            SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAddressKey,
                SteamUser.GetSteamID().ToString());
            SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), "name",
                SteamFriends.GetPersonaName() + "'s Lobby");
        }

        private void OnJoinRequest(GameLobbyJoinRequested_t callback)
        {
            SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
        }

        public void OnLobbyEntered(LobbyEnter_t callback)
        {
            CurrentLobbyID = IsSteamServer ? callback.m_ulSteamIDLobby : default;

            if (SceneManager.GetActiveScene().name != "LobbyScene") SceneManager.LoadScene("LobbyScene");

            GameConsole.Log("Entered lobby: " + (IsSteamServer ? SteamMatchmaking.GetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), "name") : "Lobby"));

            if (NetworkServer.active || !IsSteamServer) return;

            var netManager = NetworkManager.singleton as CustomNetworkManager;

            if (netManager != null) netManager.ConnectToAddress(ConnectAddress);
        }

        public string ConnectAddress => SteamMatchmaking.GetLobbyData(new CSteamID(CurrentLobbyID), HostAddressKey);

        public void DisableJoins()
        {
            if (_isSteamServer)
            {
                SteamMatchmaking.SetLobbyJoinable(new CSteamID(CurrentLobbyID), false);
            }
        }

        public void ActivateJoinOverlay() //Called when the Join button is pressed
        {
            if (_isSteamServer)
            {
                SteamFriends.ActivateGameOverlay("Friends");
            }
        }

        public void HostLobby() //Called when the Host button is pressed
        {
            if (_isSteamServer)
            {
                SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, MaxConnections);
            }
        }

        public void PromptInvite()
        {
            if (_isSteamServer)
            {
                SteamFriends.ActivateGameOverlayInviteDialog(new CSteamID(CurrentLobbyID));
            }
        }
    }
}