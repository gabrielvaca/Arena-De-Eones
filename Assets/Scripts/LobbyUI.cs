using UnityEngine;
using UnityEngine.UI;
using TMPro; // Necesario para los textos modernos

public class LobbyUI : MonoBehaviour
{
    [Header("Referencias UI")]
    public Button btnCrear;
    public Button btnUnirse;
    public TMP_InputField inputCodigo;
    public TMP_Text textoCodigoDisplay; // Donde mostraremos el código generado
    public GameObject panelMenu; // Para ocultar el menú al empezar a jugar

    void Start()
    {
        // Asignamos las funciones a los botones
        btnCrear.onClick.AddListener(CrearPartida);
        btnUnirse.onClick.AddListener(UnirsePartida);

        // Ocultamos el texto del código al inicio
        textoCodigoDisplay.text = "Generando código...";
        textoCodigoDisplay.gameObject.SetActive(false);
    }

    void CrearPartida()
    {
        btnCrear.interactable = false; // Evitar doble clic
        textoCodigoDisplay.gameObject.SetActive(true);
        textoCodigoDisplay.text = "Creando sala...";

        // Llamamos al RelayManager
        RelayManager.Instance.CreateRelay();

        // TRUCO: Como CreateRelay es asíncrono, vamos a checar cada segundo
        // si ya tenemos código para mostrarlo en pantalla.
        InvokeRepeating(nameof(ActualizarCodigoEnPantalla), 1f, 1f);
    }

    void UnirsePartida()
    {
        string codigo = inputCodigo.text;

        // Validación simple
        if (string.IsNullOrEmpty(codigo) || codigo.Length < 6)
        {
            Debug.Log("Código inválido, debe tener 6 caracteres");
            return;
        }

        btnUnirse.interactable = false;
        textoCodigoDisplay.text = "Conectando...";
        textoCodigoDisplay.gameObject.SetActive(true);

        RelayManager.Instance.JoinRelay(codigo);

        // Opcional: Ocultar el menú después de unos segundos
        Invoke(nameof(OcultarMenu), 2f);
    }

    void ActualizarCodigoEnPantalla()
    {
        // Esta variable la vamos a agregar al RelayManager en un segundo
        string codigo = RelayManager.Instance.joinCodeMostrable;

        if (!string.IsNullOrEmpty(codigo))
        {
            textoCodigoDisplay.text = "CÓDIGO DE SALA: " + codigo;
            CancelInvoke(nameof(ActualizarCodigoEnPantalla)); // Dejamos de buscar
        }
    }

    void OcultarMenu()
    {
        panelMenu.SetActive(false);
    }
}