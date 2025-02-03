using System;
using System.Linq;
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
using UnityEngine.UI;

namespace TonyDev.Game.Global.Network
{
    public class CustomNetworkManager : NetworkRoomManager
    {
        private const int Port = 7777;
        
        public bool TelepathyServer => transport is TelepathyTransport;

        public new void Start()
        {
            _lobbyCreated = false;
            maxConnections = SteamLobbyManager.MaxConnections;

            if (!SteamLobbyManager.IsSteamServer)
            {
                InitTelepathy();
            }
            else
            {
                InitFizzy();
            }

            base.Start();
        }

        private void InitTelepathy()
        {
            var telepathy = GetComponent<TelepathyTransport>();
            if (telepathy == null) telepathy = gameObject.AddComponent<TelepathyTransport>();

            telepathy.port = Port;

            transport = telepathy;
            Transport.active = telepathy;
        }
        
        private void InitFizzy()
        {
            var fizzy = GetComponent<FizzySteamworks>();
            if (fizzy == null) fizzy = gameObject.AddComponent<FizzySteamworks>();

            transport = fizzy;
            Transport.active = fizzy;
        }

        public override void OnClientDisconnect()
        {
            base.OnClientDisconnect();

            SteamLobbyManager.Singleton.LeaveLobby();
        }

        public override void OnServerReady(NetworkConnectionToClient conn)
        {
            base.OnServerReady(conn);

            if (SceneManager.GetActiveScene().name == GameplayScene && SteamLobbyManager.IsSteamServer)
            {
                SteamLobbyManager.Singleton.DisableJoins();
            }
        }

        private bool _lobbyCreated = false;

        public async UniTask CreateAndHost()
        {

            if (SteamLobbyManager.IsSteamServer)
            {
                SteamLobbyManager.Singleton.HostLobby();

                _lobbyCreated = false;

                SteamLobbyManager.Singleton.OnLobbyCreateSuccessful += OnLobbyCreateSuccessful;
                
                await UniTask.WaitUntil(() => _lobbyCreated);
            }
            else
            {
                networkAddress = "localhost";
                OnLobbyCreateSuccessful();
            }
        }

        private void OnLobbyCreateSuccessful()
        {
            _lobbyCreated = true;
            //Debug.Log("Starting host!");
            StartHost();

            if(SteamLobbyManager.IsSteamServer) SteamLobbyManager.Singleton.OnLobbyCreateSuccessful -= OnLobbyCreateSuccessful;

            if (GameManager.IsDemo)
            {
                maxConnections = 1;
                //ServerChangeScene(GameplayScene);
            }
        }

        public override void OnStartHost()
        {
            base.OnStartHost();
            if (GameManager.IsDemo)
            {
                //ServerChangeScene(GameplayScene);
            }
        }

        public override void OnRoomServerPlayersReady()
        {
            if (GameManager.IsDemo || GameManager.QuickTestMode)
            {
                ServerChangeScene(GameplayScene);
            }
        }

        [Server]
        public void StartGame()
        {
            CustomRoomPlayer.Local.RpcStartFadeOut();
            StartGameTask().Forget();
        }

        private async UniTask StartGameTask()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(TransitionController.FadeOutTimeSeconds));
            ServerChangeScene(GameplayScene);
        }

        public void ConnectToAddress(string address)
        {
            networkAddress = address;
            StartClient();
        }

        private void OnConnectedToServer()
        {
            if(!SteamLobbyManager.IsSteamServer) SteamLobbyManager.Singleton.OnLobbyEntered(default);
        }
    }
}