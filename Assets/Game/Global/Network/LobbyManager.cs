using System.Collections.Generic;
using System.Linq;
using Mirror;
using TMPro;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Global.Console;
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
        }

        private void Awake()
        {
            playButton.interactable = false;
        }

        public override void OnStartAuthority()
        {
            base.OnStartAuthority();

            playButton.interactable = true;
            playButton.onClick.AddListener(Play);
        }

        [Server]
        private void Play()
        {
            playButton.interactable = false;
            GameConsole.Log("Starting game...");

            foreach (var (key, value) in _playerTiles)
            {
                UsernameDict[key] = value.username;
            }

            CmdFinishLobby(UsernameDict.Keys.ToArray(), UsernameDict.Values.ToArray());

            OnPlay?.Invoke();
        }

        [Command(requiresAuthority = false)]
        private void CmdFinishLobby(int[] keys, string[] values)
        {
            RpcSetUsernames(keys, values);
            NetworkServer.Destroy(gameObject);
        }

        [ClientRpc]
        private void RpcSetUsernames(int[] keys, string[] values)
        {
            var newDict = new Dictionary<int, string>();
            for (var i = 0; i < keys.Length; i++)
            {
                newDict[keys[i]] = values[i];
            }
            UsernameDict = newDict;
        }

        [Server]
        public void OnNewPlayerConnected(NetworkConnectionToClient conn = null)
        {
            if (conn == null) return;

            var connId = conn.connectionId;

            foreach (var tileObject in _playerTiles.Select(pt => pt.Value.gameObject))
            {
                NetworkServer.Spawn(tileObject,
                    conn); //Instantiate all the tiles that were created before the new player connected.
                TargetOnNewTileCreated(conn, tileObject, connId);
            }

            var newTile = Instantiate(playerTilePrefab, playerGridTransform);

            NetworkServer.Spawn(newTile);

            newTile.GetComponent<NetworkIdentity>().AssignClientAuthority(conn);

            RpcOnNewTileCreated(newTile, connId); //Instantiate a tile for the newly joined player on all clients.

            _lobbyPlayerCount += 1;
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