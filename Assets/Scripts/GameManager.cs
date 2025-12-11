using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;

    [Header("Configuración")]
    [SerializeField] private float partidaDuracion = 90f; // 1:30 minutos

    [Header("Referencias de Torres (HOST / ROJO)")]
    public Health redKingTower;
    public List<Health> redPrincessTowers;

    [Header("Referencias de Torres (CLIENTE / AZUL)")]
    public Health blueKingTower;
    public List<Health> bluePrincessTowers;

    [Header("Interfaz (UI)")]
    public TextMeshProUGUI timerText;
    public GameObject winPanel;
    public TextMeshProUGUI winnerText;

    // Variables de Red
    private NetworkVariable<float> currentTimer = new NetworkVariable<float>(90f);
    private NetworkVariable<bool> isGameOver = new NetworkVariable<bool>(false);
    private NetworkVariable<bool> isMatchActive = new NetworkVariable<bool>(false);

    // Propiedad pública para que el ManaManager sepa cuándo arrancar
    public bool IsMatchActive => isMatchActive.Value;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        // Aseguramos que el panel de victoria empiece oculto
        if (winPanel != null) winPanel.SetActive(false);
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            currentTimer.Value = partidaDuracion;

            // 1. Nos suscribimos por si alguien entra tarde (seguridad)
            NetworkManager.Singleton.OnClientConnectedCallback += CheckPlayersCount;

            // 2. ¡EL ARREGLO! Chequeo INMEDIATO
            // Si venimos del menú con Relay, los clientes ya están conectados al cargar la escena.
            // Preguntamos: "¿Cuántos somos AHORA MISMO?"
            if (NetworkManager.Singleton.ConnectedClients.Count >= 2)
            {
                StartMatch();
            }
        }
    }

    public override void OnNetworkDespawn()
    {
        // Limpieza para evitar errores al salir
        if (IsServer && NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= CheckPlayersCount;
        }
    }

    private void CheckPlayersCount(ulong clientId)
    {
        if (!IsServer) return;

        // Doble chequeo por si el evento se dispara después
        if (NetworkManager.Singleton.ConnectedClients.Count >= 2)
        {
            StartMatch();
        }
    }

    private void StartMatch()
    {
        // Solo iniciamos si no ha empezado ya
        if (!isMatchActive.Value)
        {
            Debug.Log("¡2 JUGADORES LISTOS! INICIANDO PARTIDA...");
            isMatchActive.Value = true; // Esto activa el reloj y el Maná
        }
    }

    private void Update()
    {
        UpdateTimerUI();

        // Lógica del Servidor (Árbitro)
        if (IsServer)
        {
            // SI EL JUEGO NO HA EMPEZADO O YA TERMINÓ -> NO HACER NADA
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

    // --- LÓGICA DE VICTORIA ---

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
        // Desempate por vida de la torre central
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

            // Texto amarillo esperando, rojo acabándose, blanco normal
            if (!isMatchActive.Value && !isGameOver.Value) timerText.color = Color.yellow;
            else if (t < 10) timerText.color = Color.red;
            else timerText.color = Color.white;
        }
    }
}