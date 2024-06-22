using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using TonyDev.Game.Level.Rooms;
using UnityEngine;
using UnityEngine.Serialization;

namespace TonyDev
{
    public enum MapZone
    {
        Basic, Desert, Ice
    }

    [Serializable]
    public struct MapTheme
    {
        public int startFloor;
        public MapZone zone;
        [Header("Room Counts")]
        public int uncommonAmount;
        public int specialAmount;
        public Vector2Int roomAmountRange;
        [FormerlySerializedAs("roomEntires")] [Header("Rooms")]
        public RoomEntry[] roomEntries;
    }
    
    [CreateAssetMenu(fileName="MapGenConfig", menuName = "Map Gen Config")]
    public class MapGenConfig : ScriptableObject
    {
        public MapTheme[] mapZones;
        public int loopPoint;
        
        [Description("Defaults to 0. Should only be used for debug purposes.")]
        public int floorAtLaunchOffset = 0;
    }
}
