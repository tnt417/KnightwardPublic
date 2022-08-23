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

        public delegate void LobbyEvent();

        public event LobbyEvent OnPlay;

        private void OnPlayerCountChange(int oldValue, int newValue)
        {
            playerCountText.text = newValue + "/4";
        }

        private void Awake()
        {
            GameConsole.Log("Re-awoke");
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

            /*NetworkServer.SendToAll(new SceneMessage()
            {
               sceneName = "CastleScene",
               sceneOperation = SceneOperation.Normal
            });

            SceneManager.LoadScene("CastleScene");*/
            
            NetworkServer.UnSpawn(gameObject);
            Destroy(gameObject);

            OnPlay?.Invoke();
        }

        [Server]
        public void OnNewPlayerConnected(NetworkConnectionToClient conn = null)
        {
            if (conn == null) return;

            var connId = conn.connectionId;

            GameConsole.Log($"Adding player {connId} to the lobby.");

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