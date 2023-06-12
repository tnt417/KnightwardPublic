using System;
using TMPro;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Global.Network;
using UnityEngine;

namespace TonyDev.Game.UI.Lobby
{
    public class UsernameUIController : MonoBehaviour
    {
        private Player _player;
        public TMP_Text usernameLabel;

        private void Awake()
        {
            _player = GetComponentInParent<Player>();
            _player.OnUsernameChange += SetUsername;
        }

        private void SetUsername(string username)
        {
            usernameLabel.text = _player.isOwned ? "" : username;
        }
    }
}
