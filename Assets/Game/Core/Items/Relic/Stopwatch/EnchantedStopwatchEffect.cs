using TonyDev.Game.Core.Effects;
using TonyDev.Game.Core.Effects.ItemEffects;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Global;

namespace TonyDev.Game.Core.Items.Relics.Stopwatch
{
    public class EnchantedStopwatchEffect : GameEffect
    {
        public override void OnAddOwner()
        {
            Timer.TickSpeedMultiplier = 0.75f;
            Player.LocalInstance.OnHurt += OnPlayerHit;
        }

        public override void OnRemoveOwner()
        {
            Timer.TickSpeedMultiplier = 1f;
            Player.LocalInstance.OnHurt -= OnPlayerHit;
        }

        public override void OnUpdateOwner()
        {
            
        }

        private void OnPlayerHit(float value)
        {
            Timer.GameTimer += 3f;
        }
    }
}
