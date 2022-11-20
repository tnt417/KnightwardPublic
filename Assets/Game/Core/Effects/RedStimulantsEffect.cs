using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Global;
using UnityEngine;

namespace TonyDev.Game.Core.Effects
{
    public class RedStimulantsEffect : GameEffect
    {
        public Vector2 healthRedirectScale;
        private float RedirectMultiplier => LinearScale(healthRedirectScale.x, healthRedirectScale.y, 50);

        public override void OnAddOwner()
        {
        }

        public override void OnRemoveOwner()
        {
        }

        private float _nextBuffTime;

        public override void OnUpdateOwner()
        {
            if (_nextBuffTime > Time.time) return;

            _nextBuffTime = Time.time + 1f;

            var strength = RedirectMultiplier * Entity.Stats.GetStat(Stat.Health);

            Entity.Stats.AddBuff(new StatBonus(StatType.Flat, Stat.Damage, strength, EffectIdentifier), 1f);
        }

        public override string GetEffectDescription()
        {
            return
                $"{Tools.WrapColor($"{RedirectMultiplier:P1}", Color.yellow)} <color=green>of your health is added to your damage stat.</color>";
        }
    }
}