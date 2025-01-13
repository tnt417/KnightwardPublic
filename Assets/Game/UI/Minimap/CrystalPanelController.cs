using System;
using System.Collections.Generic;
using System.Linq;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Enemies;
using TonyDev.Game.Global;
using TonyDev.Game.Level.Decorations.Crystal;
using UnityEngine;

namespace TonyDev
{
    public class CrystalPanelController : MonoBehaviour
    {
        private readonly Dictionary<GameEntity, RectTransform> _uiObjects = new();
        private readonly List<GameObject> _inactiveUiObjects = new();

        [SerializeField] private GameObject enemyUiObjectPrefab;

        private const float PollingRate = 0.05f;
        private const int EnemyRange = 20;
        
        private float _pollingTimer = 0f;
        
        private RectTransform _rectTransform;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        private void Update()
        {
            _pollingTimer += Time.deltaTime;
            if (_pollingTimer < PollingRate) return;
            
            
            _pollingTimer = 0f;

            var enemies = GameManager.GetEntitiesInRange(Crystal.Instance.transform.position, EnemyRange).Where(e => e is Enemy).ToList();

            foreach (var (k, v) in _uiObjects.ToList())
            {
                if (enemies.Contains(k))
                {
                    _uiObjects[k].anchoredPosition = WorldToPanel(k.transform.position);
                }
                else
                {
                    _uiObjects[k].gameObject.SetActive(false);
                    _inactiveUiObjects.Add(v.gameObject);
                    _uiObjects.Remove(k);
                }
            }

            var newEnemies = enemies.Where(e => !_uiObjects.ContainsKey(e)).ToList();

            var objCount = _inactiveUiObjects.Count;
            
            // Can be covered by our already instantiated objects
            for (var i = 0; i < objCount; i++)
            {
                if (newEnemies.Count <= i) break;
                
                var obj = _inactiveUiObjects[0];
                
                var rt = obj.GetComponent<RectTransform>();
                
                obj.SetActive(true);
                rt.anchoredPosition = WorldToPanel(newEnemies[i].transform.position);
                
                _uiObjects.Add(newEnemies[i], rt);

                _inactiveUiObjects.RemoveAt(0);
            }
            
            // Leftovers that need new objects
            for (var i = objCount; i < newEnemies.Count; i++)
            {
                var rt = Instantiate(enemyUiObjectPrefab, transform).GetComponent<RectTransform>();
                //Debug.Log(WorldToPanel(newEnemies[i].transform.position));
                rt.anchoredPosition = WorldToPanel(newEnemies[i].transform.position);
                _uiObjects.Add(newEnemies[i], rt);
            }
        }

        private Vector2 WorldToPanel(Vector2 pos)
        {
            var relativeTo = (pos - (Vector2)Crystal.Instance.transform.position) / EnemyRange;
            
            return relativeTo * new Vector2(_rectTransform.rect.width, _rectTransform.rect.height);
        }
    }
}
