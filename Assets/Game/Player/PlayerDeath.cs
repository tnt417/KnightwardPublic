using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDeath : MonoBehaviour
{
    [SerializeField] private float deathCooldown;
    [SerializeField] private GameObject healthBarObject;
    private bool dead;
    private float _deathTimer;
    
    void Update()
    {
        if (dead)
        {
            _deathTimer += Time.deltaTime;
            if (_deathTimer >= deathCooldown)
            {
                Revive();
            }
        }
    }

    public void Die()
    {
        dead = true;
        healthBarObject.SetActive(false);
        Player.Instance.playerAnimator.PlayDeadAnimation();
        foreach (var e in FindObjectsOfType<Enemy>())
        {
            e.UpdateTarget(); //Set new targets for all enemies, so that they don't target the dead player
        }
    }

    private void Revive()
    {
        dead = false;
        _deathTimer = 0;
        healthBarObject.SetActive(true);
        Player.Instance.SetHealth(Player.Instance.MaxHealth);
        foreach (var e in FindObjectsOfType<Enemy>())
        {
            e.UpdateTarget(); //Set new targets for all enemies, so that they might switch back to the player.
        }
    }
}
