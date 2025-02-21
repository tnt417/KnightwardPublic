using System;
using System.Collections.Generic;
using System.Linq;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Enemies;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Global;
using UnityEngine;

namespace TonyDev
{
    public class LaserWeaponController : MonoBehaviour
    {
        public GameObject nodePrefab;
        public LineRenderer laserRenderer;
        public LineRenderer[] subLaserRenderers;
        public GameObject visualParent;
        public Animator animator;

        [NonSerialized] public bool IsOwner = false;
        
        private ParticleSystem[] _nodePool;

        public int maxNumBounces = 3;
        public float laserDist = 7f;
        public float subLaserDist = 4f;

        private void Start()
        {
            _nodePool = new ParticleSystem[maxNumBounces + 1 + subLaserRenderers.Length];
            
            for (var i = 0; i < _nodePool.Length; i++)
            {
                var node = Instantiate(nodePrefab, transform, true);
                var ps = node.GetComponent<ParticleSystem>();
                ps.Stop();
                _nodePool[i] = ps;
            }
        }

        private float _hitTimer = 0;
        private List<Enemy> _collidingEnemies = new List<Enemy>();
        private List<Enemy> _subLaserCollidingEnemies = new List<Enemy>();

        private Vector2 _lerpDirection;
        
        public void Update()
        {
            _hitTimer += Time.deltaTime;

            if (_hitTimer > 0.05 / Player.LocalInstance.Stats.GetStat(Stat.AttackSpeed))
            {
                _hitTimer = 0;
                foreach(var e in _collidingEnemies)
                {
                    e.CmdDamageEntity(0.15f * Player.LocalInstance.Stats.OutgoingDamage, Player.LocalInstance.Stats.CritSuccessful, null, false, DamageType.Contact);
                }
                foreach(var e in _subLaserCollidingEnemies)
                {
                    e.CmdDamageEntity(0.2f * Player.LocalInstance.Stats.OutgoingDamage, Player.LocalInstance.Stats.CritSuccessful, null, false, DamageType.Contact);
                }
            }
        }
        
