using System;
using TonyDev.Game.Global;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TonyDev.Game.Core.Effects
{
    public class ParticleTrailEffect : GameEffect
    {
        [NonSerialized] private GameObject _particleObject;
        public string OverridePrefab;
        public bool VisibleGlobal;

        public override void OnAddOwner()
        {
            if (!VisibleGlobal)
            {
                Create();
            }
        }
        
        public override void OnAddClient()
        {
            if (VisibleGlobal)
            {
                Create();
            }
        }

        private void Create()
        {
            _particleObject = Object.Instantiate(ObjectFinder.GetPrefab(string.IsNullOrEmpty(OverridePrefab) ? "particleTrail" : OverridePrefab), Entity.transform.position, Quaternion.identity, Entity.transform);
            ParticleSystem = _particleObject.GetComponent<ParticleSystem>();
        }

        public override void OnRemoveClient()
        {
            if(_particleObject != null) Object.Destroy(_particleObject);
        }

        [NonSerialized] public ParticleSystem ParticleSystem;

        public void SetColor(Color color)
        {
            if (_particleObject == null) return;
            var settings = ParticleSystem.main;
            settings.startColor = new ParticleSystem.MinMaxGradient(color);
        }
        
        public void SetVisible(bool visible)
        {
            if (_particleObject == null) return;
            
            _particleObject.SetActive(visible);
            
            if(visible) _particleObject.GetComponent<ParticleSystem>()?.Play();
            else _particleObject.GetComponent<ParticleSystem>()?.Stop();
        }
    }
}