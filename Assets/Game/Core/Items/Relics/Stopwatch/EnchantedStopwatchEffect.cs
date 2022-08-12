using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Global;

namespace TonyDev.Game.Core.Effects.ItemEffects
{
    [GameEffect(ID="enchantedStopwatchEffect")]
    public class EnchantedStopwatchEffect : GameEffect
    {
        public override void OnAdd(GameEntity source)
        {
            Timer.TickSpeedMultiplier = 0.75f;
            Player.Instance.OnHurt += OnPlayerHit;
        }

        public override void OnRemove()
        {
            Timer.TickSpeedMultiplier = 1f;
            Player.Instance.OnHurt -= OnPlayerHit;
        }

        public override void OnUpdate()
        {
            
        }

        private void OnPlayerHit(float value)
        {
            Timer.GameTimer += 3f;
        }
    }
}
