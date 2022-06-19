using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSlashController : MonoBehaviour
{
    [SerializeField] private Animator combatAnimator;
    [SerializeField] private GameObject slashObject;
    [SerializeField] private SpriteRenderer slashSpriteRenderer;
    private DamageComponent _slashDamageComponent;
    private Camera _mainCamera;
    [SerializeField] private float attackTimerMax;
    private float _attackTimer;
    private const float SlashRadius = 1f;
    private void Start()
    {
        _mainCamera = Camera.main;
        _slashDamageComponent = slashObject.GetComponent<DamageComponent>();
    }

    private void Update()
    {
        _attackTimer += Time.deltaTime;
        
        if (Input.GetMouseButton(0) && _attackTimer >= attackTimerMax) Attack();
    }

    private void Attack()
    {
        _attackTimer = 0; //Attack code
        UpdateSlashDirection();
        combatAnimator.Play("SwordSlash");
    }

    private void UpdateSlashDirection()
    {
        Vector2 myPosition = transform.position;
        var direction = ((Vector2)_mainCamera.ScreenToWorldPoint(Input.mousePosition) - myPosition).normalized;
        _slashDamageComponent.SetKnockbackVector(direction);
        slashObject.transform.position = (Vector2) myPosition + direction*SlashRadius;
        slashObject.transform.right = direction;
    }

    private void OnDisable()
    {
        slashSpriteRenderer.sprite = null;
    }
}
