namespace TonyDev.Game.Core.Entities.Enemies
{
    public interface IMovement
    {
        public bool DoMovement { get; }
        abstract void UpdateMovement();
        public float SpeedMultiplier { get; }
    }
}
