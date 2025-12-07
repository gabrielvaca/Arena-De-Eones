using UnityEngine;
using Unity.Netcode;

public class CameraSetup : MonoBehaviour
{
    public Vector3 p1Position = new Vector3(0, 15, -15); // Posición normal
    public Vector3 p1Rotation = new Vector3(45, 0, 0);

    public Vector3 p2Position = new Vector3(0, 15, 15);  // Posición espejo (Lado opuesto)
    public Vector3 p2Rotation = new Vector3(45, 180, 0); // Rotada 180 grados

    void Start()
    {
        // Nos suscribimos al evento de conexión para ajustar la cámara cuando entremos
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    private void OnClientConnected(ulong clientId)
    {
        // Solo ajustamos la cámara si somos NOSOTROS los que nos conectamos
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            if (NetworkManager.Singleton.IsServer)
            {
                // Soy el Host (Jugador 1)
                transform.position = p1Position;
                transform.rotation = Quaternion.Euler(p1Rotation);
            }
            else
            {
                // Soy el Cliente (Jugador 2) -> ME VOY AL LADO OSCURO
                transform.position = p2Position;
                transform.rotation = Quaternion.Euler(p2Rotation);
            }
        }
    }

    // Limpieza de eventos
    void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
    }
}