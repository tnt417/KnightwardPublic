using System.Collections.Generic;
using TonyDev.Game.Core.Attacks;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Global;
using UnityEngine;

namespace TonyDev.Game.Core.Effects
{
    public class HammerSpinEffect : AbilityEffect
    {
        public ProjectileData DetachProjectile;
        public ProjectileData AttachedProjectile;
        public float MaxHammers;
        public float SpinSpeed; //In cycles per second
        public float SpinRadius;
        
        private float HammerRespawnTime => ModifiedCooldown / MaxHammers;

        private double _hammerRespawnTimer;

        public override void OnRemoveOwner()
        {
            DestroyAllHammers();
            base.OnRemoveOwner();
        }

        public override void OnUpdateOwner()
        {
            base.OnUpdateOwner();
            
            if (Time.time > _hammerRespawnTimer && _hammers.Count < MaxHammers)
            {
                _hammerRespawnTimer = Time.time + HammerRespawnTime;

                SpawnHammer();
            }

            for (int i = 0; i < _hammers.Count; i++)
            {
                var go = _hammers[i];
                go.transform.localPosition = new Vector2(Mathf.Cos(Time.time*SpinSpeed + (float) i / _hammers.Count * 2 * Mathf.PI),
                    Mathf.Sin(Time.time*SpinSpeed + (float) i / _hammers.Count * 2 * Mathf.PI)) * SpinRadius;
                go.transform.GetChild(0).transform.up = Vector2.Perpendicular((Vector2) go.transform.localPosition - Vector2.zero);
            }
        }

        protected override void OnAbilityActivate()
        {
            base.OnAbilityActivate();

            foreach (var go in _hammers)
            {
                var dir = (GameManager.MainCamera.ScreenToWorldPoint(Input.mousePosition) - go.transform.position).normalized;
                
                ObjectSpawner.SpawnProjectile(Entity, go.transform.position, dir,
                    DetachProjectile);
            }

            DestroyAllHammers();
            
            _hammerRespawnTimer = Time.time + HammerRespawnTime;
            _hammers.Clear();
        }

        private void DestroyAllHammers()
        {
            foreach (var go in _hammers)
            {
                GameManager.Instance.CmdDestroyProjectile(go.GetComponent<AttackComponent>().identifier);
                Object.Destroy(go);
            }
            
            _hammerRespawnTimer = Time.time + HammerRespawnTime;
            _hammers.Clear();
        }

        protected override void OnAbilityDeactivate()
        {
            base.OnAbilityDeactivate();
        }

        private readonly List<GameObject> _hammers = new();

        private void SpawnHammer()
        {
            if (_hammers.Count >= MaxHammers) return;
            
            var hammer = ObjectSpawner.SpawnProjectile(Entity, (Vector2) Entity.transform.position + new Vector2(2, 0),
                Vector2.zero, AttachedProjectile, true);
            _hammers.Add(hammer);
            
            var att = hammer.GetComponent<AttackComponent>();
                
            att.OnDamageDealt += (dmg, _, _) =>
            {
                if(dmg > 0) SpawnHammer();
            };
        }
    }
}