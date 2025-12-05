using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardPlayController : MonoBehaviour
{
    public static CardPlayController Instance;

    [Header("Configuración")]
    public LayerMask groundLayer; // Capa del suelo (Floor) para saber dónde spawnear

    private CardData currentCardData;   // Datos de la carta que estamos arrastrando
    private CardDisplay currentCardUI;  // Referencia visual para destruirla/reciclarla luego

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    // 1. Empieza el arrastre: Guardamos qué carta es
    public void StartDrag(CardData data, CardDisplay ui)
    {
        currentCardData = data;
        currentCardUI = ui;
    }

    // 2. Termina el arrastre: Intentamos poner la tropa
    public void EndDrag()
    {
        if (currentCardData == null) return;

        // Lanzar rayo desde la cámara a la posición del mouse
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // Verificamos si el rayo chocó con el suelo (Layer Ground)
        if (Physics.Raycast(ray, out hit, 1000f, groundLayer))
        {
            // Validar si hay maná suficiente
            if (ManaManager.Instance.TrySpendMana(currentCardData.costoMana))
            {
                SpawnUnit(hit.point);

                // Opcional: Avisar al DeckManager que la carta se usó (para borrarla de la mano)
                // DeckManager.Instance.DiscardCard(currentCardUI); 
                // Por ahora solo la destruimos visualmente para probar:
                Destroy(currentCardUI.gameObject);
            }
            else
            {
                Debug.Log("¡No hay suficiente maná!");
            }
        }
        else
        {
            Debug.Log("Posición inválida (No soltaste la carta en el suelo)");
        }

        // Limpiar referencias
        currentCardData = null;
        currentCardUI = null;
    }

    void SpawnUnit(Vector3 position)
    {
        if (currentCardData.prefabUnidad != null)
        {
            Instantiate(currentCardData.prefabUnidad, position, Quaternion.identity);
            Debug.Log($"Unidad invocada: {currentCardData.nombreCarta}");
        }
        else
        {
            Debug.LogError("¡La carta no tiene Prefab asignado en el ScriptableObject!");
        }
    }
}
