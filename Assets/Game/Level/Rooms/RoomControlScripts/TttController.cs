using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Mirror;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Enemies;
using TonyDev.Game.Core.Entities.Player;
using UnityEngine;

namespace TonyDev.Game.Level.Rooms.RoomControlScripts
{
    [Serializable]
    public class TttArea
    {
        public Rect detectionArea;
        public LevelItemSpawner itemSpawner;
        [NonSerialized] public GameObject SpawnedPrefab;
    }

    public class TttController : NetworkBehaviour
    {
        private Room _room;
        private IEnumerable<GameEntity> _tttEnemies;

        public TttArea[] tttAreas;

        public GameObject claimedPrefab;

        private async UniTask ExecuteBehavior()
        {
            await UniTask.WaitUntil(() =>
            {
                _room = RoomManager.Instance.GetRoomFromID(netId);
                return _room != null &&
                       _room.ContainedEntities.Any(e => e is Enemy);
            }); //Wait until entities are spawned.

            _tttEnemies = _room.ContainedEntities.Where(e => e is Enemy);

            foreach (var ge in _tttEnemies)
            {
                ge.OnDeathOwner += (_) =>
                {
                    if (this == null) return;
                    SetTile(ge.transform.position, true);
                };
            }
        }

        private void SetTile(Vector2 entityPos, bool claimed)
        {
            var relativePos = entityPos - (Vector2) transform.position;

            var grantedArea = tttAreas.FirstOrDefault(area => area.detectionArea.Contains(relativePos));

            if (grantedArea == null) return;

            CmdSetTile(grantedArea.detectionArea.center, claimed);
        }

        [Command(requiresAuthority = false)]
        private void CmdSetTile(Vector2 pos, bool claimed)
        {
            RpcSetTile(pos, claimed);
        }

        [ClientRpc]
        private void RpcSetTile(Vector2 pos, bool claimed)
        {
            TttArea first = null;
            foreach (var area in tttAreas)
            {
                if (area.detectionArea.Contains(pos))
                {
                    first = area;
                    break;
                }
            }

            if (first == null) return;

            if (claimed)
            {
                if (first.SpawnedPrefab != null) return;

                first.SpawnedPrefab =
                    Instantiate(claimedPrefab, (Vector2) transform.position + pos, Quaternion.identity, transform);
            }
            else
            {
                if (first.SpawnedPrefab != null) Destroy(first.SpawnedPrefab);
                first.SpawnedPrefab = null;
            }
        }

        public void GrantRewards()
        {
            GrantTask().Forget();
        }

        private async UniTask GrantTask()
        {
            await UniTask.WaitForEndOfFrame(this);

            foreach (var area in tttAreas)
            {
                if (area.SpawnedPrefab == null) continue;

                area.itemSpawner.SpawnItem();
            }

            CmdOnGrantRewards();
        }

        [Command(requiresAuthority = false)]
        private void CmdOnGrantRewards()
        {
            RpcOnGrantRewards();
        }

        [ClientRpc]
        private void RpcOnGrantRewards()
        {
            foreach (var area in tttAreas)
            {
                if (area.SpawnedPrefab == null) continue;

                Destroy(area.SpawnedPrefab);
            }
        }

        public override void OnStartClient()
        {
            Player.LocalInstance.OnLocalHurt += () =>
            {
                if (this == null) return;
                SetTile(Player.LocalInstance.transform.position, false);
            };
        }

        public override void OnStartServer()
        {
            var src = new CancellationTokenSource();
            src.RegisterRaiseCancelOnDestroy(this);

            ExecuteBehavior().AttachExternalCancellation(src.Token);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0f, 1f, 0.7f);

            if (tttAreas == null) return;

            foreach (var a in tttAreas)
            {
                Gizmos.DrawCube(a.detectionArea.center, new Vector3(a.detectionArea.width, a.detectionArea.height, 1));
            }
        }
    }
}