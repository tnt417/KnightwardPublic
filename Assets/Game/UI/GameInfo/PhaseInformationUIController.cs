using System;
using System.Linq;
using TMPro;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Enemies;
using TonyDev.Game.Global;
using UnityEngine;

namespace TonyDev.Game.UI.GameInfo
{
    public class PhaseInformationUIController : MonoBehaviour
    {
        [SerializeField] private TMP_Text dungeonFloorText;

        private void FixedUpdate()
        {
            dungeonFloorText.text = "Dungeon Floor: " + GameManager.DungeonFloor;
        }
    }
}
