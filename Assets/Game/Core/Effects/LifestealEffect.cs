using TonyDev.Game.Core.Entities;
using TonyDev.Game.Global;

namespace TonyDev.Game.Core.Effects
{
    public class LifestealEffect : GameEffect
    {
        public float LeechPercent;

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
            var leech = -dmg * LeechPercent;

            ObjectSpawner.SpawnDmgPopup(Entity.transform.position, leech, isCrit);

            Entity.ApplyDamage(leech, out var success);
        }
    }
}