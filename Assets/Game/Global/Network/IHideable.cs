using Mirror;

namespace TonyDev.Game.Global.Network
{
    public interface IHideable
    {
        NetworkIdentity CurrentParentIdentity { get; set; }

        // public bool CompareVisibility(NetworkIdentity other) => other.netId == this.CurrentParentIdentity.netId;
    }
}
