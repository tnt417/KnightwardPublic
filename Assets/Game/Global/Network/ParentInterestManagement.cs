using System.Collections.Generic;
using Mirror;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Enemies;
using TonyDev.Game.Core.Entities.Player;
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
                ForceRebuild();
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
            if (identity == newObserver.identity) return true;
            
            var identityParentRoom = identity.GetComponent<Room>();
            var identityParentHideable = identity.GetComponent<IHideable>();

            if (identityParentHideable == null && identityParentRoom == null)
            {
                return true; //If identity is not hideable or a room, make it visible.
            }

            if (identityParentHideable != null && identityParentHideable.CurrentParentIdentity == null)
            {
                return true; //If identity has no parent identity, can see them
            }

            var observerParentHideable = newObserver.identity.GetComponent<IHideable>();
            if (observerParentHideable == null)
            {
                return true; //If observer is not a hideable(entity), make it see everything.
            }

            if (identityParentRoom != null)
            {
                return identityParentRoom.netIdentity ==
                       observerParentHideable
                           .CurrentParentIdentity; //If observing a room, check if room is the observer's current room.
            }

            var visible = identityParentHideable == null || observerParentHideable.CurrentParentIdentity ==
                identityParentHideable
                    .CurrentParentIdentity;

            return visible; //Otherwise, check if the parents are equal.
        }

        public override void OnRebuildObservers(NetworkIdentity identity, HashSet<NetworkConnectionToClient> newObservers)
        {
            // for each connection
            foreach (var conn in NetworkServer.connections.Values)
            {
                // if not authenticated or not joined the world, don't rebuild
                if (conn == null || !conn.isAuthenticated || conn.identity == null)
                {
                    continue;
                }

                // check observer (checks if other identity has the same parent identity as us)
                var visible = OnCheckObserver(identity, conn);
                if(visible) newObservers.Add(conn);
                
                //Don't set host visibility according to the observing of clients
                if (conn.identity != NetworkClient.localPlayer) continue;
                
                SetHostVisibility(identity, visible); //Set host visibility ourselves
            }
        }

        [ServerCallback]
        public override void SetHostVisibility(NetworkIdentity identity, bool visible)
        {
            if (identity.GetComponent<Room>() != null) return;

            var entity = identity.GetComponent<GameEntity>();
            if (entity != null) entity.VisibleToHost = visible;

            foreach (var rend in identity.GetComponentsInChildren<Renderer>())
                rend.enabled = visible;
            foreach (var img in identity.GetComponentsInChildren<Image>())
                img.enabled = visible;
            foreach (var l2d in identity.GetComponentsInChildren<Light2DBase>())
                l2d.enabled = visible;
            //foreach (var coll in identity.GetComponentsInChildren<Collider2D>())
            //    coll.enabled = visible;
            foreach (var roomDoor in identity.GetComponentsInChildren<RoomDoor>())
                roomDoor.SetHostVisibility(visible);
        }
    }
}