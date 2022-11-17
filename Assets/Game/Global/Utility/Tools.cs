using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TonyDev.Game.Global
{
    public class Tools : MonoBehaviour
    {
        public static T SelectRandom<T>(IEnumerable<T> iEnumerable)
        {
            if (iEnumerable == null) return default;

            var array = iEnumerable.ToArray();
            if (array.Length == 0) return default;
            var index = Random.Range(0, array.Length);
            return array[index];
        }
        
        public static string RemoveWhitespace(string input)
        {
            return new string(input.ToCharArray()
                .Where(c => !char.IsWhiteSpace(c))
                .ToArray());
        }
        
        //Helper function to rotate a vector by radians
        public static Vector2 Rotate(Vector2 v, float radians)
        {
            var sin = Mathf.Sin(radians);
            var cos = Mathf.Cos(radians);

            var tx = v.x;
            var ty = v.y;
            v.x = (cos * tx) - (sin * ty);
            v.y = (sin * tx) + (cos * ty);
            return v;
        }
        
        public static List<Type> GetTypes<T>()
        {
            var assembly = Assembly.Load("TonyDev");

            return assembly.GetTypes().Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(T))).ToList();
        }

        public static string WrapColor(string input, Color color)
        {
            var colorTag = "#"+ColorUtility.ToHtmlStringRGBA(color);

            return "<color=" + colorTag + ">" + input + "</color>";
        }
    }
}
