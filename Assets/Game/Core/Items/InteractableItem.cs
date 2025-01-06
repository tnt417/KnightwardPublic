using TonyDev.Game.Level.Decorations;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TonyDev.Game.Core.Items
{
    public class InteractableItem : InteractableButton
    {
        public int sellPrice;
        public int stackCount;

        private new void Start()
        {
            base.Start();
            Indicator.SetSellPrice(sellPrice);
            Indicator.SetCount(stackCount);
        }
        
        public void SetSellPrice(int newSellPrice)
        {
            sellPrice = newSellPrice;
            _sellPriceChanged = true;
        }
        
        public void SetCount(int count)
        {
            stackCount = count;
            _countChanged = true;
        }
        
        public override void SetCost(int newCost)
        {
            base.SetCost(newCost);
            OverrideInteractKey(Key.E, newCost == 0 ? InteractType.Pickup : InteractType.Purchase);
        }

        private bool _sellPriceChanged;
        private bool _countChanged;

        private new void Update()
        {
            base.Update();
            if (_sellPriceChanged)
            {
                Indicator.SetSellPrice(sellPrice);
                _sellPriceChanged = false;
            }

            if (_countChanged)
            {
                Indicator.SetCount(stackCount);
                _countChanged = false;
            }
        }
    }
}
