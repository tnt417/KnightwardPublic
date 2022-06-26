using UnityEngine;

namespace TonyDev.Game.Core.Entities.Enemies
{
    public interface IEnemyMovement : IMovement
    {
        abstract Transform Target { get; }
    }
}
