using Mirror;
using TonyDev.Game.Core.Entities.Enemies.ScriptableObjects;
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
    }
}
