using UnityEngine;
using UnityEngine.UI;
// Si usas TextMeshPro, descomenta la siguiente línea:
// using TMPro; 

public class CardDisplay : MonoBehaviour
{
    [Header("Referencias UI")]
    public Image iconImage;       // La imagen de la tropa
    public Text costText;         // El texto del costo (usar Text o TMP_Text)
    public Image backgroundImage; // El fondo de la carta (para cambiar color si no hay maná)
    
    [Header("Datos (Se llenan por código)")]
    public CardData cardData;     // La data que esta carta está mostrando actualmente

    // Referencia al botón para saber si fue clicado (opcional, para selección)
    private Button btn;

    private void Awake()
    {
        btn = GetComponent<Button>();
    }

    // --- SETUP INICIAL ---
    // Este método lo llamará el DeckManager para "inyectar" los datos
    public void Setup(CardData data)
    {
        cardData = data;

        // Asignamos visuales
        if (cardData != null)
        {
            iconImage.sprite = cardData.icono;
            costText.text = cardData.costoMana.ToString();
            gameObject.name = "UI_" + cardData.nombreCarta; // Para orden en la jerarquía
        }
    }

    // --- BUCLE DEL JUEGO ---
    private void Update()
    {
        if (cardData == null || ManaManager.Instance == null) return;

        // Criterio QA Visual: Feedback al usuario sobre el maná
        // Consultamos a tu ManaManager si tenemos suficiente recurso
        bool canAfford = ManaManager.Instance.HasEnoughMana(cardData.costoMana);

        if (canAfford)
        {
            // Estado: DISPONIBLE (Color normal)
            iconImage.color = Color.white;
            backgroundImage.color = Color.white;
        }
        else
        {
            // Estado: NO DISPONIBLE (Oscurecido/Gris)
            iconImage.color = new Color(0.5f, 0.5f, 0.5f, 1f); // Gris oscuro
            backgroundImage.color = Color.gray;
        }
    }

    // Este método se conectará al evento OnClick del botón en el Inspector
    public void OnClickCard()
    {
        if (cardData == null) return;

        // Aquí validamos si podemos seleccionarla
        if (ManaManager.Instance.HasEnoughMana(cardData.costoMana))
        {
            Debug.Log($"Carta seleccionada: {cardData.nombreCarta}. Esperando posición...");
            
            // AQUÍ IRÁ LA LÓGICA DEL SIGUIENTE ISSUE:
            // DeckManager.Instance.SetSelectedCard(this);
        }
        else
        {
            Debug.Log("No tienes suficiente maná para seleccionar esta carta.");
        }
    }
}