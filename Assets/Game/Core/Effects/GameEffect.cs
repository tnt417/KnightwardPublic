using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Global.Console;
using System.Reflection;
using TonyDev.Game.Core.Entities.Player;
using UnityEngine;
using UnityEngine.Serialization;
using Tools = TonyDev.Game.Global.Tools;

namespace TonyDev.Game.Core.Effects
{
    [Serializable]
    public class GameEffect
    {
        [TextArea] public string effectDescription;
        
        //Dictionary to hold references to game effects to allow sending references to GameEffects across the network
        public static Dictionary<string, GameEffect> GameEffectIdentifiers = new ();

        //Identifier to hold reference to game effect to allow sending references to GameEffect across the network
        [NonSerialized] public string EffectIdentifier;

        [NonSerialized] public GameEntity Entity; //The entity affected by the effect
        [NonSerialized] public GameEntity Source; //The entity that inflicted the effect

        public virtual void OnAddServer() {} //Called on the server when the effect is added to an entity
        public virtual void OnRemoveServer() {} //Called on the server when the effect is removed from an entity
        public virtual void OnUpdateServer() {} //Called on the server when the effect updates on an entity
        
        public virtual void OnAddOwner() {} //Called on the entity's owner client when the effect is added to an entity
        public virtual void OnRemoveOwner() {} //Called on the entity's owner client when the effect is removed from an entity
        public virtual void OnUpdateOwner() {} //Called on the entity's owner client when the effect updates on an entity
        
        #region Static
        public static List<Type> GameEffectTypes;

        static GameEffect()
        {
            GameEffectTypes = Tools.GetTypes<GameEffect>();
        }

        public static GameEffect CreateEffect<T>() where T : GameEffect
        {
            return Activator.CreateInstance(typeof(T)) as GameEffect;
        }
        
        public static GameEffect CreateEffect(string typeName)
        {
            var type = GameEffectTypes.FirstOrDefault(t => t.Name == typeName);
            
            if (type == null)
            {
                Debug.LogWarning("Invalid type name!");
                return null;
            }
            
            return Activator.CreateInstance(type) as GameEffect;
        }

        public static Type GetType(string typeName)
        {
            return GameEffectTypes.FirstOrDefault(t => t.Name == typeName);
        }
        #endregion
    }
}