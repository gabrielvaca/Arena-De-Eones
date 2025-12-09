using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TowerVisual : MonoBehaviour
{
    [SerializeField] private Transform rotatingPart;
    [SerializeField] private float rotationSpeed = 10f;

    [SerializeField] private Slider healthSlider;
    [SerializeField] private GameObject healthBarContainer;


    public void AimAt(Vector3 worldTargetPos)
    {
        if (!rotatingPart) return;

        Vector3 direction = worldTargetPos - rotatingPart.position;
        direction.y = 0f;

        if (direction.magnitude < 0.1f) return;

        Quaternion targetRot = Quaternion.LookRotation(direction);
        rotatingPart.rotation = Quaternion.Lerp(rotatingPart.rotation, targetRot, rotationSpeed * Time.deltaTime);
    }

    public void SetMaxHealth(int maxHP)
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHP;
        }
    }

    public void UpdateHealthBar(int currentHP, int maxHP)
    {
        if (healthSlider == null) return;

        healthSlider.value = (float)currentHP / maxHP;

        if (healthBarContainer != null)
        {
            bool shouldShow = currentHP < maxHP && currentHP > 0;
            healthBarContainer.SetActive(shouldShow);
        }
    }
}