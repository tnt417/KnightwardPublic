using System;
using System.Linq;
using Mirror;
using TonyDev.Game.Core.Effects;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Enemies.ScriptableObjects;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Core.Items;
using UnityEngine;
using UnityEngine.U2D;

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

        public static void WriteEnemyData(this NetworkWriter writer, EnemyData value)
        {
            var isNull = value == null;
            writer.WriteBool(isNull);
            if (isNull) return;

            writer.WriteString(value.enemyName);
        }

        public static EnemyData ReadEnemyData(this NetworkReader reader)
        {
            var isNull = reader.ReadBool();
            if (isNull) return null;

            var name = reader.ReadString();

            return ObjectFinder.GetEnemyData(name);
        }

        #region GameEffect

        private static int _identifierIndex;

        private static string GenerateUniqueEffectIdentifier(GameEffect effect)
        {
            if (!string.IsNullOrEmpty(effect
                .EffectIdentifier)) //If supplied identifier is null or empty, need to generate a unique one.
            {
                return effect.EffectIdentifier;
            }
            else
            {
                var id = Player.LocalInstance.netId + "_" + _identifierIndex;
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
            var isNew = string.IsNullOrEmpty(value.EffectIdentifier) || !GameEffect.GameEffectIdentifiers.ContainsKey(value.EffectIdentifier);
            writer.WriteBool(isNew);
            
            if(isNew) //If GameEffect is new, generate an identifier
            {
                var id = GenerateUniqueEffectIdentifier(value);
                value.EffectIdentifier = id;
                GameEffect.GameEffectIdentifiers[id] = value;
            }
            
            writer.WriteString(value.EffectIdentifier);

            if (!isNew) return; //If GameEffect already exists, can just send the identifier

            var isSourceNull = value.Source == null;
            writer.WriteBool(isSourceNull);

            if (!isSourceNull)
            {
                writer.WriteNetworkBehaviour(value.Source);
            }

            var typeName = value.GetType().Name;

            writer.WriteString(typeName);

            foreach (var field in value.GetType().GetFields())
            {
                switch (Type.GetTypeCode(field.FieldType))
                {
                    case TypeCode.Int32:
                        writer.WriteInt((int) field.GetValue(value));
                        break;
                    case TypeCode.Boolean:
                        writer.WriteBool((bool) field.GetValue(value));
                        break;
                    case TypeCode.String:
                        writer.WriteString((string) field.GetValue(value));
                        break;
                    case TypeCode.Double:
                        writer.WriteDouble((double) field.GetValue(value));
                        break;
                    case TypeCode.Single:
                        writer.WriteFloat((float) field.GetValue(value));
                        break;
                    default:
                        break;
                }
            }
        }

        public static GameEffect ReadGameEffect(this NetworkReader reader)
        {
            var isNull = reader.ReadBool();
            if (isNull) return null;

            var isNew = reader.ReadBool();

            var id = reader.ReadString();
            
            if (!isNew)
            {
                return GameEffect.GameEffectIdentifiers[id];
            }

            var isSourceNull = reader.ReadBool();

            GameEntity source = null;

            if (!isSourceNull)
            {
                source = reader.ReadNetworkBehaviour<GameEntity>();
            }

            var typeName = reader.ReadString();

            var gameEffect =
                (GameEffect) Activator.CreateInstance(
                    GameEffect.GameEffectTypes.FirstOrDefault(t => t.Name == typeName) ?? typeof(GameEffect));

            gameEffect.Source = source;

            foreach (var field in gameEffect.GetType().GetFields())
            {
                switch (Type.GetTypeCode(field.FieldType))
                {
                    case TypeCode.Int32:
                        field.SetValue(gameEffect, reader.ReadInt());
                        break;
                    case TypeCode.Boolean:
                        field.SetValue(gameEffect, reader.ReadBool());
                        break;
                    case TypeCode.String:
                        field.SetValue(gameEffect, reader.ReadString());
                        break;
                    case TypeCode.Double:
                        field.SetValue(gameEffect, reader.ReadDouble());
                        break;
                    case TypeCode.Single:
                        field.SetValue(gameEffect, reader.ReadFloat());
                        break;
                    default:
                        break;
                }
            }

            GameEffect.GameEffectIdentifiers[id] = gameEffect;
            
            return gameEffect;
        }

        #endregion
    }
}