using TonyDev.Game.Global;
using TonyDev.Game.UI.Tower;

namespace TonyDev.Game.Core.Effects
{
    public class CelestialWrenchEffect : GameEffect
    {
        public override void OnAddOwner()
        {
            TowerPlacementManager.Instance.placeablePhases.Add(GamePhase.Dungeon);
        }

        public override void OnRemoveOwner()
        {
            TowerPlacementManager.Instance.placeablePhases.Remove(GamePhase.Dungeon);
        }
    }
}