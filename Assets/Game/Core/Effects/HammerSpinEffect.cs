using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using TonyDev.Game.Core.Attacks;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Global;
using UnityEngine;
using UnityEngine.InputSystem;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

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

        public override void OnAddOwner()
        {
            ActivateButton = KeyCode.None;
            base.OnAddOwner();
        }

        public override void OnRemoveOwner()
        {
            DestroyAllHammers();
            base.OnRemoveOwner();
        }

        public override void OnUpdateOwner()
        {
            if (Mouse.current.rightButton.isPressed && Ready)
            {
                OnAbilityActivate();
            }

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

            HammerSfx(_hammers.Count).Forget();
            
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

        private readonly Dictionary<GameObject, AttackComponent> _hammers = new();

        private async UniTask HammerSfx(int count)
        {
            for (int i = 0; i < count; i++)
            {
                SoundManager.PlaySound("woosh", 0.5f, Entity.transform.position, Random.Range(0.6f, 0.8f));
                await UniTask.Delay(Random.Range(50, 75));
            }
        }
        
        private void SpawnHammer()
        {
            if (_hammers.Count >= MaxHammers) return;
            
            var hammer = ObjectSpawner.SpawnProjectile(Entity, (Vector2) Entity.transform.position + new Vector2(2, 0),
                Vector2.zero, AttachedProjectile, true);

            var att = hammer.GetComponent<AttackComponent>();
                
            att.OnDamageDealt += (dmg, _, _, _) =>
            {
                if(dmg > 0) SpawnHammer();
            };
            
            _hammers.Add(hammer, att);
        }
    }
}