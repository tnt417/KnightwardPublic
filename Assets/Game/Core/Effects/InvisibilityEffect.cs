using TonyDev.Game.Core.Entities.Player;

namespace TonyDev.Game.Core.Effects
{
    public class InvisibilityEffect : GameEffect
    {
        public override void OnAddOwner()
        {
            Player.LocalInstance.playerAnimator.SetOpacity(0.5f);
            Entity.IsTangible = false;
        }

        public override void OnRemoveOwner()
        {
            Player.LocalInstance.playerAnimator.SetOpacity(1f);
            Entity.IsTangible = true;
        }

        public override void OnUpdateOwner()
        {

        }
    }
}