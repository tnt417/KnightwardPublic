using System;
using System.Linq;
using Mirror;
using TonyDev.Game.Core.Attacks;
using TonyDev.Game.Core.Effects;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Player;
using Unity.Plastic.Newtonsoft.Json;
using UnityEngine;

namespace TonyDev.Game.Global.Network
{
    public static class CustomReadWrite
    {
        public static void WriteSprite(this NetworkWriter writer, Sprite value)
        {
            var isNull = value == null;
            writer.WriteBool(isNull);
            if (isNull) return;

            writer.WriteString(value.name.Split("(")[0]);
        }

        public static Sprite ReadSprite(this NetworkReader reader)
        {
            var isNull = reader.ReadBool();
            if (isNull) return null;

            var spriteName = reader.ReadString();
            var sprite = ObjectFinder.GetSprite(spriteName);

            return sprite;
        }

        #region GameEffect

        private static int _identifierIndex;

        public static string GenerateUniqueEffectIdentifier(GameEffect effect)
        {
            if (!string.IsNullOrEmpty(effect
                .EffectIdentifier)) //If supplied identifier is null or empty, need to generate a unique one.
            {
                return effect.EffectIdentifier;
            }
            else
            {
                var id = Player.LocalInstance.netId + "_" + _identifierIndex + effect.GetType().Name;
                _identifierIndex++;
                return id;
            }
        }

        public static void WriteGameEffect(this NetworkWriter writer, GameEffect value)
        {
            var isNull = value == null;
            writer.WriteBool(isNull);
            if (isNull) return;

            //GameEffect is new if it doesn't have a generated identifier or if GameEffectIdentifiers doesn't contain the identifier
            var isNew = string.IsNullOrEmpty(value.EffectIdentifier) ||
                        !GameEffect.GameEffectIdentifiers.ContainsKey(value.EffectIdentifier);

            if (isNew) //If GameEffect is new, generate an identifier
            {
                GameEffect.RegisterEffect(value);
            }

            writer.WriteString(value.EffectIdentifier);

            var isSourceNull = value.Source == null;
            writer.WriteBool(isSourceNull);

            if (!isSourceNull)
            {
                writer.WriteNetworkBehaviour(value.Source);
            }

            var typeName = value.GetType().Name;

            writer.WriteString(typeName);

            writer.WriteString(JsonConvert.SerializeObject(value, Formatting.None,
                new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    TypeNameHandling = TypeNameHandling.All
                }));

            return;

            foreach (var field in value.GetType().GetFields().Where(f => f.IsPublic && !f.IsNotSerialized))
            {
                if (field.FieldType.IsEnum)
                {
                    writer.WriteInt((int) field.GetValue(value));
                    continue;
                }

                switch (Type.GetTypeCode(field.FieldType))
                {
                    case TypeCode.Int32:
                        writer.WriteInt((int) field.GetValue(value));
                        continue;
                    case TypeCode.Boolean:
                        writer.WriteBool((bool) field.GetValue(value));
                        continue;
                    case TypeCode.String:
                        writer.WriteString((string) field.GetValue(value));
                        continue;
                    case TypeCode.Double:
                        writer.WriteDouble((double) field.GetValue(value));
                        continue;
                    case TypeCode.Single:
                        writer.WriteFloat((float) field.GetValue(value));
                        continue;
                    default:
                        break;
                }

                if (field.FieldType == typeof(Sprite))
                {
                    writer.WriteSprite((Sprite) field.GetValue(value));
                    continue;
                }

                if (field.FieldType == typeof(ProjectileData))
                {
                    writer.Write((ProjectileData) field.GetValue(value));
                    continue;
                }

                writer.WriteString(field.FieldType.Name);
                writer.Write(JsonConvert.SerializeObject(field.GetValue(value), Formatting.None,
                    new JsonSerializerSettings
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    }));
            }
        }

        public static GameEffect ReadGameEffect(this NetworkReader reader)
        {
            // return DeserializeGameEffect(reader.ReadString());

            var isNull = reader.ReadBool();
            if (isNull) return null;

            //var isNew = reader.ReadBool();

            var id = reader.ReadString();

            var isSourceNull = reader.ReadBool();

            GameEntity source = null;

            if (!isSourceNull)
            {
                source = reader.ReadNetworkBehaviour<GameEntity>();
            }

            var typeName = reader.ReadString();

            var gameEffect = GameEffect.GameEffectIdentifiers.ContainsKey(id)
                ? GameEffect.GameEffectIdentifiers[id]
                : //If key already exists in the dictionary, don't create new effect.
                (GameEffect) Activator.CreateInstance(
                    GameEffect.GameEffectTypes.FirstOrDefault(t => t.Name == typeName) ??
                    typeof(GameEffect)); //Otherwise, create a new instance

            JsonConvert.PopulateObject(reader.ReadString(), gameEffect, new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.None,
                TypeNameHandling = TypeNameHandling.All
            });
            
            gameEffect.EffectIdentifier = id;
            
            GameEffect.RegisterEffect(gameEffect);

            return gameEffect;

            gameEffect.Source = source;

            foreach (var field in gameEffect.GetType().GetFields().Where(f => f.IsPublic && !f.IsNotSerialized))
            {
                if (field.FieldType.IsEnum)
                {
                    field.SetValue(gameEffect, reader.ReadInt());
                    continue;
                }

                switch (Type.GetTypeCode(field.FieldType))
                {
                    case TypeCode.Int32:
                        field.SetValue(gameEffect, reader.ReadInt());
                        continue;
                    case TypeCode.Boolean:
                        field.SetValue(gameEffect, reader.ReadBool());
                        continue;
                    case TypeCode.String:
                        field.SetValue(gameEffect, reader.ReadString());
                        continue;
                    case TypeCode.Double:
                        field.SetValue(gameEffect, reader.ReadDouble());
                        continue;
                    case TypeCode.Single:
                        field.SetValue(gameEffect, reader.ReadFloat());
                        continue;
                    default:
                        break;
                }

                if (field.FieldType == typeof(Sprite))
                {
                    field.SetValue(gameEffect, reader.ReadSprite());
                    continue;
                }

                if (field.FieldType == typeof(ProjectileData))
                {
                    field.SetValue(gameEffect, reader.Read<ProjectileData>());
                    continue;
                }

                field.SetValue(gameEffect, JsonConvert.DeserializeObject(reader.ReadString()));
            }

            GameEffect.GameEffectIdentifiers[id] = gameEffect;

            return gameEffect;
        }

        #endregion
    }
}