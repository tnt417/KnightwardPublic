using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TonyDev
{
    public class ClassButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private ClassController classController;

        [SerializeField] private Animator buttonAnimator;
        [SerializeField] private Image buttonImage;
        [SerializeField] private SoundPlayer clickSound;

        [SerializeReference] public string classEffectName;
        
        private bool _selected;

        private void Update()
        {
            if (_selected)
            {
                buttonImage.rectTransform.localScale = new Vector3(1.1f, 1.1f, 1f);
            }
        }

        public void OnClick()
        {
            _selected = true;
            buttonImage.color = new Color(buttonImage.color.r, buttonImage.color.g, buttonImage.color.b, 1f);
            buttonAnimator.Play("ClassButtonClick");
            clickSound.PlaySound();
            classController.SetClass(this, classEffectName);
        }

        public void OnDeselect()
        {
            _selected = false;
            buttonImage.color = new Color(buttonImage.color.r, buttonImage.color.g, buttonImage.color.b, 190/255f);
            buttonAnimator.Play("ClassButtonUnhover");
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
