using System;
using System.Linq;
using Mirror;
using TMPro;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Enemies;
using TonyDev.Game.Global;
using TonyDev.Game.Global.Network;
using TonyDev.Game.Level.Rooms;
using UnityEngine;

namespace TonyDev.Game.UI.GameInfo
{
    public class PhaseInformationUIController : MonoBehaviour
    {
        [SerializeField] private TMP_Text dungeonFloorText;
        [SerializeField] private TMP_Text numPlayersText;

        private void FixedUpdate()
        {
            dungeonFloorText.text = GameManager.DungeonFloor + (RoomGenerator.Config.floorAtLaunchOffset != 0 ? " + " + RoomGenerator.Config.floorAtLaunchOffset : "");
            numPlayersText.text = NetworkManager.singleton.numPlayers.ToString();
        }
    }
}
