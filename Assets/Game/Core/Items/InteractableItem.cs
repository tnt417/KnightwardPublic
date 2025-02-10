using TonyDev.Game.Level.Decorations;
using TonyDev.Game.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TonyDev.Game.Core.Items
{
    public class InteractableItem : InteractableButton
    {
        public int stackCount;

        private new void Start()
        {
            base.Start();
        }
        
        public void SetSellPrice(int newSellPrice)
        {
            base.SetCost(-newSellPrice);
            
            TryCallToUpdate();
        }
        
        public void SetCount(int count)
        {
            stackCount = count;
            
            TryCallToUpdate();
        }
        
        public override void SetCost(int newCost)
        {
            base.SetCost(newCost);
            SetInteractKey(Key.E, newCost <= 0 ? InteractType.Pickup : InteractType.Purchase);
            SetInteractKey(Key.F, newCost < 0 ? InteractType.Scrap : InteractType.None);
            
            TryCallToUpdate();
        }
        
    }
}