        public void FixedUpdate()
        {
            if (!IsOwner) return;
            
            transform.position = Player.LocalInstance.transform.position;

            var mouseDir = GameManager.MouseDirectionHigh.normalized;
            
            _lerpDirection = Vector2.MoveTowards(_lerpDirection, mouseDir,
                Time.deltaTime * 7f * Mathf.Max(Vector2.Distance(_lerpDirection, mouseDir), 0.3f));
            
            for (var i = 0; i < _nodePool.Length; i++)
            {
                _nodePool[i].Stop();
            }
            
            if (Player.LocalInstance.CanAttack)
            {
                animator.SetBool("LaserActive", true);
                Player.LocalInstance.playerAnimator.attackingOverride = true;
            }
            else
            {
                animator.SetBool("LaserActive", false);
                Player.LocalInstance.playerAnimator.Shake(0);
                Player.LocalInstance.playerAnimator.attackingOverride = false;
                return;
            }

            Player.LocalInstance.playerAnimator.SetAttackAnimProgress(0.75f);
            Player.LocalInstance.playerAnimator.Shake(3f);

            var bounces = 0;
            var linePoints = new List<Vector3>();
            
            var laserRemaining = laserDist;
            
            var dir = _lerpDirection;
            var firePoint = gameObject.transform.position + new Vector3(0, 0.4f, 0);

            var testHit = Physics2D.Raycast(firePoint, dir,
                laserRemaining, LayerMask.GetMask("Enemy"));

            Debug.DrawRay(firePoint, dir * laserRemaining, Color.red);
            
            if (testHit.rigidbody != null)
            {
                var enemy = testHit.rigidbody.GetComponent<Enemy>();
                //Debug.Log("Hit: " + testHit.transform.gameObject.name + ", Enemy found: " + (enemy != null) );
            }
            
            linePoints.Add(firePoint);
            _nodePool[0].Play();
            _nodePool[0].transform.position = firePoint;
            
            _collidingEnemies.Clear();
            _subLaserCollidingEnemies.Clear();
            
            var hitMirror = false;
            MirrorController mirrorController = null;
            var mirrorHitPoint = Vector2.zero;
            
            while (bounces < maxNumBounces)
            {
                var mirrorCast = Physics2D.RaycastAll((Vector2)_nodePool[bounces].transform.position + dir * 0.05f, dir, laserRemaining, LayerMask.GetMask("Default"));

                var mirrorHit = mirrorCast.FirstOrDefault(hit => hit.transform.gameObject.CompareTag("StaffMirror"));

                if (mirrorHit.transform != null)
                {
                    hitMirror = true;
                    mirrorController = mirrorHit.transform.GetComponent<MirrorController>();
                    mirrorHitPoint = mirrorHit.point;
                    linePoints.Add(mirrorHitPoint);
                    _nodePool[bounces + 1].Play();
                    _nodePool[bounces + 1].transform.position = mirrorHit.point;
                }
                
                var hits = Physics2D.RaycastAll((Vector2)_nodePool[bounces].transform.position + dir * 0.05f, dir, laserRemaining, LayerMask.GetMask("Enemy", "Level"));
                
                var wallHit = hits.FirstOrDefault(h => h.transform.gameObject.layer == LayerMask.NameToLayer("Level"));
                
                foreach(var h in hits)
                {
                    if (h.transform == mirrorHit.transform) break;
                    if (h.transform == wallHit.transform) break;
                    if (h.rigidbody == null) continue;
                    
                    var enemy = h.rigidbody.GetComponent<Enemy>();
                    if (enemy != null)
                    {
                        _collidingEnemies.Add(enemy);
                    }
                }

                if (hitMirror)
                {
                    if(mirrorController != null) mirrorController.MirrorHit();
                    break;
                }
                
                if (wallHit.transform != null)
                {
                    dir = Vector2.Reflect(dir, wallHit.normal);
                    linePoints.Add(wallHit.point);
                    bounces++;
                    _nodePool[bounces].Play();
                    _nodePool[bounces].transform.position = wallHit.point;
                    laserRemaining -= wallHit.distance;
                }
                else
                {
                    var unhitPoint = linePoints[^1] + (Vector3) dir.normalized * laserRemaining;
                    
                    linePoints.Add(unhitPoint);
                    _nodePool[bounces+1].Play();
                    _nodePool[bounces+1].transform.position = unhitPoint;
                    break;
                }
            }

            if (hitMirror)
            {
                for (var i = 0; i < subLaserRenderers.Length; i++)
                {
                    subLaserRenderers[i].gameObject.SetActive(true);
                    
                    var newDir = GameTools.Rotate(dir, (i * 20 - 10) * Mathf.Deg2Rad);

                    var hits = Physics2D.RaycastAll(mirrorHitPoint + newDir * 0.05f, newDir,
                        subLaserDist, LayerMask.GetMask("Enemy", "Level"));

                    var wallHit =
                        hits.FirstOrDefault(h => h.transform.gameObject.layer == LayerMask.NameToLayer("Level"));

                    foreach (var h in hits)
                    {
                        if (h.transform == wallHit.transform) break;
                        if (h.rigidbody == null) continue;

                        var enemy = h.rigidbody.GetComponent<Enemy>();
                        if (enemy != null)
                        {
                            _subLaserCollidingEnemies.Add(enemy);
                        }
                    }

                    var subPoints = new List<Vector3>();

                    subPoints.Add(mirrorHitPoint);

                    if (wallHit.transform != null)
                    {
                        subPoints.Add(wallHit.point);
                        _nodePool[bounces + 2 + i].Play();
                        _nodePool[bounces + 2 + i].transform.position = wallHit.point;
                    }
                    else
                    {
                        var unhitPoint = subPoints[^1] + (Vector3) newDir.normalized * subLaserDist;

                        subPoints.Add(unhitPoint);
                        _nodePool[bounces + 2 + i].Play();
                        _nodePool[bounces + 2 + i].transform.position = unhitPoint;
                    }
                    
                    subLaserRenderers[i].positionCount = 2;
                    subLaserRenderers[i].SetPositions(subPoints.ToArray());
                }
            }
            else
            {
                for (var i = 0; i < subLaserRenderers.Length; i++)
                {
                    subLaserRenderers[i].gameObject.SetActive(false);
                }
            }

            for (var i = linePoints.Count + (hitMirror ? subLaserRenderers.Length : 0); i < _nodePool.Length; i++)
            {
                _nodePool[i].Stop();
            }

            laserRenderer.positionCount = linePoints.Count;
            laserRenderer.SetPositions(linePoints.ToArray());
        }
    }
}
