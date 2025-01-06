using TMPro;
using UnityEngine;

namespace TonyDev.Game.UI
{
    public class Indicator : MonoBehaviour
    {
        [Header("Label")]
        [SerializeField] private TMP_Text labelText;
        
        [Header("Cost")]
        [SerializeField] private TMP_Text costLabel;
        [SerializeField] private GameObject costObject;
        
        [Header("Essence")]
        [SerializeField] private TMP_Text essenceLabel;
        [SerializeField] private GameObject essenceObject;

        [SerializeField] private TMP_Text countLabel;
        
        public void SetCost(int cost)
        {
            costObject.SetActive(cost != 0);
            costLabel.text = cost < 0 ? "+" : "" + cost;
        }

        public void SetLabel(string label)
        {
            labelText.text = label;
        }

        public void SetSellPrice(int sellPrice)
        {
            essenceObject.SetActive(sellPrice != 0);
            essenceLabel.text = "+" + sellPrice;
        }

        public void SetCount(int count)
        {
            if (count == 1)
            {
                countLabel.enabled = false;
            }
            else
            {
                countLabel.enabled = true;
                countLabel.text = "x" + count;
            }
        }
    }
}
