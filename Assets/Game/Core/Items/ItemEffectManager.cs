using System.Collections.Generic;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Core.Items.ItemEffects;
using TonyDev.Game.Level.Decorations.Crystal;
using UnityEngine;
using UnityEngine.Events;

namespace TonyDev.Game.Core.Items
{
    public static class ItemEffectManager
    {
        public static void OnEffectsAdded(List<ItemEffect> effects) //Called when an item is inserted into the inventory
        {
            if (effects == null) return;
            
            foreach (var e in effects)
            {
                e.OnAdd();
            }
        }

        public static void OnEffectsUpdate(List<ItemEffect> effects)
        {
            if (effects == null) return;
            
            foreach (var e in effects)
            {
                e.OnUpdate();
            }
        }

        public static void OnEffectsRemoved(List<ItemEffect> effects) //Called when an item is inserted into the inventory
        {
            if (effects == null) return;
            
            foreach (var e in effects)
            {
                e.OnRemove();
            }
        }
    }
}
