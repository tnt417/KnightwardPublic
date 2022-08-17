using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Global;
using UnityEngine;

namespace TonyDev.Game.Core.Effects.ItemEffects
{
    [GameEffect(ID = "broadswordEffect")]
    public class BroadswordEffect : AbilityEffect
    {
        protected override void OnAbilityActivate()
        {
            Object.Instantiate(ObjectDictionaries.Prefabs["broadswordEffect"], Player.LocalInstance.transform.position, Quaternion.identity);
        }

        protected override void OnAbilityDeactivate()
        {
            
        }

        public override void OnAdd(GameEntity source)
        {
            ActivateButton = KeyCode.Q;
            Cooldown = 10f;
        }

        public override void OnRemove()
        {
        }
    }
}
