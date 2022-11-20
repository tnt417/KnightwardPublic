using System;
using Unity.Plastic.Newtonsoft.Json;
using UnityEngine;

namespace TonyDev.Game.Global.Network
{
    public class SpriteConverter : JsonConverter<Sprite>
    {
        public override void WriteJson(JsonWriter writer, Sprite value, JsonSerializer serializer)
        {
            writer.WriteValue(value == null ? "" : value.name.Split("(")[0]);
        }

        public override Sprite ReadJson(JsonReader reader, Type objectType, Sprite? existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            var name = (string)reader.Value;

            return string.IsNullOrEmpty(name) ? null : ObjectFinder.GetSprite(name);
        }
    }
}
