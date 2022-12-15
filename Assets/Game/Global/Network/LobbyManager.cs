using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Mirror;
using Steamworks;
using TMPro;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Global.Console;
using TonyDev.Game.Level;
using TonyDev.Game.UI.Lobby;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TonyDev.Game.Global.Network
{
    public class LobbyManager : NetworkBehaviour
    {
        [SerializeField] private GameObject playerTilePrefab;
        [SerializeField] private Transform playerGridTransform;
        [SerializeField] private TMP_Text playerCountText;
        [SerializeField] private Button playButton;
        [SerializeField] private GameObject playerPrefab;

        [SyncVar(hook = nameof(OnPlayerCountChange))]
        private int _lobbyPlayerCount;

        private readonly Dictionary<int, ConnectedPlayerTile> _playerTiles = new();

        public static Dictionary<int, string> UsernameDict = new();
        
        public delegate void LobbyEvent();

        public event LobbyEvent OnPlay;

        private void OnPlayerCountChange(int oldValue, int newValue)
        {
            playerCountText.text = newValue + "/4";
            SteamFriends.SetRichPresence("status", $"In lobby. ({newValue}/{NetworkManager.singleton.maxConnections})");
        }


        public override void OnStartServer()
        {
            playButton.onClick.AddListener(Play);
            playButton.interactable = true;
        }

        private void Awake()
        {
            playButton.interactable = false;
        }

        [Server]
        private void Play()
        {
            playButton.interactable = false;

            foreach (var (key, value) in _playerTiles)
            {
                UsernameDict[key] = value.username;
            }
            
            Debug.Log("Calling Cmd!");
            
            CmdBroadcastUsernames(UsernameDict.Keys.ToArray(), UsernameDict.Values.ToArray());
            
            PlayTask().Forget();
        }
        
        [Server]
        private async UniTask PlayTask()
        {
            GameConsole.Log("Starting game...");
            
            CmdFadeOut();

            await UniTask.Delay(TimeSpan.FromSeconds(TransitionController.FadeOutTimeSeconds));

            await UniTask.Delay(TimeSpan.FromSeconds(1f));
            
            //OnPlay?.Invoke();
        }

        [Command(requiresAuthority = false)]
        private void CmdFadeOut()
        {
            RpcFadeOut();
        }
        
        [ClientRpc(includeOwner = true)]
        private void RpcFadeOut()
        {
            TransitionController.Instance.FadeOut();
        }

        [Command(requiresAuthority = false)]
        public void CmdBroadcastUsernames(int[] keys, string[] values)
        {
            Debug.Log("Cmd!");
            RpcSetUsernames(keys, values);
        }

        [ClientRpc(includeOwner = true)]
        private void RpcSetUsernames(int[] keys, string[] values)
        {
            Debug.Log("Rpc!");
            var newDict = new Dictionary<int, string>();
            for (var i = 0; i < keys.Length; i++)
            {
                newDict[keys[i]] = values[i];
            }
            UsernameDict = newDict;
        }

        [Server]
        public async UniTask OnNewPlayerConnected(NetworkConnectionToClient conn)
        {
            if (conn == null) return;

            await UniTask.DelayFrame(1);

            var connId = conn.connectionId;

            foreach (var tileObject in _playerTiles.Select(pt => pt.Value.gameObject))
            {
                NetworkServer.Spawn(tileObject,
                    conn); //Instantiate all the tiles that were created before the new player connected.
                TargetOnNewTileCreated(conn, tileObject, connId);
            }

            var newTile = Instantiate(playerTilePrefab, playerGridTransform);

            NetworkServer.Spawn(newTile, conn);

            var tile = newTile.GetComponent<ConnectedPlayerTile>();
            
            tile.netIdentity.AssignClientAuthority(conn);
            
            await UniTask.DelayFrame(1);

            RpcOnNewTileCreated(newTile, connId); //Instantiate a tile for the newly joined player on all clients.

            _lobbyPlayerCount += 1;
        }

        [Server]
        public void OnPlayerLeave(NetworkConnectionToClient conn)
        {
            if (conn == null) return;
            
            NetworkServer.Destroy(_playerTiles[conn.connectionId].gameObject);
            _lobbyPlayerCount -= 1;
        }

        [TargetRpc]
        private void TargetOnNewTileCreated(NetworkConnection target, GameObject tileObject, int connId)
        {
            tileObject.transform.SetParent(playerGridTransform);

            _playerTiles[connId] = tileObject.GetComponent<ConnectedPlayerTile>();
        }

        [ClientRpc]
        private void RpcOnNewTileCreated(GameObject tileObject, int connId)
        {
            tileObject.transform.SetParent(playerGridTransform);

            _playerTiles[connId] = tileObject.GetComponent<ConnectedPlayerTile>();
        }
    }
}