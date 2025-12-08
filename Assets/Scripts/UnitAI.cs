using UnityEngine;
using UnityEngine.AI;
using Unity.Netcode;

public class UnitAI : NetworkBehaviour
{
    private NavMeshAgent agent;
    private string tagObjetivo; // A qué torres voy a atacar

    public override void OnNetworkSpawn()
    {
        // Solo el servidor calcula el movimiento
        if (!IsServer) return;

        agent = GetComponent<NavMeshAgent>();

        // --- LÓGICA DE EQUIPOS ---
        // Si el dueño de esta unidad es el Host (ID 0) -> Ataca Torres P2
        if (OwnerClientId == 0)
        {
            tagObjetivo = "TorreP2";
        }
        // Si el dueño es el Cliente (Cualquier ID > 0) -> Ataca Torres P1
        else
        {
            tagObjetivo = "TorreP1";
        }

        IrATorreMasCercana();
    }

    void IrATorreMasCercana()
    {
        GameObject[] enemigos = GameObject.FindGameObjectsWithTag(tagObjetivo);

        GameObject masCercano = null;
        float distanciaMin = Mathf.Infinity;

        foreach (GameObject enemigo in enemigos)
        {
            float dist = Vector3.Distance(transform.position, enemigo.transform.position);
            if (dist < distanciaMin)
            {
                distanciaMin = dist;
                masCercano = enemigo;
            }
        }

        if (masCercano != null)
        {
            agent.SetDestination(masCercano.transform.position);
        }
    }
}