using UnityEngine;

namespace TonyDev
{
    public class WishlistButtonController : MonoBehaviour
    {
        public void OnClick()
        {
            Application.OpenURL("https://store.steampowered.com/app/2250460/Knightward/#game_area_purchase");
        }
    }
}
