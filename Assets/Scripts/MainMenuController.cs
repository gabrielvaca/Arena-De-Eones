using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Paneles")]
    public GameObject panelMenuPrincipal;
    public GameObject panelLobby; // ARRASTRA AQUÍ TU NUEVO PANEL DE LOBBY
    public GameObject panelCreditos;

    private void Start()
    {
        ShowMainMenu();
    }

    public void OnClickJugar()
    {
        // Ocultamos botones principales y mostramos el Lobby (Crear/Unir)
        panelMenuPrincipal.SetActive(false);
        if (panelLobby != null) panelLobby.SetActive(true);
    }

    // Botón "Atrás" (Ponlo en tu Panel Lobby para poder volver)
    public void OnClickVolverAlMenu()
    {
        ShowMainMenu();
    }

    // ... (El resto de tus funciones de Créditos/Salir siguen igual) ...

    private void ShowMainMenu()
    {
        panelMenuPrincipal.SetActive(true);
        if (panelLobby != null) panelLobby.SetActive(false);
        if (panelCreditos != null) panelCreditos.SetActive(false);
    }
    
    public void OnClickCreditos()
    {
        if (panelMenuPrincipal != null) 
    {
        panelMenuPrincipal.SetActive(false);
    }

    // 2. Mostramos el panel de créditos
    if (panelCreditos != null) 
    {
        panelCreditos.SetActive(true);
    }
        
    }

    public void OnClickVolver()
{
    // 1. Ocultamos los créditos
    if (panelCreditos != null) 
    {
        panelCreditos.SetActive(false);
    }

    // 2. Volvemos a mostrar el menú principal
    if (panelMenuPrincipal != null) 
    {
        panelMenuPrincipal.SetActive(true);
    }
}
    public void OnClickSalir() { Application.Quit(); }
}