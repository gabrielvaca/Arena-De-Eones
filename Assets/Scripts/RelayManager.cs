using UnityEngine;
using Unity.Netcode;
using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using System; // Necesario para 'Action'

public class RelayManager : MonoBehaviour
{
    // Singleton para acceder desde cualquier lado
    public static RelayManager Instance;

    // Evento para avisar a la UI (LobbyUI) que ya terminamos de conectar
    public event Action OnMatchmakingComplete;

    // Variable para guardar el código y mostrarlo en pantalla
    public string joinCodeMostrable;

    private void Awake()
    {
        // Configuración Singleton + DontDestroyOnLoad para sobrevivir al cambio de escena
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private async void Start()
    {
        // Inicializamos los servicios de Unity (Relay/Lobby)
        await UnityServices.InitializeAsync();

        // Nos logueamos como anónimos si no lo estamos ya
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
    }

    // --- LÓGICA DEL HOST (CREAR PARTIDA) ---
    public async void CreateRelay()
    {
        try
        {
            // 1. Crear asignación para 2 jugadores (Host + 1 Invitado)
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(2);

            // 2. Generar el código de unión
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            joinCodeMostrable = joinCode;
            Debug.Log("Código generado: " + joinCode);

            // 3. Configurar el transporte de red con los datos de Relay
            NetworkManager.Singleton.GetComponent<Unity.Netcode.Transports.UTP.UnityTransport>().SetHostRelayData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData
            );

            // 4. Iniciar el Host
            NetworkManager.Singleton.StartHost();

            // 5. Avisar a la UI (LobbyUI) para que oculte botones y muestre el código
            OnMatchmakingComplete?.Invoke();

            // 6. ¡AQUÍ ESTÁ LA CLAVE!
            // No cargamos escena todavía. Activamos el "Vigilante" para esperar al cliente.
            NetworkManager.Singleton.OnClientConnectedCallback += CheckPlayersToStart;
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
        }
    }

    // El "Vigilante": Se ejecuta automáticamente cada vez que alguien entra a la sala
    private void CheckPlayersToStart(ulong clientId)
    {
        // Solo el servidor vigila esto
        if (NetworkManager.Singleton.IsServer)
        {
            Debug.Log($"Jugador conectado. Total actual: {NetworkManager.Singleton.ConnectedClients.Count}");

            // Si ya somos 2 jugadores (El Host + El Cliente que acaba de llegar)
            if (NetworkManager.Singleton.ConnectedClients.Count == 2)
            {
                Debug.Log("¡Sala llena! Iniciando viaje a la Arena...");

                // Nos desuscribimos para que no siga vigilando (limpieza)
                NetworkManager.Singleton.OnClientConnectedCallback -= CheckPlayersToStart;

                // ¡VÁMONOS A LA ARENA!
                // Carga la escena "Arena" y se lleva a todos los jugadores conectados
                NetworkManager.Singleton.SceneManager.LoadScene("Arena", UnityEngine.SceneManagement.LoadSceneMode.Single);
            }
        }
    }

    // --- LÓGICA DEL CLIENTE (UNIRSE) ---
    public async void JoinRelay(string joinCode)
    {
        try
        {
            // 1. Unirse a la asignación con el código
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            // 2. Configurar el transporte
            NetworkManager.Singleton.GetComponent<Unity.Netcode.Transports.UTP.UnityTransport>().SetClientRelayData(
                joinAllocation.RelayServer.IpV4,
                (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.Key,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData
            );

            // 3. Iniciar como Cliente
            NetworkManager.Singleton.StartClient();

            // 4. Avisar a la UI para ocultar el menú
            OnMatchmakingComplete?.Invoke();

            // El cliente NO carga escena. Espera a que el Host (CheckPlayersToStart) lo haga.
        }
        catch (RelayServiceException e)
        {
            Debug.Log("Error al unirse: " + e);
        }
    }
}