using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode; // Necesario para el multijugador

public class CardPlayController : NetworkBehaviour
{
    public static CardPlayController Instance;

    [Header("Configuración")]
    public LayerMask groundLayer; // Capa del suelo para el Raycast

    [Header("Catálogo de Unidades")]
    // ARRASTRA AQUÍ TUS PREFABS EN ORDEN (Element 0 = DogKnight, Element 1 = Arquero, etc.)
    public List<GameObject> unitLibrary;

    // Variables temporales para la carta que se está arrastrando
    private CardData currentCardData;
    private CardDisplay currentCardUI;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    // 1. Empieza el arrastre
    public void StartDrag(CardData data, CardDisplay ui)
    {
        // Ya no bloqueamos por Owner aquí para permitir arrastrar en UI local
        currentCardData = data;
        currentCardUI = ui;
    }

    // 2. Termina el arrastre: Validamos zona y maná
    public void EndDrag()
    {
        if (currentCardData == null) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 1000f, groundLayer))
        {
            bool esZonaValida = false;

            // --- LÓGICA DE ZONAS POR TAGS ---
            // CASO 1: Soy el Host (Jugador 1)
            if (IsServer)
            {
                if (hit.collider.CompareTag("ZoneP1")) esZonaValida = true;
            }
            // CASO 2: Soy el Cliente (Jugador 2)
            else
            {
                if (hit.collider.CompareTag("ZoneP2")) esZonaValida = true;
            }

            // --- RESULTADO ---
            if (esZonaValida)
            {
                // Si la zona es correcta, gastamos maná
                if (ManaManager.Instance != null && ManaManager.Instance.TrySpendMana(currentCardData.costoMana))
                {
                    // Invocamos la unidad
                    RequestSpawnUnit(currentCardData.unitId, hit.point);
                    Destroy(currentCardUI.gameObject);
                }
                else
                {
                    Debug.Log("No hay suficiente maná.");
                }
            }
            else
            {
                Debug.Log("🚫 Zona inválida: No puedes invocar en territorio enemigo.");
            }
        }
        else
        {
            Debug.Log("No tocaste ninguna zona de spawn válida.");
        }

        currentCardData = null;
        currentCardUI = null;
    }

    // 3. Solicitud de Spawn
    void RequestSpawnUnit(int id, Vector3 position)
    {
        if (IsServer)
        {
            // Si soy el Host, instancio directamente Y me asigno como dueño
            SpawnUnitLogic(id, position, Quaternion.identity, NetworkManager.Singleton.LocalClientId);
        }
        else
        {
            // Si soy Cliente, le pido al servidor
            SpawnUnitServerRpc(id, position);
        }
    }

    // 4. ServerRpc: Recibe la petición del Cliente
    [ServerRpc(RequireOwnership = false)]
    private void SpawnUnitServerRpc(int unitId, Vector3 position, ServerRpcParams serverRpcParams = default)
    {
        // Detectamos QUIÉN envió el mensaje (El ID del cliente)
        ulong senderId = serverRpcParams.Receive.SenderClientId;

        Quaternion spawnRotation = Quaternion.identity;

        // Si es el Cliente (Jugador 2), rotamos la unidad 180 grados
        if (senderId != NetworkManager.Singleton.LocalClientId)
        {
            spawnRotation = Quaternion.Euler(0, 180, 0);
        }

        // Ejecutamos la lógica pasando el senderId como dueño
        SpawnUnitLogic(unitId, position, spawnRotation, senderId);
    }

    // 5. Lógica Real de Spawn (con asignación de dueño)
    // El parámetro ownerId = 999 es un valor por defecto por seguridad
    private void SpawnUnitLogic(int unitId, Vector3 position, Quaternion rotation, ulong ownerId)
    {
        GameObject prefab = unitLibrary[unitId];

        // 1. Instanciar
        GameObject newUnit = Instantiate(prefab, position, rotation);

        // 2. Spawnear CON PROPIEDAD (Esto rellena el OwnerClientId automáticamente)
        var netObj = newUnit.GetComponent<NetworkObject>();

        if (ownerId != 999)
        {
            netObj.SpawnWithOwnership(ownerId);
        }
        else
        {
            netObj.Spawn();
        }
    }
}