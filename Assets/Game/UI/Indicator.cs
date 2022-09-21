using TMPro;
using UnityEngine;

namespace TonyDev.Game.UI
{
    public class Indicator : MonoBehaviour
    {
        [SerializeField] private TMP_Text labelText;
        [SerializeField] private TMP_Text costLabel;
        [SerializeField] private GameObject costObject;
        
        public void SetCost(int cost)
        {
            costObject.SetActive(cost != 0);
            costLabel.text = cost < 0 ? "+" : "" + cost;
        }

        public void SetLabel(string label)
        {
            labelText.text = label;
        }
    }
}
