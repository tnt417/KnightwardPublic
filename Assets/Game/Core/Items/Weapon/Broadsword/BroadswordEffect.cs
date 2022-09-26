using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Global;
using UnityEngine;

namespace TonyDev.Game.Core.Effects.ItemEffects
{
    public class BroadswordEffect : AbilityEffect
    {
        protected override void OnAbilityActivate()
        {
            Object.Instantiate(ObjectFinder.GetPrefab("broadswordEffect"), Player.LocalInstance.transform.position, Quaternion.identity);
        }

        protected override void OnAbilityDeactivate()
        {
            
        }
    }
}
