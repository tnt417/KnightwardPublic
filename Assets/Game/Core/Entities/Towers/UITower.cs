using System.Collections;
using System.Collections.Generic;
using TonyDev.Game.Core.Entities.Towers;
using TonyDev.UI;
using UnityEngine;
using UnityEngine.UI;

namespace TonyDev
{
    public class UITower : MonoBehaviour
    {
        //Editor variables
        [SerializeField] private Image towerImage;
        //
        
        private GameObject _towerPrefab;

        public void Set(GameObject prefab)
        {
            towerImage.sprite = prefab.GetComponentInChildren<SpriteRenderer>().sprite;
            _towerPrefab = prefab;
        }

        public void OnClick()
        {
            TowerUIController.Instance.StartPlacingTower(this, _towerPrefab);
        }
    }
}
