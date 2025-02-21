using TonyDev.Game.Core.Behavior;
using TonyDev.Game.Level;
using UnityEngine;

namespace TonyDev
{
    public class UpgradeInteractable : InteractableButton
    {
        public static UpgradeInteractable Instance;

        private void Awake()
        {
            Instance = this;
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

            UpgradeManager.OnUpgradeLocal += Deactivate;
        }

        private void Deactivate()
        {
            notificationObject.SetActive(false);
            isInteractable = false;
        }

        private void OnDestroy()
        {
            UpgradeManager.OnUpgradeLocal -= Deactivate;
        }

        public GameObject notificationObject;
    }
}