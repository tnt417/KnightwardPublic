using UnityEngine;
using UnityEngine.EventSystems;

namespace TonyDev
{
    public class GameButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Animator buttonAnimator;
        [SerializeField] private SoundPlayer clickSound;

        public void OnClick()
        {
            buttonAnimator.Play("ClassButtonClick");
            clickSound.PlaySound();
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            buttonAnimator.SetBool("hovered", true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            buttonAnimator.SetBool("hovered", false);
        }
    }
}