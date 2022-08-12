using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace TonyDev.Game.Core.Effects.ItemEffects
{
    [AttributeUsage(AttributeTargets.Class)]
    public class GameEffectAttribute : Attribute
    {
        public string ID;
        
        public static readonly Dictionary<string, Type> GameEffectsDictionary = new ();

        static GameEffectAttribute()
        {
            LoadItemEffects();
        }
        
        private static void LoadItemEffects()
        {
            var assembly = Assembly.Load("TonyDev");
                
            var classes = assembly.GetTypes()
                .Where(t => t.IsClass)
                .Where(t => t.GetCustomAttributes(typeof(GameEffectAttribute), false).FirstOrDefault() != null);

            foreach (var c in classes)
            {
                var attr = c.GetCustomAttributes(typeof(GameEffectAttribute), false).FirstOrDefault();
                
                if (attr is GameEffectAttribute itemEffectAttribute)
                {
                    Debug.Log(itemEffectAttribute.ID);
                    GameEffectsDictionary.Add(itemEffectAttribute.ID, c);
                }
            }
        }
    }
}
