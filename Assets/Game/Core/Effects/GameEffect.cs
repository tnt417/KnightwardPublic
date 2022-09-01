using System;
using JetBrains.Annotations;
using TonyDev.Game.Core.Effects.ItemEffects;
using TonyDev.Game.Core.Entities;

namespace TonyDev.Game.Core.Effects
{
    public abstract class GameEffect
    {
        public GameEntity Entity;
        public abstract void OnAdd([CanBeNull] GameEntity source);
        public abstract void OnRemove();
        public abstract void OnUpdate();

        public static GameEffect FromString(string id)
        {
            return Activator.CreateInstance(GameEffectAttribute.GameEffectsDictionary[id]) as GameEffect;
        }
    }
}
