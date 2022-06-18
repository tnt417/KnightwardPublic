using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSlashController : MonoBehaviour
{
    [SerializeField] private Animator combatAnimator;
    [SerializeField] private GameObject slashObject;
    private DamageComponent slashDamageComponent;
    private Camera mainCamera;
    [SerializeField] private float attackTimerMax;
    private float attackTimer;
    private const float slashRadius = 1f;
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
