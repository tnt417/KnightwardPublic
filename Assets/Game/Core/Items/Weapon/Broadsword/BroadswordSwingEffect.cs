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

            var dir = GameManager.MouseDirection.normalized;

            var pos = (Vector2) Entity.transform.position;
            var aoeStat = Entity.Stats.GetStat(Stat.AoeSize);
            var atkSpdStat = Entity.Stats.GetStat(Stat.AttackSpeed);

            var proj = ObjectSpawner.SpawnProjectile(Entity,
                pos + (_combo == 2
                    ? Vector2.zero
                    : dir * 1.5f *
                      aoeStat), GameManager.MouseDirection,
                _combo == 0 ? Attack1 : _combo == 1 ? Attack2 : Attack3);

            SmoothCameraFollow.Shake(_combo == 2 ? 3f : 2f, 2f);
            
            // if (_combo == 2)
            // {
            //     // var playerMovement = Player.LocalInstance.playerMovement.currentMovementInput.normalized;
            //     //
            //     // var proj1 = ObjectSpawner.SpawnProjectile(Entity,
            //     //     pos + playerMovement * 2.5f *
            //     //     aoeStat, playerMovement,
            //     //     Attack1);
            //     //
            //     // var proj2 = ObjectSpawner.SpawnProjectile(Entity,
            //     //     pos + playerMovement * 2.5f *
            //     //     aoeStat, playerMovement,
            //     //     Attack2);
            //     //
            //     // proj1.GetComponent<Animator>().speed = atkSpdStat;
            //     // proj2.GetComponent<Animator>().speed = atkSpdStat;
            //
            //     //Player.LocalInstance.playerMovement.Dash(2f, -dir);
            // }

            if (_combo != 2) proj.GetComponent<Animator>().speed = atkSpdStat;
            //Player.LocalInstance.playerMovement.Dash(1f, Player.LocalInstance.playerMovement.currentMovementInput);

            _combo++;
            _combo %= 3;
        }

        private void PlaySound()
        {
            SoundManager.PlaySound("dagger", 0.5f, Entity.transform.position, Random.Range(0.6f, 0.8f));
        }

        public override void OnRemoveOwner()
        {
            Entity.OnAttack -= OnAttack;
        }
    }
}