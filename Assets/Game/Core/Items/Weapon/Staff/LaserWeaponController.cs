using System;
using System.Collections;
using System.Collections.Generic;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Global;
using UnityEngine;

namespace TonyDev
{
    public class LaserWeaponController : MonoBehaviour
    {
        public GameObject nodePrefab;
        public LineRenderer laserRenderer;
        public GameObject visualParent;

        [NonSerialized] public bool IsOwner = false;
        
        private GameObject[] _nodePool;

        public int maxNumBounces = 3;
        public float laserDist = 7f;

        private void Start()
        {
            _nodePool = new GameObject[maxNumBounces + 1];
            
            for (var i = 0; i < maxNumBounces + 1; i++)
            {
                var node = Instantiate(nodePrefab, visualParent.transform, true);
                node.SetActive(false);
                _nodePool[i] = node;
            }
        }

        public void Update()
        {
            if (!IsOwner) return;
            
            transform.position = Player.LocalInstance.transform.position;

            if (Player.LocalInstance.CanAttack)
            {
                visualParent.SetActive(true);
                Player.LocalInstance.playerAnimator.attackingOverride = true;
            }
            else
            {
                visualParent.SetActive(false);
                Player.LocalInstance.playerAnimator.Shake(0);
                Player.LocalInstance.playerAnimator.attackingOverride = false;
                return;
            }

            Player.LocalInstance.playerAnimator.SetAttackAnimProgress(0.75f);
            Player.LocalInstance.playerAnimator.Shake(3f);
            
            var dir = GameManager.MouseDirection;
            var firePoint = gameObject.transform.position + new Vector3(0, 0.5f, 0);
            
            var bounces = 0;
            var linePoints = new List<Vector3>();
            
            linePoints.Add(firePoint);
            _nodePool[0].SetActive(true);
            _nodePool[0].transform.position = firePoint;
            
            while (bounces < maxNumBounces)
            {
                var hit = Physics2D.Raycast(firePoint, dir, laserDist, LayerMask.GetMask("Level"));

                if (hit.collider != null)
                {
                    dir = Vector2.Reflect(dir, hit.normal);
                    linePoints.Add(hit.point);
                    bounces++;
                    _nodePool[bounces].SetActive(true);
                    _nodePool[bounces].transform.position = hit.point;
                }
                else
                {
                    var unhitPoint = linePoints[^1] + (Vector3) dir.normalized * laserDist;
                    
                    linePoints.Add(unhitPoint);
                    _nodePool[bounces+1].SetActive(true);
                    _nodePool[bounces+1].transform.position = unhitPoint;
                    break;
                }
            }

            for (var i = linePoints.Count - 1; i < _nodePool.Length; i++)
            {
                _nodePool[i].SetActive(false);
            }

            laserRenderer.positionCount = linePoints.Count;
            laserRenderer.SetPositions(linePoints.ToArray());
        }
    }
}
