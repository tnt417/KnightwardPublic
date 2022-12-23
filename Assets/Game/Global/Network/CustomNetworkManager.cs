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
            base.Start();
            maxConnections = SteamLobbyManager.MaxConnections;
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

        public async UniTask CreateAndHost()
        {
            SteamLobbyManager.Singleton.HostLobby();
            
            var lobbyCreated = false;

            SteamLobbyManager.Singleton.OnLobbyCreateSuccessful += () =>
            {
                lobbyCreated = true;
                StartHost();
            };

            await UniTask.WaitUntil(() => lobbyCreated);
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