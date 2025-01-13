using System;
using System.Collections.Generic;
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

        private CustomRoomPlayer[] _Crp;
        
        private void Awake()
        {
            _Crp = FindObjectsByType<CustomRoomPlayer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        }

        private void FixedUpdate()
        {
            dungeonFloorText.text = GameManager.DungeonFloor + (RoomGenerator.Config.floorAtLaunchOffset != 0 ? " + " + RoomGenerator.Config.floorAtLaunchOffset : "");
            numPlayersText.text = _Crp.Where(crp => crp != null).Max(crp => crp.playerNumber).ToString();
        }
    }
}
