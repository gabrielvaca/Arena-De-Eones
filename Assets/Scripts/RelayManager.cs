using System; // Necesario para usar 'Action'
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

    public event Action OnMatchmakingComplete;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // No destruir en carga si se mueve entre escenas
            // DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject);
        }
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

        // Este evento maneja cuándo la conexión fue realmente exitosa.
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }

        Debug.Log("Relay Manager: Conectado a Unity Services. ID Jugador: " + AuthenticationService.Instance.PlayerId);
    }

    private void OnDestroy()
    {
        // Limpiamos la suscripción al destruir el objeto
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
    }

    // --- FUNCIÓN PARA EL HOST (CREAR LA SALA) ---
    public async void CreateRelay()
    {
        try
        {
            // ... (código para crear la asignación y obtener joinCode)
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(1); // Solo necesitamos 1 extra para 1v1
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
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

    private void OnClientConnected(ulong clientId)
    {
        // Caso Cliente (Quien se une): 
        // El cliente está conectado cuando su propio ID de cliente (LocalClientId) es reconocido por el NetworkManager.
        if (!NetworkManager.Singleton.IsHost && clientId == NetworkManager.Singleton.LocalClientId)
        {
            // La conexión fue exitosa. Ocultamos el lobby.
            OnMatchmakingComplete?.Invoke();
            Debug.Log("Lobby cerrado: Cliente conectado exitosamente.");

            // Desuscribimos el evento de conexión para que no se dispare de nuevo.
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }

        // Caso Host (Quien crea la sala): 
        // El Host solo debe cerrar el lobby cuando el número de clientes conectados es 2 (Host + 1 Cliente).
        if (NetworkManager.Singleton.IsHost && NetworkManager.Singleton.ConnectedClients.Count == 2)
        {
            // La partida 1v1 está completa. Ocultamos el lobby.
            OnMatchmakingComplete?.Invoke();
            Debug.Log("Lobby cerrado: Segundo jugador detectado (Host). Partida lista.");

            // Desuscribimos el evento de conexión.
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
    }
}