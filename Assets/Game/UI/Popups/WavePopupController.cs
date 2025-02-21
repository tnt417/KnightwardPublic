using UnityEngine;

namespace TonyDev
{
    public class WavePopupController : MonoBehaviour
    {
        public static WavePopupController Instance;

        [SerializeField] private Animator _popupAnimator;

        private void Start()
        {
            Instance = this;
        }

        public void Play()
        {
            _popupAnimator.Play("WavePopup");
        }
    }
}
