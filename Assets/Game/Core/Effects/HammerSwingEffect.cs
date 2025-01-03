using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
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

            Player.LocalInstance.playerAnimator.SetWeaponAnimSprite("hammer_anim");
            Player.LocalInstance.playerAnimator.attackAnimationName = "Overhead";
        }

        public override void OnRemoveOwner()
        {
            base.OnRemoveOwner();

            Player.LocalInstance.playerAnimator.UnShake();

            _charging = false;
            ((Player) Entity).playerAnimator.attackingOverride = false;

            foreach (var ind in _indicators)
            {
                Object.Destroy(ind);
            }

            _indicators.Clear();
        }

        private bool _charging = false;
        private bool _launching = false;
        private float _chargeTimer;

        public override void OnUpdateOwner()
        {
            base.OnUpdateOwner();

            UpdateIndicators();

            if (_charging)
            {
                _chargeTimer += Time.deltaTime;

                Player.LocalInstance.playerAnimator.SetAttackAnimProgress(Amount * 0.25f);

                if (Amount > _lastAmount)
                {
                    Player.LocalInstance.playerAnimator.Shake(Amount * 0.7f);
                    SoundManager.PlaySoundPitchVariant("dagger", 0.5f, Entity.transform.position, 1.6f + Amount * 0.2f,
                        1.7f + Amount * 0.2f);
                    _lastAmount = Amount;
                }
            }

            if (Player.LocalInstance.fireKeyHeld)
            {
                _charging = true;
                ((Player) Entity).playerAnimator.attackingOverride = true;
            }

            if (!Player.LocalInstance.fireKeyHeld && _charging)
            {
                _charging = false;
                Launch();
            }
        }

        private List<GameObject> _indicators = new();

        private float ModifiedMaxChargeTime => MaxChargeTime / Entity.Stats.GetStat(Stat.AttackSpeed);
        private int _lastAmount;

        private int Amount =>
            Mathf.Clamp(Mathf.RoundToInt((_chargeTimer / ModifiedMaxChargeTime) * MaxAmount), 0, MaxAmount);

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
                var spacingOffset = direction * (i + 1) * ProjectileSpacing * Entity.Stats.GetStat(Stat.AoeSize);
                ind.transform.position = (Vector2) Entity.transform.position + new Vector2(0, -0.5f) + spacingOffset;
                ind.transform.localScale = Vector3.one * Entity.Stats.GetStat(Stat.AoeSize);
            }
        }

        private void Launch()
        {
            _lastAmount = 0;

            //TODO: Change this to instead just change the conditions of PlayerIsAttacking or whatever, so that the player is 'attacking' when not clicking in the case of the hammer, that way the end of the animation can play through
            //Player.LocalInstance.playerAnimator.PlayOverrideAnim("PlayerOverheadDownIdle", 0, 0.75f).Forget();

            LaunchAnim().Forget();

            for (var i = 0; i < Amount; i++)
            {
                var direction = GameManager.MouseDirection.normalized;
                var spacingOffset = direction * (i + 1) * ProjectileSpacing * Entity.Stats.GetStat(Stat.AoeSize);
                ObjectSpawner.SpawnProjectile(Entity,
                    (Vector2) Entity.transform.position + new Vector2(0, -0.5f) + spacingOffset, direction, Projectile);
            }

            _chargeTimer = 0;
        }

        private async UniTask LaunchAnim()
        {
            Player.LocalInstance.playerAnimator.UnShake();
            if (Amount == 0)
            {
                ((Player) Entity).playerAnimator.attackingOverride = false;
                return;
            }

            ((Player) Entity).playerAnimator.SetAttackAnimProgress(0.75f);
            await UniTask.Delay(TimeSpan.FromSeconds(0.5f));
            if (!_charging) ((Player) Entity).playerAnimator.attackingOverride = false;
        }
    }
}