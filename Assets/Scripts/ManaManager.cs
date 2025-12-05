using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManaManager : MonoBehaviour
{
    // Singleton para acceder fácil desde otros scripts (Issue #6)
    public static ManaManager Instance;

    [Header("Configuración")]
    public float maxMana = 10f;
    [Tooltip("1 punto cada 2 segundos = 0.5 por segundo")]
    public float manaRegenRate = 0.5f;
    public float startingMana = 5f; // Según GDD [cite: 147] se inicia con 5

    [Header("Estado Actual (Solo Lectura)")]
    public float currentMana;

    private void Awake()
    {
        // Configuración básica del Singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Criterio QA: El maná empieza en un valor (5 según GDD)
        currentMana = startingMana;
    }

    private void Update()
    {
        // Criterio QA: Regeneración automática
        if (currentMana < maxMana)
        {
            currentMana += manaRegenRate * Time.deltaTime;

            // Criterio QA: Se detiene estrictamente al llegar a 10
            if (currentMana > maxMana)
            {
                currentMana = maxMana;
            }
        }
    }

    // --- MÉTODOS PÚBLICOS (API) ---

    // Método para consultar si tienes suficiente maná (útil para UI o validación)
    public bool HasEnoughMana(int cost)
    {
        return currentMana >= cost;
    }

    // Criterio QA: Método público para consumir el recurso
    public bool TrySpendMana(int cost)
    {
        if (currentMana >= cost)
        {
            currentMana -= cost;
            return true; // Compra exitosa
        }
        else
        {
            Debug.Log("¡No hay suficiente maná!");
            return false; // Compra fallida
        }
    }

    // Extra: Para obtener el valor entero para la UI (ej: mostrar "4" en vez de "4.56")
    public int GetIntegerMana()
    {
        return Mathf.FloorToInt(currentMana);
    }
}
