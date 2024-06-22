using System;
using System.Linq;
using TMPro;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Enemies;
using TonyDev.Game.Global;
using TonyDev.Game.Level.Rooms;
using UnityEngine;

namespace TonyDev.Game.UI.GameInfo
{
    public class PhaseInformationUIController : MonoBehaviour
    {
        [SerializeField] private TMP_Text dungeonFloorText;

        private void FixedUpdate()
        {
            dungeonFloorText.text = "Dungeon Floor: " + GameManager.DungeonFloor + (RoomGenerator.Config.floorAtLaunchOffset != 0 ? " + " + RoomGenerator.Config.floorAtLaunchOffset : "");
        }
    }
}
