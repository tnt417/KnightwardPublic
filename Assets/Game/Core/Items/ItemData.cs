using UnityEngine;

namespace TonyDev.Game.Core.Items
{
    [CreateAssetMenu(fileName = "ItemData", menuName = "ItemData")]
    public class ItemData : ScriptableObject
    {
        public Item item;
    }
}
