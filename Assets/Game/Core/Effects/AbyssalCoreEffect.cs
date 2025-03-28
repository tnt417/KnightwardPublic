using TonyDev.Game.Global;
using UnityEngine;

namespace TonyDev.Game.Core.Effects
{
    public class AbyssalCoreEffect : GameEffect
    {
        private GameObject _suckZone;
        
        public override void OnAddServer()
        {
            _suckZone = GameObject.Instantiate(ObjectFinder.GetPrefab("AbyssalCoreSuckZone"), Entity.transform);
        }

        public override void OnRemoveServer()
        {
            GameObject.Destroy(_suckZone);
        }
        
        public override string GetEffectDescription()
        {
            return
                $"<color=#63ab3f>Suck nearby enemies toward you.</color>";
        }
    }
}