using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TonyDev.Game.Core.Attacks;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Global;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TonyDev
{
    public class HomingMovement : MonoBehaviour
    {
        public float startForce;
        public float range;
        public float homeSpeed;
        //public float steeringStrength;

        private Transform _target;
        public Team targetTeam;

        private Rigidbody2D _rb2d;
        
        private void Start()
        {
            _rb2d = GetComponent<Rigidbody2D>();

            if (_rb2d == null)
            {
                Debug.LogWarning("Please add a rigidbody!");
                Destroy(this);
                return;
            }

            UpdateTarget();

            if (_target == null) return;
            
            _rb2d.AddForce(Vector2.Perpendicular(transform.position - _target.transform.position).normalized * (Random.Range(0, 1f) > 0.5f ? -1 : 1) * startForce);
            
            // var angle = Random.Range(0, 360);
            //
            // var radAngle = angle * Mathf.Deg2Rad;
            // var dir = new Vector2(Mathf.Cos(radAngle), Mathf.Sin(radAngle));
            //
            // _rb2d.AddForce(dir * startForce);
        }

        private float _targetTimer;
        
        private void FixedUpdate()
        {
            if (_target == null)
            {
                _targetTimer += Time.fixedDeltaTime;
                
                if (_targetTimer > GameEntity.EntityTargetUpdatingRate)
                {
                    UpdateTarget();
                    _targetTimer = 0;
                }
            }

            if (_target == null) return;
            
            _rb2d.AddForce((_target.transform.position - transform.position).normalized * homeSpeed * Time.fixedDeltaTime, ForceMode2D.Impulse);
        }

        private void UpdateTarget()
        {
            _target = GameManager.EntitiesReadonly.Where(ge => ge.Team == targetTeam)
                .OrderBy(ge => Vector2.Distance(transform.position, ge.transform.position)).FirstOrDefault()
                ?.transform;

            if (_target != null && Vector2.Distance(_target.position, transform.position) > range) _target = null;
        }
    }
}
