using TonyDev.Game.Core.Entities.Player;

namespace TonyDev.Game.Core.Effects
{
    public class PlayerSizeEffect : GameEffect
    {
        public float SizeMod;
    
        public override void OnAddClient()
        {
            if (Entity is Player p)
            {
                p.playerAnimator.ModifyPlayerSize(SizeMod);
            }
            else
            {
                Entity.RemoveEffect(this);
            }
        }

        public override void OnRemoveClient()
        {
            if (Entity is Player p)
            {
                p.playerAnimator.ModifyPlayerSize(1/SizeMod);
            }
        }
        
        public override string GetEffectDescription()
        {
            return $"Increase player size by <color=yellow>{(SizeMod-1):P0}</color>.";
        }
    }
}