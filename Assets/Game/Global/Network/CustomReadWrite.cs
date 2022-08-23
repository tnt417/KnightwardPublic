using Mirror;
using UnityEngine;
using UnityEngine.U2D;

namespace TonyDev.Game.Global.Network
{
    public static class CustomReadWrite
    {
        public static void WriteSprite(this NetworkWriter writer, Sprite value)
        {
            writer.WriteString(value.name.Split("(")[0]);
        }

        public static Sprite ReadSprite(this NetworkReader reader)
        {
            var spriteName = reader.ReadString();
            var sprite = ObjectFinder.GetSprite(spriteName);

            return sprite;
        }
    }
}
