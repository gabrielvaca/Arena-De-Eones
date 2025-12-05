using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // IMPORTANTE: Necesario para arrastrar

// Agregamos las interfaces de Drag
public class CardDisplay : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Referencias UI")]
    public Image iconImage;
    public Text costText;
    public Image backgroundImage;

    [Header("Datos")]
    public CardData cardData;

    private CanvasGroup canvasGroup; // Para que el rayo atraviese la carta al arrastrar
    private Vector3 originalPosition; // Para volver si no se juega
    private Transform originalParent;

    private void Awake()
    {
        // El CanvasGroup nos ayuda a controlar si la carta bloquea el mouse o no
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void Setup(CardData data)
    {
        cardData = data;
        if (cardData != null)
        {
            iconImage.sprite = data.icono;
            costText.text = data.costoMana.ToString();
            gameObject.name = "UI_" + data.nombreCarta;
        }
    }

    private void Update()
    {
        if (cardData == null || ManaManager.Instance == null) return;

        // Feedback visual de si alcanza el maná
        bool canAfford = ManaManager.Instance.currentMana >= cardData.costoMana;

        // Si la estamos arrastrando, no cambiamos el color
        if (canvasGroup.blocksRaycasts)
        {
            iconImage.color = canAfford ? Color.white : Color.gray;
        }
    }

    // --- INTERFACES DE ARRASTRE ---

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Validar Maná antes de dejar arrastrar
        if (!ManaManager.Instance.HasEnoughMana(cardData.costoMana))
        {
            Debug.Log("No tienes maná suficiente.");
            eventData.pointerDrag = null; // Cancelar arrastre
            return;
        }

        originalPosition = transform.position;
        originalParent = transform.parent;

        // Sacar la carta del layout para que flote libre
        transform.SetParent(transform.root);
        canvasGroup.blocksRaycasts = false; // Permitir que el rayo atraviese la carta y toque el suelo 3D

        // Avisar al controlador
        CardPlayController.Instance.StartDrag(cardData, this);
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition; // La carta sigue al mouse
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        // Avisar al controlador que soltamos
        CardPlayController.Instance.EndDrag();

        // Si la carta no fue destruida (porque no se jugó), volver a la mano
        if (this != null && gameObject != null)
        {
            transform.SetParent(originalParent);
            transform.position = originalPosition;
        }
    }
}