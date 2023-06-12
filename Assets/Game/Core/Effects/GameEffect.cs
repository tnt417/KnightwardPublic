using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Global.Console;
using System.Reflection;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Core.Items;
using TonyDev.Game.Global;
using TonyDev.Game.Global.Network;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

namespace TonyDev.Game.Core.Effects
{
    [Serializable]
    public class GameEffectList
    {
        [SerializeReference] public List<GameEffect> gameEffects = new();
    }
    
    [Serializable]
    public class GameEffect
    {
        [TextArea] public string effectDescription;

        //Dictionary to hold references to game effects to allow sending references to GameEffects across the network
        public static Dictionary<string, GameEffect> GameEffectIdentifiers = new();

        //Identifier to hold reference to game effect to allow sending references to GameEffect across the network
        [NonSerialized] public string EffectIdentifier;

        [NonSerialized] public Item Item;
        [NonSerialized] public GameEntity Entity; //The entity affected by the effect
        [NonSerialized] public GameEntity Source; //The entity that inflicted the effect

        [HideInInspector] public int DungeonFloorUponCreation = 0;

        public virtual void OnAddClient()
        {
        } //Called on all clients when the effect is added to an entity

        public virtual void OnRemoveClient()
        {
        } //Called on all clients when the effect is removed from an entity

        public virtual void OnUpdateClient()
        {
        } //Called on all clients when the effect updates on an entity

        public virtual void OnAddServer()
        {
        } //Called on the server when the effect is added to an entity

        public virtual void OnRemoveServer()
        {
        } //Called on the server when the effect is removed from an entity

        public virtual void OnUpdateServer()
        {
        } //Called on the server when the effect updates on an entity

        public virtual void OnAddOwner()
        {
        } //Called on the entity's owner client when the effect is added to an entity

        public virtual void OnRemoveOwner()
        {
        } //Called on the entity's owner client when the effect is removed from an entity

        public virtual void OnUpdateOwner()
        {
        } //Called on the entity's owner client when the effect updates on an entity

        public virtual void OnRegisterLocal()
        {
            if(DungeonFloorUponCreation <= 0) DungeonFloorUponCreation = GameManager.DungeonFloor;
        }
        
        public virtual string GetEffectDescription()
        {
            return effectDescription;
        }

        #region Static

        public static List<Type> GameEffectTypes;

        static GameEffect()
        {
            GameEffectTypes = GameTools.GetTypes<GameEffect>();
        }

        public static GameEffect CreateEffect<T>(Item item) where T : GameEffect
        {
            var effect = Activator.CreateInstance(typeof(T)) as GameEffect;
            RegisterEffect(effect);
            return effect;
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

        public static void RegisterEffect(GameEffect gameEffect)
        {
            if (string.IsNullOrEmpty(gameEffect.EffectIdentifier))
            {
                gameEffect.EffectIdentifier = CustomReadWrite.GenerateUniqueEffectIdentifier(gameEffect);
            }

            if (!GameEffectIdentifiers.ContainsKey(gameEffect.EffectIdentifier))
            {
                gameEffect.OnRegisterLocal();
            }

            GameEffectIdentifiers[gameEffect.EffectIdentifier] = gameEffect;
        }

        protected float LinearScale(float baseValue, float approachValue, int byDungeonFloor)
        {
            return baseValue;
            
            return Mathf.Lerp(baseValue, approachValue,
                Mathf.Clamp01((float) DungeonFloorUponCreation / byDungeonFloor));
        }

        protected float DiminishingScale(float baseValue, float approachValue, int byDungeonFloor)
        {
            return baseValue;
            
            var lerpMagnitude = Mathf.Clamp01(Mathf.Log(DungeonFloorUponCreation, byDungeonFloor));

            return Mathf.Lerp(baseValue, approachValue, lerpMagnitude);
        }

        #endregion
    }
}