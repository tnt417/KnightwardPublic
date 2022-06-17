using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [SerializeField] private Animator combatAnimator;
    [SerializeField] private GameObject slashObject;
    private DamageComponent slashDamageComponent;
    private Camera mainCamera;
    private const float slashRadius = 1f;
    [SerializeField] private float attackTimerMax;
    private float attackTimer;

    private void Start()
    {
        mainCamera = Camera.main;
        slashDamageComponent = slashObject.GetComponent<DamageComponent>();
    }
    
    private void Update()
    {
        attackTimer += Time.deltaTime;
        if (Input.GetMouseButton(0) && attackTimer >= attackTimerMax)
        {
            attackTimer = 0;
            UpdateSlashDirection();
            combatAnimator.Play("SwordSlash");
        }
    }

    private void UpdateSlashDirection()
    {
        Vector2 myPosition = transform.position;
        var direction = ((Vector2)mainCamera.ScreenToWorldPoint(Input.mousePosition) - myPosition).normalized;
        slashDamageComponent.SetKnockbackVector(direction);
        slashObject.transform.position = (Vector2) myPosition + direction*slashRadius;
        slashObject.transform.right = direction;
    }
}
