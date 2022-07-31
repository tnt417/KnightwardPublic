using UnityEngine;

namespace TonyDev.Game.Core.Items.ItemEffects
{
    public abstract class ItemEffect
    {
        public abstract void OnAdd();
        public abstract void OnRemove();
        public abstract void OnUpdate();
    }
}
