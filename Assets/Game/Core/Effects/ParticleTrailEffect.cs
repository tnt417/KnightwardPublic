using System;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Global;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TonyDev.Game.Core.Effects
{
    public class ParticleTrailEffect : GameEffect
    {
        [NonSerialized] private GameObject _particleObject;

        public override void OnAddOwner()
        {
            _particleObject = Object.Instantiate(ObjectFinder.GetPrefab("particleTrail"), Entity.transform.position, Quaternion.identity, Entity.transform);
        }

        public override void OnRemoveOwner()
        {
            Object.Destroy(_particleObject);
        }

        public void SetColor(Color color)
        {
            if (_particleObject == null) return;
            
            var settings = _particleObject.GetComponent<ParticleSystem>().main;
            settings.startColor = new ParticleSystem.MinMaxGradient(color);
        }
        
        public void SetVisible(bool visible)
        {
            if (_particleObject == null) return;
            
            if(visible) _particleObject.GetComponent<ParticleSystem>()?.Play();
            else _particleObject.GetComponent<ParticleSystem>()?.Stop();
        }
    }
}