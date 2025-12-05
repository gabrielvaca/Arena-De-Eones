using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Necesario para tocar la UI

public class ManaUI : MonoBehaviour
{
    [Header("Conexiones")]
    public Image manaBarImage; // Aquí arrastras la barra morada

    void Update()
    {
        // Verifica que el ManaManager exista para evitar errores
        if (ManaManager.Instance != null)
        {
            // Calculamos el porcentaje (Ej: 5 maná / 10 max = 0.5 o 50%)
            float percentage = ManaManager.Instance.currentMana / ManaManager.Instance.maxMana;

            // Actualizamos la barra visual
            manaBarImage.fillAmount = percentage;
        }
    }
}
