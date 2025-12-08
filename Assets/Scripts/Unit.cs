using UnityEngine;
using Unity.Netcode;

public class Unit : NetworkBehaviour
{
    // Esta variable nos dirá de qué equipo es
    public int teamID;

    public override void OnNetworkSpawn()
    {
        // TRUCO MAESTRO:
        // En Netcode, cada jugador tiene una ID única (0, 1, 2...).
        // Usamos esa ID directamente como su equipo.
        // OwnerClientId ya viene sincronizado por Unity.
        teamID = (int)OwnerClientId;

        // Opcional: Cambiar color para debug visual
        if (teamID == 0) GetComponentInChildren<Renderer>().material.color = Color.red; // Host
        else GetComponentInChildren<Renderer>().material.color = Color.blue; // Cliente
    }
}