using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;

    [Header("Configuración")]
    [SerializeField] private float partidaDuracion = 90f; // 1:30 minutos

    [Header("Referencias de Torres")]
    public Health redKingTower;
    public List<Health> redPrincessTowers;
    public Health blueKingTower;
    public List<Health> bluePrincessTowers;

    [Header("Interfaz (UI)")]
    public TextMeshProUGUI timerText;
    public GameObject winPanel;
    public TextMeshProUGUI winnerText;

    // YA NO NECESITAMOS EL BOTÓN
    // public Button startButton; 

    // Variables de Red
    private NetworkVariable<float> currentTimer = new NetworkVariable<float>(90f);
    private NetworkVariable<bool> isGameOver = new NetworkVariable<bool>(false);
    private NetworkVariable<bool> isMatchActive = new NetworkVariable<bool>(false);
    // --- AGREGA ESTA LÍNEA ---
    // Esto permite que otros scripts pregunten "¿Ya empezamos?"
    public bool IsMatchActive => isMatchActive.Value;
    // --------------------------

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        if (winPanel != null) winPanel.SetActive(false);
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            currentTimer.Value = partidaDuracion;

            // SUSCRIBIRSE AL EVENTO DE CONEXIÓN
            // Esto le dice al servidor: "Avísame cuando alguien se conecte"
            NetworkManager.Singleton.OnClientConnectedCallback += CheckPlayersCount;
        }
    }

    public override void OnNetworkDespawn()
    {
        // Limpieza: Nos desuscribimos para evitar errores al salir
        if (IsServer && NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= CheckPlayersCount;
        }
    }

    // Esta función se ejecuta automáticamente cada vez que alguien entra
    private void CheckPlayersCount(ulong clientId)
    {
        if (!IsServer) return;

        // Contamos cuántos jugadores hay conectados
        // El Host cuenta como 1. Cuando entra el Cliente, serán 2.
        int connectedPlayers = NetworkManager.Singleton.ConnectedClients.Count;

        if (connectedPlayers >= 2)
        {
            StartMatch();
        }
    }

    private void StartMatch()
    {
        // Solo iniciamos si no ha empezado ya
        if (!isMatchActive.Value)
        {
            Debug.Log("¡JUGADOR 2 CONECTADO! INICIANDO PARTIDA...");
            isMatchActive.Value = true; // Esto activa el reloj para todos
        }
    }

    private void Update()
    {
        UpdateTimerUI();

        // Lógica del Servidor
        if (IsServer)
        {
            // SI NO HAY 2 JUGADORES, EL TIEMPO NO CORRE
            if (!isMatchActive.Value || isGameOver.Value) return;

            // 1. Restar Tiempo
            currentTimer.Value -= Time.deltaTime;

            // 2. Chequear Torre del Rey (Victoria Instantánea)
            if (!redKingTower.IsAlive) { EndGame("¡GANA EL EQUIPO AZUL!"); return; }
            if (!blueKingTower.IsAlive) { EndGame("¡GANA EL EQUIPO ROJO!"); return; }

            // 3. Chequear Tiempo Agotado
            if (currentTimer.Value <= 0f)
            {
                CheckTimeOutWinner();
            }
        }
    }

    // --- (Resto de funciones de victoria IGUALES) ---

    private void CheckTimeOutWinner()
    {
        int redDestroyed = CountDeadTowers(redPrincessTowers);
        int blueDestroyed = CountDeadTowers(bluePrincessTowers);

        if (blueDestroyed > redDestroyed) EndGame("¡GANA EL EQUIPO ROJO!");
        else if (redDestroyed > blueDestroyed) EndGame("¡GANA EL EQUIPO AZUL!");
        else CheckTieBreaker();
    }

    private int CountDeadTowers(List<Health> towers)
    {
        int count = 0;
        foreach (var t in towers) if (!t.IsAlive) count++;
        return count;
    }

    private void CheckTieBreaker()
    {
        if (redKingTower.CurrentHealth.Value > blueKingTower.CurrentHealth.Value) EndGame("¡GANA EL ROJO!");
        else if (blueKingTower.CurrentHealth.Value > redKingTower.CurrentHealth.Value) EndGame("¡GANA EL AZUL!");
        else EndGame("¡EMPATE!");
    }

    private void EndGame(string winnerMessage)
    {
        isGameOver.Value = true;
        ShowWinPanelClientRpc(winnerMessage);
    }

    [ClientRpc]
    private void ShowWinPanelClientRpc(string message)
    {
        if (winPanel != null)
        {
            winPanel.SetActive(true);
            if (winnerText != null) winnerText.text = message;
        }
    }

    private void UpdateTimerUI()
    {
        if (timerText != null)
        {
            float t = Mathf.Max(0, currentTimer.Value);
            int m = Mathf.FloorToInt(t / 60);
            int s = Mathf.FloorToInt(t % 60);
            timerText.text = string.Format("{0:00}:{1:00}", m, s);

            // Feedback visual: Si la partida no ha empezado, mostramos texto de espera
            if (!isMatchActive.Value && !isGameOver.Value)
            {
                // Opcional: Puedes cambiar el color a amarillo mientras espera
                timerText.color = Color.yellow;
            }
            else
            {
                if (t < 10) timerText.color = Color.red;
                else timerText.color = Color.white;
            }
        }
    }
}