using System;
using Newtonsoft.Json;
using UnityEngine;

namespace TonyDev.Game.Global.Network
{
    public class SpriteConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var sprite = value as Sprite;

            writer.WriteValue(sprite == null ? "" : sprite.name.Split("(")[0]);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var name = (string)reader.Value;

            return string.IsNullOrEmpty(name) ? null : ObjectFinder.GetSprite(name);
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Sprite);
        }
    }
}
