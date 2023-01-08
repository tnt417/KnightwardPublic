using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TonyDev.Game.UI.GameInfo
{
    public class CooldownObjectUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private Image cooldownImage;
        [SerializeField] private Image iconImage;

        private Func<float> _totalCooldown;
        private Func<float> _remainingCooldown;
        private KeyCode _activateKey;
        private string KeyName => Enum.GetName(typeof(KeyCode), _activateKey)?.Replace("Alpha", "");

        private bool _set;
        
        public void Set(Sprite imgSprite, Func<float> totalCooldownFunc, Func<float> remainingCooldownFunc, KeyCode activateKey)
        {
            iconImage.sprite = imgSprite;
            
            _totalCooldown = totalCooldownFunc;
            _remainingCooldown = remainingCooldownFunc;
            _activateKey = activateKey;

            _set = true;
        }

        private int _lastCeilCd;
        
        private void Update()
        {
            if (!_set)
            {
                Debug.LogWarning("Set method hasn't been called!");
                return;
            }
            
            var cd = _remainingCooldown.Invoke();

            cooldownImage.fillAmount = cd / _totalCooldown.Invoke();

            var ceilCd = Mathf.CeilToInt(cd);

            if (ceilCd == _lastCeilCd) return;

            _lastCeilCd = ceilCd;

            var keyName = _activateKey == KeyCode.None ? "Ready" : KeyName;
            
            statusText.text = cd > 0 ? ceilCd.ToString() : keyName;
            statusText.color = cd > 0 ? Color.white : Color.green;
        }
    }
}
