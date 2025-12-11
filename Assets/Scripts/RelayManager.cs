using System;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine.SceneManagement;

public class RelayManager : MonoBehaviour
{
    public static RelayManager Instance { get; private set; }
    public string joinCodeMostrable;
    public event Action OnMatchmakingComplete;

    // Asegúrate de que este nombre coincida EXACTAMENTE con tu archivo de escena
    private const string SCENE_GAME = "Arena"; 

    private void Awake()
    {
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
        await UnityServices.InitializeAsync();
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }
    }

    // --- HOST ---
    public async void CreateRelay()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(1);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            joinCodeMostrable = joinCode;
            Debug.Log("Código generado: " + joinCode);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetHostRelayData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData
            );

            // 1. Iniciamos el Host PERO NO CARGAMOS LA ESCENA AÚN
            NetworkManager.Singleton.StartHost();
            
            // Aquí nos quedamos en el Menú Principal esperando al jugador 2...
            Debug.Log("Host iniciado. Esperando al Jugador 2...");
        }
        catch (RelayServiceException e)
        {
            Debug.LogError("Error Relay: " + e);
        }
    }

    // --- CLIENTE ---
    public async void JoinRelay(string joinCode)
    {
        try
        {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetClientRelayData(
                joinAllocation.RelayServer.IpV4,
                (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.Key,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData
            );

            NetworkManager.Singleton.StartClient();
        }
        catch (RelayServiceException e)
        {
            Debug.LogError("Error Join Relay: " + e);
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        // Esta función se ejecuta en el Servidor cada vez que alguien se conecta
        if (NetworkManager.Singleton.IsServer)
        {
            Debug.Log($"Jugador conectado: {clientId}");

            // Verificamos si ya hay 2 jugadores (Host + Cliente)
            if (NetworkManager.Singleton.ConnectedClients.Count == 2)
            {
                Debug.Log("¡Jugador 2 conectado! Iniciando partida...");
                // AHORA SÍ cargamos la escena para todos
                NetworkManager.Singleton.SceneManager.LoadScene(SCENE_GAME, LoadSceneMode.Single);
            }
        }

        // Evento para ocultar UI local (opcional)
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            OnMatchmakingComplete?.Invoke();
        }
    }
}