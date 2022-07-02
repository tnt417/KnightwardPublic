using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
    }
}
