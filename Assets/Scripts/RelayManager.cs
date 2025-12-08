using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using System.Threading.Tasks;

public class RelayManager : MonoBehaviour
{
    // Singleton para acceder desde cualquier script (como LobbyUI)
    public static RelayManager Instance { get; private set; }

    // Variable pública donde guardamos el código para que la UI lo lea
    public string joinCodeMostrable;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private async void Start()
    {
        // 1. Inicializamos la conexión con la nube de Unity
        await UnityServices.InitializeAsync();

        // 2. Nos autenticamos como usuario anónimo
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        Debug.Log("Relay Manager: Conectado a Unity Services. ID Jugador: " + AuthenticationService.Instance.PlayerId);
    }

    // --- FUNCIÓN PARA EL HOST (CREAR LA SALA) ---
    public async void CreateRelay()
    {
        try
        {
            // Creamos una asignación para 3 jugadores extra (4 en total contándote a ti)
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(3);

            // Obtenemos el código de unión (ej: "Q3X9L")
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            // Guardamos el código en la variable pública para que la UI lo muestre
            joinCodeMostrable = joinCode;

            Debug.Log("¡CÓDIGO DE PARTIDA GENERADO: " + joinCode + " !");

            // Le pasamos los datos técnicos al UnityTransport de Netcode
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetHostRelayData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData
            );

            // Iniciamos el Host
            NetworkManager.Singleton.StartHost();
        }
        catch (RelayServiceException e)
        {
            Debug.LogError("Error al crear Relay: " + e);
        }
    }

    // --- FUNCIÓN PARA EL CLIENTE (UNIRSE A LA SALA) ---
    public async void JoinRelay(string joinCode)
    {
        try
        {
            Debug.Log("Intentando unirse con código: " + joinCode);

            // Le pedimos a la nube los datos de conexión usando el código
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            // Configuramos el transporte con los datos recibidos
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetClientRelayData(
                joinAllocation.RelayServer.IpV4,
                (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.Key,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData
            );

            // Iniciamos el Cliente
            NetworkManager.Singleton.StartClient();
        }
        catch (RelayServiceException e)
        {
            Debug.LogError("Error al unirse a Relay: " + e);
        }
    }
}