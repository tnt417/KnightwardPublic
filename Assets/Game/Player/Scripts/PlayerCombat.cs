using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerShootController))]
public class PlayerCombat : MonoBehaviour
{
    [SerializeField] private PlayerShootController _playerShootController;
}
