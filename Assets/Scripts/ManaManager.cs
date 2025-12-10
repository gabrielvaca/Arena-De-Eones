using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManaManager : MonoBehaviour
{
    public static ManaManager Instance;

    [Header("Configuración")]
    public float maxMana = 10f;
    [Tooltip("1 punto cada 2 segundos = 0.5 por segundo")]
    public float manaRegenRate = 0.5f;
    public float startingMana = 5f;

    [Header("Estado Actual")]
    public float currentMana;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Iniciamos en 5
        currentMana = startingMana;
    }

    private void Update()
    {
        // --- NUEVA LÓGICA DE ESPERA ---

        // 1. Si no hay GameManager, no sabemos qué hacer, así que no regeneramos.
        if (GameManager.Instance == null) return;

        // 2. Si la partida NO ha empezado (estamos esperando al cliente)...
        if (!GameManager.Instance.IsMatchActive)
        {
            // ... Mantenemos el maná congelado en el valor inicial (5)
            // Esto evita que el Host se llene de maná mientras espera.
            currentMana = startingMana;
            return;
        }

        // ------------------------------

        // 3. Si llegamos aquí, ¡La partida YA empezó! Regeneramos normal.
        if (currentMana < maxMana)
        {
            currentMana += manaRegenRate * Time.deltaTime;

            if (currentMana > maxMana)
            {
                currentMana = maxMana;
            }
        }
    }

    // --- MÉTODOS PÚBLICOS (Igual que antes) ---

    public bool HasEnoughMana(int cost)
    {
        return currentMana >= cost;
    }

    public bool TrySpendMana(int cost)
    {
        if (currentMana >= cost)
        {
            currentMana -= cost;
            return true;
        }
        else
        {
            Debug.Log("¡No hay suficiente maná!");
            return false;
        }
    }

    public int GetIntegerMana()
    {
        return Mathf.FloorToInt(currentMana);
    }
}