using UnityEngine;

namespace TonyDev.Editor
{
    public class EditorStyleTools
    {
        public static Color GetPastelRainbow(int index)
        {
            var rainbow = index % 6;

            return rainbow switch
            {
                0 => new Color(255f/255f, 154/255f, 162/255f, 50/255f),
                1 => new Color(255/255f, 183/255f, 178/255f, 50/255f),
                2 => new Color(255/255f, 218/255f, 193/255f, 50/255f),
                3 => new Color(226/255f, 240/255f, 203/255f, 50/255f),
                4 => new Color(181/255f, 234/255f, 215/255f, 50/255f),
                5 => new Color(199/255f, 206/255f, 234/255f, 50/255f),
                _ => Color.gray
            };
        }
    }
}
