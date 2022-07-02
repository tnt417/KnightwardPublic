using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Core.Items.ItemEffects;
using TonyDev.Game.Level.Decorations.Crystal;
using UnityEngine;
using UnityEngine.Events;

namespace TonyDev.Game.Core.Items
{
    public static class ItemEffectManager
    {
        public static void OnEffectsAdded(ItemEffect[] effects) //Called when an item is inserted into the inventory
        {
            if (effects == null) return;
            
            foreach (var e in effects)
            {
                e.OnAdd();
            }
        }

        public static void OnEffectsUpdate(ItemEffect[] effects)
        {
            
        }

        public static void OnEffectsRemoved(ItemEffect[] effects) //Called when an item is inserted into the inventory
        {
            if (effects == null) return;
            
            foreach (var e in effects)
            {
                e.OnRemove();
            }
        }
    }
}
