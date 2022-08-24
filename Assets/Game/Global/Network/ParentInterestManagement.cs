using System.Collections.Generic;
using Mirror;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Level.Rooms;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

namespace TonyDev.Game.Global.Network
{
    public class ParentInterestManagement : InterestManagement
    {
        [Tooltip("Rebuild all every 'rebuildInterval' seconds.")]
        public float rebuildInterval = 1;

        private double _lastRebuildTime;

        [ServerCallback]
        private void Update()
        {
            // rebuild all spawned NetworkIdentity's observers every interval
            if (NetworkTime.time >= _lastRebuildTime + rebuildInterval)
            {
                RebuildAll();
                _lastRebuildTime = NetworkTime.time;
            }
        }

        [ServerCallback]
        public void ForceRebuild()
        {
            RebuildAll();
        }

        public override bool OnCheckObserver(NetworkIdentity identity, NetworkConnectionToClient newObserver)
        {
            var identityParentRoom = identity.GetComponent<Room>();
            var identityParentEntity = identity.GetComponent<GameEntity>();

            if (identityParentEntity == null && identityParentRoom == null)
                return true; //If identity is not an entity or room, make it visible.
            if (identityParentEntity != null && identityParentEntity.currentParentIdentity == null)
                return true; //If identity has no parent identity, can see them

            var observerParentEntity = newObserver.identity.GetComponent<GameEntity>();
            if (observerParentEntity == null) return true; //If observer is not an entity, make it see everything.

            if (identityParentRoom != null)
            {
                return identityParentRoom.netIdentity ==
                       observerParentEntity
                           .currentParentIdentity; //If observing a room, check if room is the observer's current room.
            }

            return identityParentEntity.currentParentIdentity ==
                   observerParentEntity.currentParentIdentity; //Otherwise, check if the parents are equal.
        }

        public override void OnRebuildObservers(NetworkIdentity identity,
            HashSet<NetworkConnectionToClient> newObservers)
        {
            // for each connection
            foreach (NetworkConnectionToClient conn in NetworkServer.connections.Values)
                // if authenticated and joined the world
                if (conn != null && conn.isAuthenticated && conn.identity != null)
                    // check if other identity has the same parent identity as us.
                    if (OnCheckObserver(identity, conn))
                        newObservers.Add(conn);
        }

        [ServerCallback]
        public override void SetHostVisibility(NetworkIdentity identity, bool visible)
        {
            foreach (var rend in identity.GetComponentsInChildren<Renderer>())
                rend.enabled = visible;
            foreach (var img in identity.GetComponentsInChildren<Image>())
                img.enabled = visible;
            foreach (var l2d in identity.GetComponentsInChildren<Light2DBase>())
                l2d.enabled = visible;
            foreach (var coll in identity.GetComponentsInChildren<Collider2D>())
                coll.enabled = visible;
            foreach (var roomDoor in identity.GetComponentsInChildren<RoomDoor>())
                roomDoor.SetHostVisibility(visible);

            var entity = identity.GetComponent<GameEntity>();
            if (entity != null) entity.visibleToHost = visible;
        }
    }
}