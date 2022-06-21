using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMovement
{
    public bool DoMovement { get; }
    abstract void UpdateMovement();
    public float SpeedMultiplier { get; }
}
