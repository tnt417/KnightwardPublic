using System.Collections;
using System.Collections.Generic;
using TonyDev.Game.Core.Items.ItemEffects;
using UnityEngine;

namespace TonyDev
{
    public abstract class AbilityEffect : ItemEffect
    {
        public KeyCode activateKey;
        public float cooldown;

        private float _timer;
        
        protected abstract void OnAbilityActivate();

        public override void OnUpdate()
        {
            _timer += Time.deltaTime;

            if (_timer > cooldown && Input.GetKeyDown(activateKey))
            {
                OnAbilityActivate();
                _timer = 0;
            }
        }
    }
}
