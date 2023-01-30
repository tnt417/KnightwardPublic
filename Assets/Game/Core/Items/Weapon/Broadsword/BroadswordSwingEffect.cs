using System.Collections;
using System.Collections.Generic;
using TonyDev.Game;
using TonyDev.Game.Core.Attacks;
using TonyDev.Game.Core.Effects;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Global;
using UnityEngine;

namespace TonyDev
{
    public class BroadswordSwingEffect : GameEffect
    {
        public ProjectileData Attack1;
        public ProjectileData Attack2;
        public ProjectileData Attack3;

        private int _combo = 0;

        public override void OnAddOwner()
        {
            Entity.OnAttack += OnAttack;
        }

        public override void OnUpdateOwner()
        {
            if (!Player.LocalInstance.fireKeyHeld)
            {
                _combo = 0;
            }
        }

        private void OnAttack()
        {
            PlaySound();

            var proj = ObjectSpawner.SpawnProjectile(Entity,
                (Vector2) Entity.transform.position + (_combo == 2 ? Vector2.zero : GameManager.MouseDirection.normalized * 1.5f *
                Entity.Stats.GetStat(Stat.AoeSize)), GameManager.MouseDirection,
                _combo == 0 ? Attack1 : _combo == 1 ? Attack2 : Attack3);
            
            if(_combo != 2) proj.GetComponent<Animator>().speed = Entity.Stats.GetStat(Stat.AttackSpeed);
            //Player.LocalInstance.playerMovement.Dash(1f, Player.LocalInstance.playerMovement.currentMovementInput);

            _combo++;
            _combo %= 3;
        }

        private void PlaySound()
        {
            SoundManager.PlaySoundPitchVariant("dagger", 0.5f, Entity.transform.position, 0.6f, 0.8f);
        }

        public override void OnRemoveOwner()
        {
            Player.LocalInstance.OnAttack -= OnAttack;
        }
    }
}