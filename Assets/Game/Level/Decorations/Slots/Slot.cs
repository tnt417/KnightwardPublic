using System;
using TonyDev.Game.Core.Entities;
using UnityEngine;

namespace TonyDev.Game.Level.Decorations.Slots
{
    public class Slot : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer slotRenderer;

        private SlotEntry _currentEntry;
        
        public Action SwapImageAction;

        [SerializeField] private Animator slotAnimator;

        public void SetEntry(SlotEntry newEntry)
        {
            _currentEntry = newEntry;
            slotRenderer.sprite = newEntry.slotSprite;
        }
        
        public void NotifyDone()
        {
            SwapImageAction?.Invoke();
        }

        public void PlayAnimation()
        {
            slotAnimator.Play("SlotRoll");
        }
    }
}
