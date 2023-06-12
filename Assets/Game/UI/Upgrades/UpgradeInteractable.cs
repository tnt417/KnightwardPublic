using System.Collections;
using System.Collections.Generic;
using TonyDev.Game.Core.Behavior;
using TonyDev.Game.Level;
using UnityEngine;

namespace TonyDev
{
    public class UpgradeInteractable : InteractableButton
    {
        public UpgradeInteractable instance;

        private void Awake()
        {
            instance = this;
        }

        protected new void Start()
        {
            base.Start();
            
            notificationObject.SetActive(false);
            isInteractable = false;
            
            WaveManager.Instance.OnWaveBegin += wave =>
            {
                if ((wave - 1) % 8 != 0 || wave <= 1) return;
                notificationObject.SetActive(true);
                isInteractable = true;
                UpgradeManager.Instance.RollUpgrades(3, true);
            };

            UpgradeManager.Instance.OnUpgradeLocal += () =>
            {
                notificationObject.SetActive(false);
                isInteractable = false;
            };
        }

        public GameObject notificationObject;

        public void SetActive(bool active)
        {
            Indicator.gameObject.SetActive(active);
        }
    }
}