using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TonyDev.Game.Global
{
    public class AbilityControlManager : MonoBehaviour
    {
        public static AbilityControlManager Instance;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;
        }

        private readonly Action[] _abilityAction = new Action[10];

        private int _index;
        
        public int GetOpenIndex()
        {
            for (var i = 0; i < _abilityAction.Length; i++)
            {
                if (_abilityAction[i] == null) return i;
            }

            return -1;
        }
        
        public void RegisterCallback(Action callback, int index)
        {
            _abilityAction[index] = () => callback?.Invoke();
        }

        public void UnregisterCallback(int index)
        {
            _abilityAction[index] = null;
        }
        
        public void OnAbility1(InputValue value)
        {
            if (!GameManager.GameControlsActive) return;
            if (value.isPressed) _abilityAction[0]?.Invoke();
        }

        public void OnAbility2(InputValue value)
        {
            if (!GameManager.GameControlsActive) return;
            if (value.isPressed) _abilityAction[1]?.Invoke();
        }

        public void OnAbility3(InputValue value)
        {
            if (!GameManager.GameControlsActive) return;
            if (value.isPressed) _abilityAction[2]?.Invoke();
        }

        public void OnAbility4(InputValue value)
        {
            if (!GameManager.GameControlsActive) return;
            if (value.isPressed) _abilityAction[3]?.Invoke();
        }

        public void OnAbility5(InputValue value)
        {
            if (!GameManager.GameControlsActive) return;
            if (value.isPressed) _abilityAction[4]?.Invoke();
        }

        public void OnAbility6(InputValue value)
        {
            if (!GameManager.GameControlsActive) return;
            if (value.isPressed) _abilityAction[5]?.Invoke();
        }

        public void OnAbility7(InputValue value)
        {
            if (!GameManager.GameControlsActive) return;
            if (value.isPressed) _abilityAction[6]?.Invoke();
        }

        public void OnAbility8(InputValue value)
        {
            if (!GameManager.GameControlsActive) return;
            if (value.isPressed) _abilityAction[7]?.Invoke();
        }

        public void OnAbility9(InputValue value)
        {
            if (!GameManager.GameControlsActive) return;
            if (value.isPressed) _abilityAction[8]?.Invoke();
        }
        
        public void OnAbility10(InputValue value)
        {
            if (!GameManager.GameControlsActive) return;
            if(value.isPressed) _abilityAction[9]?.Invoke();
        }
    }
}