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
    }
}
