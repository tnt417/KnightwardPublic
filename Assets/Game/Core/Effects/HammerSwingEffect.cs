using System;
using System.Collections.Generic;
using TonyDev.Game.Core.Attacks;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Global;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TonyDev.Game.Core.Effects
{
    public class HammerSwingEffect : GameEffect
    {
        public ProjectileData Projectile;
        public int MaxAmount;
        public float ProjectileSpacing;
        public float MaxChargeTime;
        public string IndicatorPrefabKey;

        public override void OnAddOwner()
        {
            base.OnAddOwner();

            for (var i = 0; i < MaxAmount; i++)
            {
                var go = Object.Instantiate(ObjectFinder.GetPrefab(IndicatorPrefabKey));
                go.SetActive(false);
                _indicators.Add(go);
            }
            
            Player.LocalInstance.playerAnimator.attackAnimationName = "Attack";
        }

        public override void OnRemoveOwner()
        {
            base.OnRemoveOwner();
            
            foreach (var ind in _indicators)
            {
                Object.Destroy(ind);
            }
            
            _indicators.Clear();
        }

        private bool _charging = false;
        private float _chargeTimer;
        
        public override void OnUpdateOwner()
        {
            base.OnUpdateOwner();

            UpdateIndicators();
            
            if (_charging)
            {
                _chargeTimer += Time.deltaTime;
            }

            if (Player.LocalInstance.fireKeyHeld)
            {
                _charging = true;
            }
            
            if (!Player.LocalInstance.fireKeyHeld && _charging)
            {
                _charging = false;
                Launch();
            }
        }

        private List<GameObject> _indicators = new ();
        
        private float ModifiedMaxChargeTime => MaxChargeTime / Entity.Stats.GetStat(Stat.AttackSpeed);
        private int Amount => Mathf.Clamp(Mathf.RoundToInt((_chargeTimer / ModifiedMaxChargeTime) * MaxAmount), 0, MaxAmount);
        
        private void UpdateIndicators()
        {
            for (var i = 0; i < _indicators.Count; i++)
            {
                var ind = _indicators[i];
                
                if (i >= Amount)
                {
                    ind.SetActive(false);
                    continue;
                }

                ind.SetActive(true);
                
                var direction = GameManager.MouseDirection.normalized;
                var spacingOffset = direction * (i + 1) * ProjectileSpacing;
                ind.transform.position = (Vector2)Entity.transform.position+ new Vector2(0, -0.5f) + spacingOffset;
            }
        }
        
        private void Launch()
        {
            for (var i = 0; i < Amount; i++)
            {
                var direction = GameManager.MouseDirection.normalized;
                var spacingOffset = direction * (i + 1) * ProjectileSpacing;
                ObjectSpawner.SpawnProjectile(Entity, (Vector2)Entity.transform.position + new Vector2(0, -0.5f) + spacingOffset, direction, Projectile);
            }
            
            _chargeTimer = 0;
        }
    }
}