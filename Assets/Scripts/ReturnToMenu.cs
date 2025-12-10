using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class ReturnToMenu : MonoBehaviour
{
    public void VolverAlMenu()
    {
        // 1. Desconectar limpiamente de la red (seas Host o Cliente)
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
        }

        // 2. Destruir el NetworkManager actual 
        // (Para que al volver al menú se cree uno nuevo y fresco)
        if (NetworkManager.Singleton != null)
        {
            Destroy(NetworkManager.Singleton.gameObject);
        }

        // 3. Cargar la escena del menú
        // Asegúrate de que el nombre coincida EXACTO con tu escena
        SceneManager.LoadScene("MenuPrincipal");
    }
}