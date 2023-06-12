using TonyDev.Game.Level.Decorations;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TonyDev.Game.Core.Items
{
    public class InteractableItem : InteractableButton
    {
        public int sellPrice;

        private new void Start()
        {
            base.Start();
            Indicator.SetSellPrice(sellPrice);
        }
        
        public void SetSellPrice(int newSellPrice)
        {
            sellPrice = newSellPrice;
            _sellPriceChanged = true;
        }
        
        public override void SetCost(int newCost)
        {
            base.SetCost(newCost);
            OverrideInteractKey(Key.E, newCost == 0 ? InteractType.Pickup : InteractType.Purchase);
        }

        private bool _sellPriceChanged;

        private new void Update()
        {
            base.Update();
            if (_sellPriceChanged)
            {
                Indicator.SetSellPrice(sellPrice);
                _sellPriceChanged = false;
            }
        }
    }
}
