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
        private List<Enemy> _suckList = new();

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
            foreach (var e in _suckList)
            {
                e.transform.position = Vector2.MoveTowards(e.transform.position, transform.position,
                    suckSpeed * Time.fixedDeltaTime * (10f-Vector2.Distance(e.transform.position, transform.position)));
            }
        }
    }
}
