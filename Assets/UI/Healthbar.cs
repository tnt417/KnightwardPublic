using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Healthbar : MonoBehaviour
{
    private IDamageable attachedDamageable;
    [SerializeField] private Slider healthSlider;
    void Start()
    {
        attachedDamageable = GetComponentInParent<IDamageable>();
        
        if (attachedDamageable == null)
        {
            Debug.LogWarning("No attached IDamageable component! Removing healthbar...");
            Destroy(gameObject);
            return;
        }

        healthSlider.maxValue = attachedDamageable.MaxHealth;
        healthSlider.value = attachedDamageable.CurrentHealth;

        attachedDamageable.OnHealthChanged += UpdateUI;
    }

    private void UpdateUI()
    {
        healthSlider.value = attachedDamageable.CurrentHealth;
    }
}
