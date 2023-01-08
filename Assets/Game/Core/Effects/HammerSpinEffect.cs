using System.Collections.Generic;
using System.Linq;
using TonyDev.Game.Core.Attacks;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Global;
using UnityEngine;
using UnityEngine.InputSystem;

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

            if (_hammers.Count == 0) return;

            var modifiedSpinSpeed = SpinSpeed * Entity.Stats.GetStat(Stat.AttackSpeed);

            var hammerList = _hammers.ToList();
            
            for (var i = 0; i < hammerList.Count; i++)
            {
                var (go, att) = hammerList[i];
                
                go.transform.localPosition = new Vector2(Mathf.Cos(Time.time*modifiedSpinSpeed + (float) i / _hammers.Count * 2 * Mathf.PI),
                    Mathf.Sin(Time.time*modifiedSpinSpeed + (float) i / _hammers.Count * 2 * Mathf.PI)) * SpinRadius;
                go.transform.GetChild(0).transform.up = Vector2.Perpendicular((Vector2) go.transform.localPosition - Vector2.zero);

                att.damageCooldown = 1f / Entity.Stats.GetStat(Stat.AttackSpeed);
            }
        }

        protected override void OnAbilityActivate()
        {
            base.OnAbilityActivate();

            foreach (var  (go, _) in _hammers)
            {
                var position = go.transform.position;
                
                var dir = (GameManager.MainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue()) - position).normalized;
                
                ObjectSpawner.SpawnProjectile(Entity, position, dir,
                    DetachProjectile);
            }

            DestroyAllHammers();
            
            _hammerRespawnTimer = Time.time + HammerRespawnTime;
            _hammers.Clear();
        }

        private void DestroyAllHammers()
        {
            foreach (var (key, value) in _hammers)
            {
                GameManager.Instance.CmdDestroyProjectile(value.identifier);
                Object.Destroy(key);
            }
            
            _hammerRespawnTimer = Time.time + HammerRespawnTime;
            _hammers.Clear();
        }

        protected override void OnAbilityDeactivate()
        {
            base.OnAbilityDeactivate();
        }

        private readonly Dictionary<GameObject, AttackComponent> _hammers = new();

        private void SpawnHammer()
        {
            if (_hammers.Count >= MaxHammers) return;
            
            var hammer = ObjectSpawner.SpawnProjectile(Entity, (Vector2) Entity.transform.position + new Vector2(2, 0),
                Vector2.zero, AttachedProjectile, true);

            var att = hammer.GetComponent<AttackComponent>();
                
            att.OnDamageDealt += (dmg, _, _) =>
            {
                if(dmg > 0) SpawnHammer();
            };
            
            _hammers.Add(hammer, att);
        }
    }
}