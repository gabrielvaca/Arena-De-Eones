using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckManager : MonoBehaviour
{
    public static DeckManager Instance;

    [Header("Configuración del Mazo")]
    public List<CardData> fullDeck = new List<CardData>(); // Arrastra aquí tus 8 ScriptableObjects
    
    [Header("Referencias UI")]
    public Transform[] handSlots; // Arrastra los 4 espacios vacíos de la UI (Panel horizontal)
    public GameObject cardPrefab; // El prefab que tiene el script CardDisplay

    // Estructuras de datos internas
    private Queue<CardData> deckQueue = new Queue<CardData>(); // Cola de robo (FIFO)
    private List<CardDisplay> currentHandCards = new List<CardDisplay>(); 

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        InitializeDeck();
        DrawInitialHand();
    }

    // 1. Barajar el mazo y meterlo en una cola
    private void InitializeDeck()
    {
        // Crear una copia temporal para barajar sin alterar el orden original en el inspector
        List<CardData> shuffledDeck = new List<CardData>(fullDeck);
        
        // Algoritmo Fisher-Yates shuffle simple
        for (int i = 0; i < shuffledDeck.Count; i++)
        {
            CardData temp = shuffledDeck[i];
            int randomIndex = Random.Range(i, shuffledDeck.Count);
            shuffledDeck[i] = shuffledDeck[randomIndex];
            shuffledDeck[randomIndex] = temp;
        }

        // Llenar la cola
        foreach (CardData card in shuffledDeck)
        {
            deckQueue.Enqueue(card);
        }
    }

    // 2. Llenar los 4 espacios iniciales
    private void DrawInitialHand()
    {
        // Asumimos que handSlots tiene 4 elementos
        for (int i = 0; i < handSlots.Length; i++)
        {
            if (deckQueue.Count > 0)
            {
                CardData nextCard = deckQueue.Dequeue();
                CreateCardInSlot(nextCard, handSlots[i]);
            }
        }
    }

    // Instancia el prefab visual y configura sus datos
    private void CreateCardInSlot(CardData data, Transform slot)
    {
        GameObject newCardObj = Instantiate(cardPrefab, slot);
        
        // Asegurar que encaje bien en el slot UI
        newCardObj.transform.localPosition = Vector3.zero;
        newCardObj.transform.localScale = Vector3.one;

        // Configurar el script CardDisplay
        CardDisplay display = newCardObj.GetComponent<CardDisplay>();
        if (display != null)
        {
            display.Setup(data);
            currentHandCards.Add(display); // Guardamos referencia por si acaso
        }
    }

    // --- MÉTODOS FUTUROS (Para el siguiente Issue) ---
    /*
    public void OnCardPlayed(CardDisplay cardDisplay)
    {
        // 1. Gastar Maná (ManaManager.Instance.TrySpendMana...)
        // 2. Spawnear Unidad
        // 3. Destruir carta visual de la mano
        // 4. Sacar nueva carta de la deckQueue y ponerla en ese slot
        // 5. Poner la carta usada al final de la deckQueue (Ciclo infinito)
    }
    */
}