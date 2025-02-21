using TonyDev.Game.Global.Network;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TonyDev
{
    public class ClassController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Animator hoverAnimator;

        public ClassButton selectedClass;

        public void SetClass(ClassButton newSelectedClass, string newClassFx)
        {
            if (selectedClass == newSelectedClass) return;
            if(selectedClass != null) selectedClass.OnDeselect();
            selectedClass = newSelectedClass;
            CustomRoomPlayer.Local.CmdSetClassFxName( newClassFx);
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            hoverAnimator.SetBool("hovered", true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            hoverAnimator.SetBool("hovered", false);
        }
    }
}
