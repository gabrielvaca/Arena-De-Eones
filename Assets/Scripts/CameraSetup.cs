using UnityEngine;
using Unity.Netcode;

public class CameraSetup : MonoBehaviour
{
    [Header("Posiciones")]
    public Vector3 p1Position = new Vector3(0, 15, -15); // Host (Abajo)
    public Vector3 p1Rotation = new Vector3(45, 0, 0);

    public Vector3 p2Position = new Vector3(0, 15, 15);  // Cliente (Arriba)
    public Vector3 p2Rotation = new Vector3(45, 180, 0); // Rotado 180°

    void Start()
    {
        // NO esperes al evento. Configúrate YA.
        AjustarCamara();
    }

    private void AjustarCamara()
    {
        // Verificamos que el NetworkManager exista (seguridad)
        if (NetworkManager.Singleton == null) return;

        if (NetworkManager.Singleton.IsServer)
        {
            // --- SOY EL HOST (Jugador 1) ---
            transform.position = p1Position;
            transform.rotation = Quaternion.Euler(p1Rotation);
        }
        else if (NetworkManager.Singleton.IsConnectedClient)
        {
            // --- SOY EL CLIENTE (Jugador 2) ---
            transform.position = p2Position;
            transform.rotation = Quaternion.Euler(p2Rotation);
        }
    }
}