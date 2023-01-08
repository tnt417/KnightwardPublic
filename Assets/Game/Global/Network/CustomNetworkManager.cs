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
        public new void Start()
        {
            _lobbyCreated = false;
            maxConnections = SteamLobbyManager.MaxConnections;
            
            base.Start();
        }
        
        public override void OnClientDisconnect()
        {
            base.OnClientDisconnect();

            SteamLobbyManager.Singleton.LeaveLobby();
        }

        public override void OnServerReady(NetworkConnectionToClient conn)
        {
            base.OnServerReady(conn);

            if (IsSceneActive(GameplayScene))
            {
                SteamLobbyManager.Singleton.DisableJoins();
            }
        }

        private bool _lobbyCreated = false;
        
        public async UniTask CreateAndHost()
        {
            networkAddress = "localhost";
            
            SteamLobbyManager.Singleton.HostLobby();
            
            _lobbyCreated = false;

            SteamLobbyManager.Singleton.OnLobbyCreateSuccessful += OnLobbyCreateSuccessful;

            await UniTask.WaitUntil(() => _lobbyCreated);
        }

        private void OnLobbyCreateSuccessful()
        {
            _lobbyCreated = true;
            Debug.Log("Starting host!");
            StartHost();

            SteamLobbyManager.Singleton.OnLobbyCreateSuccessful -= OnLobbyCreateSuccessful;
        }

        public override void OnRoomServerPlayersReady()
        {
            
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
    }
}