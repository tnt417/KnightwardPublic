using TonyDev.Game.Core.Entities;
using TonyDev.Game.Global;
using UnityEngine;

namespace TonyDev.Game.Core.Effects
{
    public class ParticleTrailEffect : GameEffect
    {
        private GameObject particleObject;
        public override void OnAddOwner()
        {
            particleObject = Object.Instantiate(ObjectFinder.GetPrefab("particleTrail"), Entity.transform.position, Quaternion.identity, Entity.transform);
        }

        public override void OnRemoveOwner()
        {
            Object.Destroy(particleObject);
        }

        public void SetColor(Color color)
        {
            var settings = particleObject.GetComponent<ParticleSystem>().main;
            settings.startColor = new ParticleSystem.MinMaxGradient(color);
        }
        
        public void SetVisible(bool visible)
        {
            if (particleObject == null) return;
            
            if(visible) particleObject.GetComponent<ParticleSystem>()?.Play();
            else particleObject.GetComponent<ParticleSystem>()?.Stop();
        }

        public override void OnUpdateOwner()
        {

        }
    }
}