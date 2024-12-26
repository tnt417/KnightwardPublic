using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TonyDev
{
    public class CelestialBuffAnim : MonoBehaviour
    {
        private List<SpriteRenderer> _childSpriteRenderers;
        [SerializeField] private GameObject subBuffPrefab;

        private float _angleOffset;

        private void Awake()
        {
            _childSpriteRenderers = new List<SpriteRenderer>(GetComponentsInChildren<SpriteRenderer>());
        }

        private int _count;

        public void AlterCount(int delta)
        {
            _count += delta;
            
            var newRenderersNeeded = _count - _childSpriteRenderers.Count;

            for (var i = 0; i < newRenderersNeeded; i++)
            {
                AddRenderer();
            }
        }
        
        private void AddRenderer()
        {
            var go =  GameObject.Instantiate(subBuffPrefab, transform);
            _childSpriteRenderers.Add(go.GetComponent<SpriteRenderer>());
        }

        public void Update()
        {
            _angleOffset += Time.deltaTime * 2f;
            
            for (int i = 0; i < _count; i++)
            {
                _childSpriteRenderers[i].enabled = true;
                var newChildPos =
                    new Vector2(Mathf.Cos(_angleOffset + (((float)i / _count) * 2 * Mathf.PI)) * 0.8f,
                        Mathf.Sin(_angleOffset + (((float)i / _count) * 2 * Mathf.PI)) * 0.65f);
                
                _childSpriteRenderers[i].transform.localPosition = newChildPos;
            }

            for (int i = _count; i < _childSpriteRenderers.Count; i++)
            {
                _childSpriteRenderers[i].enabled = false;
            }
        }
    }
}
