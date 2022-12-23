using System;
using System.Collections;
using System.Collections.Generic;
using System.Security;
using Mirror;
using TonyDev.Game.Core.Attacks;
using TonyDev.Game.Core.Entities.Enemies;
using UnityEngine;

namespace TonyDev
{
    public class SuckZone : MonoBehaviour
    {
        private readonly HashSet<Enemy> _suckList = new();

        public float suckSpeed;
        
        [ServerCallback]
        private void OnTriggerEnter2D(Collider2D other)
        {
            var enemy = other.GetComponent<Enemy>();
            
            if (enemy == null || enemy.Team != Team.Enemy) return;
            
            _suckList.Add(enemy);
        }
        
        [ServerCallback]
        private void OnTriggerExit2D(Collider2D other)
        {
            var enemy = other.GetComponent<Enemy>();
            
            if (enemy == null || enemy.Team != Team.Enemy) return;

            _suckList.Remove(enemy);
        }

        private void FixedUpdate()
        {
            var myPos = transform.position;
            
            _suckList.RemoveWhere(e => e == null);
            
            foreach (var e in _suckList)
            {
                var enemyPos = e.transform.position;
                enemyPos = Vector2.MoveTowards(enemyPos, myPos,
                    suckSpeed * Time.fixedDeltaTime);
                e.transform.position = enemyPos;
            }
        }
    }
}
