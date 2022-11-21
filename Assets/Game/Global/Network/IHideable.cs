using Mirror;

namespace TonyDev.Game.Global.Network
{
    public interface IHideable
    {
        NetworkIdentity CurrentParentIdentity { get; set; }
    }
}
