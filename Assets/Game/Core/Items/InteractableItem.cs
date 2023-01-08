using TonyDev.Game.Level.Decorations;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TonyDev.Game.Core.Items
{
    public class InteractableItem : InteractableButton
    {
        public int essence;

        private new void Start()
        {
            base.Start();
            Indicator.SetEssence(essence);
        }
        
        public void SetEssence(int newEssence)
        {
            essence = newEssence;
            _essenceChanged = true;
        }
        
        public override void SetCost(int newCost)
        {
            base.SetCost(newCost);
            OverrideInteractKey(Key.E, newCost == 0 ? InteractType.Pickup : InteractType.Purchase);
        }

        private bool _essenceChanged;

        private new void Update()
        {
            base.Update();
            if (_essenceChanged)
            {
                Indicator.SetEssence(essence);
                _essenceChanged = false;
            }
        }
    }
}
