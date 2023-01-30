using TonyDev.Game.Core.Entities;
using TonyDev.Game.Global;
using UnityEngine;

namespace TonyDev.Game.Core.Effects
{
    public class LifestealEffect : GameEffect
    {
        public Vector2 LeechPercentScale;
        private float LeechPercentFinal => LinearScale(LeechPercentScale.x, LeechPercentScale.y, 50);

        public override void OnAddOwner()
        {
            Entity.OnDamageOther += Lifesteal;
        }

        public override void OnRemoveOwner()
        {
            Entity.OnDamageOther -= Lifesteal;
        }

        private void Lifesteal(float dmg, GameEntity other, bool isCrit)
        {
            var leech = -dmg * LeechPercentFinal;

            ObjectSpawner.SpawnDmgPopup(Entity.transform.position, leech, isCrit);

            Entity.ApplyDamage(leech, out var success);
        }
        
        public override string GetEffectDescription()
        {
            return $"<color=green>Gain {GameTools.WrapColor($"{LeechPercentFinal:P1}", Color.yellow)} lifesteal.</color>";
        }
    }
}