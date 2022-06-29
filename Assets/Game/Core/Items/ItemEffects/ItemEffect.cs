using UnityEngine;

namespace TonyDev.Game.Core.Items.ItemEffects
{
    public abstract class ItemEffect : ScriptableObject
    {
        public abstract void OnAdd();
        public abstract void OnRemove();
        public abstract void OnUpdate();
    }
}
