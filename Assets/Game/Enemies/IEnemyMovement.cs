using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEnemyMovement : IMovement
{
    abstract Transform Target { get; }
}
