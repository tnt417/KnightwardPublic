using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TonyDev.Game.Global
{
    public static class GameTools
    {
        public static T SelectRandom<T>(IEnumerable<T> iEnumerable)
        {
            if (iEnumerable == null) return default;

            var array = iEnumerable.ToArray();
            if (array.Length == 0) return default;
            var index = Random.Range(0, array.Length);
            return array[index];
        }
        
        public static Vector3 GetMeanVector(List<Vector3> positions)
        {
            if(positions.Count == 0)
            {
                return Vector3.zero;
            }
 
            Vector3 meanVector = Vector3.zero;
 
            foreach(Vector3 pos in positions)
            {
                meanVector += pos;
            }
 
            return (meanVector / positions.Count);
        }
        
        public static Dictionary<T, int> SelectRandomNoRepeats<T>(Dictionary<T, int> iEnumerable, out T obj)
        {
            obj = default;
        
            if (iEnumerable == null || iEnumerable.Count == 0)
            {
                return iEnumerable;
            }

            var totalWeight = iEnumerable.Sum(kv => kv.Value);
            var maxWeight = iEnumerable.Max(kv => kv.Value);

            var scrambled = iEnumerable.OrderBy(kv => Random.Range(0, 100));

            if (!iEnumerable.Any(kv => maxWeight - kv.Value > 0))
            {
                obj = SelectRandom(iEnumerable.Keys);
            }
            else
            {
                var totalChance = scrambled.Sum(kv => (float) (maxWeight - kv.Value) / totalWeight);

                var decided = false;

                while (!decided)
                {
                    foreach (var kv in scrambled)
                    {
                        var chance = (float) (maxWeight - kv.Value) / totalWeight;

                        if (Random.Range(0f, totalChance) < chance)
                        {
                            obj = kv.Key;
                            decided = true;
                            break;
                        }
                    }
                }
            }

            if(obj != null) iEnumerable[obj] += 1;

            return iEnumerable;
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
            var colorTag = "#" + ColorUtility.ToHtmlStringRGBA(color);

            return "<color=" + colorTag + ">" + input + "</color>";
        }

        public static bool RollChance(float chance)
        {
            return Random.Range(0f, 1f) < chance;
        }

        public static int RollChanceProgressive(float baseChance, out bool success, int attempts, float chanceRamping)
        {
            attempts += 1;
            success = Random.Range(0f, 1f) < baseChance + attempts * chanceRamping;
            
            return success ? 0 : attempts;
        }
    }
}