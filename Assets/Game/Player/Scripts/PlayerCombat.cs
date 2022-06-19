using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [SerializeField] private PlayerSlashController playerSlashController;

    private void Update()
    {
        playerSlashController.enabled = Player.Instance.IsAlive;
    }
}
