namespace TonyDev.Game.Core.Entities.Enemies
{
    public interface IMovement
    {
        public bool DoMovement { get; }
        void UpdateMovement();
         float SpeedMultiplier { get; }
    }
}
