using TonyDev.Game.Core.Effects;
using TonyDev.Game.Core.Effects.ItemEffects;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Global;

namespace TonyDev.Game.Core.Items.Relics.Stopwatch
{
    public class EnchantedStopwatchEffect : GameEffect
    {
        public override void OnAddServer()
        {
            Timer.TickSpeedMultiplier = 0.75f;
            Entity.OnHurt += OnPlayerHit;
        }
        public override void OnRemoveServer()
        {
            Timer.TickSpeedMultiplier = 1f;
            Entity.OnHurt -= OnPlayerHit;
        }
        private void OnPlayerHit(float value)
        {
            Timer.GameTimer += 3f;
        }
    }
}
