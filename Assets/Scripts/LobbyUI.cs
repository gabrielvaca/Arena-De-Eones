using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LobbyUI : MonoBehaviour
{
    [Header("Referencias UI")]
    public Button btnCrear;
    public Button btnUnirse;
    public TMP_InputField inputCodigo;

    [Header("Paneles y Textos")]
    public GameObject panelBotones; // El panel que tiene los botones (se ocultará)
    public TMP_Text textoCodigoDisplay; // El texto del código (debe quedarse visible)

    void Start()
    {
        // Asignamos funciones a los botones
        btnCrear.onClick.AddListener(CrearPartida);
        btnUnirse.onClick.AddListener(UnirsePartida);

        // Nos suscribimos al evento del RelayManager
        if (RelayManager.Instance != null)
        {
            RelayManager.Instance.OnMatchmakingComplete += AlCompletarConexion;
        }

        // Estado inicial
        textoCodigoDisplay.gameObject.SetActive(false);
        if (panelBotones != null) panelBotones.SetActive(true);
    }

    void OnDestroy()
    {
        // Limpieza de eventos para evitar errores
        if (RelayManager.Instance != null)
        {
            RelayManager.Instance.OnMatchmakingComplete -= AlCompletarConexion;
        }
    }

    // --- BOTÓN CREAR ---
    void CrearPartida()
    {
        btnCrear.interactable = false;
        textoCodigoDisplay.gameObject.SetActive(true);
        textoCodigoDisplay.text = "Generando código...";

        // Solo llamamos a crear. El resto pasa en 'AlCompletarConexion'
        RelayManager.Instance.CreateRelay();
    }

    // --- BOTÓN UNIRSE ---
    void UnirsePartida()
    {
        string codigo = inputCodigo.text;

        if (string.IsNullOrEmpty(codigo) || codigo.Length < 6)
        {
            Debug.Log("Código inválido");
            return;
        }

        btnUnirse.interactable = false;
        textoCodigoDisplay.text = "Conectando...";
        textoCodigoDisplay.gameObject.SetActive(true);

        RelayManager.Instance.JoinRelay(codigo);
    }

    // --- ESTO SE EJECUTA CUANDO RELAYMANAGER TERMINA ---
    void AlCompletarConexion()
    {
        // 1. Mostrar el código si soy el Host (o si ya lo tengo)
        string codigo = RelayManager.Instance.joinCodeMostrable;
        if (!string.IsNullOrEmpty(codigo))
        {
            textoCodigoDisplay.text = "CÓDIGO DE SALA: " + codigo;
            textoCodigoDisplay.gameObject.SetActive(true);
        }

        // 2. Ocultar los botones para que no molesten
        if (panelBotones != null)
        {
            panelBotones.SetActive(false);
        }
    }
}