using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TonyDev.Game.Global
{
    public class Tools : MonoBehaviour
    {
        public static T SelectRandom<T>(IEnumerable<T> iEnumerable)
        {
            if (iEnumerable == null) return default;
            
            Random.InitState(DateTime.Now.Millisecond);
            
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
    }
}
