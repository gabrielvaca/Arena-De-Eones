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
    // El orden debe coincidir con el 'Unit Id' de tus ScriptableObjects (CardData)
    public List<GameObject> unitLibrary;

    // Variables temporales para la carta que se está arrastrando
    private CardData currentCardData;
    private CardDisplay currentCardUI;

    private void Awake()
    {
        // Configuración básica del Singleton
        if (Instance == null) Instance = this;
    }

    // 1. Empieza el arrastre: Guardamos los datos de la carta
    public void StartDrag(CardData data, CardDisplay ui)
    {
        currentCardData = data;
        currentCardUI = ui;
    }

    // 2. Termina el arrastre: Intentamos invocar
    public void EndDrag()
    {
        if (currentCardData == null) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // Lanzamos el rayo buscando objetos en la capa "Ground" (donde pusiste tus zonas)
        if (Physics.Raycast(ray, out hit, 1000f, groundLayer))
        {
            bool esZonaValida = false;

            // --- LÓGICA DE ZONAS POR TAGS ---

            // CASO 1: Soy el Host (Jugador 1)
            if (IsServer)
            {
                // Solo es válido si toqué un objeto con la etiqueta ZoneP1
                if (hit.collider.CompareTag("ZoneP1"))
                {
                    esZonaValida = true;
                }
            }
            // CASO 2: Soy el Cliente (Jugador 2)
            else
            {
                // Solo es válido si toqué un objeto con la etiqueta ZoneP2
                if (hit.collider.CompareTag("ZoneP2"))
                {
                    esZonaValida = true;
                }
            }

            // --- RESULTADO ---

            if (esZonaValida)
            {
                // Si la zona es correcta, gastamos maná e invocamos
                if (ManaManager.Instance != null && ManaManager.Instance.TrySpendMana(currentCardData.costoMana))
                {
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
                // Aquí podrías poner un sonido de error o devolver la carta visualmente
            }
        }
        else
        {
            Debug.Log("No tocaste ninguna zona de spawn válida.");
        }

        // Limpieza final
        currentCardData = null;
        currentCardUI = null;
    }

    // 3. Lógica intermedia: Decide si instanciar directo (Host) o pedirlo (Cliente)
    void RequestSpawnUnit(int id, Vector3 position)
    {
        if (IsServer)
        {
            // Si soy el Host, instancio directamente mirando hacia adelante (identidad)
            SpawnUnitLogic(id, position, Quaternion.identity);
        }
        else
        {
            // Si soy Cliente, le pido al servidor que lo haga por mí
            SpawnUnitServerRpc(id, position);
        }
    }

    // 4. El ServerRpc: Se ejecuta en el Servidor cuando el Cliente lo llama
    // RequireOwnership = false permite que cualquier cliente ejecute esto
    [ServerRpc(RequireOwnership = false)]
    private void SpawnUnitServerRpc(int unitId, Vector3 position, ServerRpcParams serverRpcParams = default)
    {
        // Detectamos QUIÉN envió el mensaje
        ulong senderId = serverRpcParams.Receive.SenderClientId;

        // Calculamos la rotación según quién sea
        Quaternion spawnRotation = Quaternion.identity;

        // Si el que envía NO es el Host (es el Cliente/Jugador 2), rotamos 180 grados
        if (senderId != NetworkManager.Singleton.LocalClientId)
        {
            spawnRotation = Quaternion.Euler(0, 180, 0);
        }

        // Ejecutamos la lógica real de instanciación en el servidor
        SpawnUnitLogic(unitId, position, spawnRotation);
    }

    // 5. La Lógica Real: Instancia el Prefab y lo Spawnea en la red
    private void SpawnUnitLogic(int unitId, Vector3 position, Quaternion rotation)
    {
        // Validamos que el ID esté dentro del rango de nuestra lista
        if (unitId >= 0 && unitId < unitLibrary.Count)
        {
            GameObject prefabToSpawn = unitLibrary[unitId];

            if (prefabToSpawn != null)
            {
                // A) Instanciamos el objeto físico en el mundo del servidor
                GameObject spawnedUnit = Instantiate(prefabToSpawn, position, rotation);

                // B) IMPORTANTE: Le decimos a Netcode que este objeto debe existir en todos los clientes
                spawnedUnit.GetComponent<NetworkObject>().Spawn();
            }
        }
        else
        {
            Debug.LogError($"Error: ID {unitId} no encontrado en la Unit Library. Revisa el Inspector.");
        }
    }
}