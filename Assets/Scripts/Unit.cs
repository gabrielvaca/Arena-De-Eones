using UnityEngine;
using Unity.Netcode;

public class Unit : NetworkBehaviour
{
    [Header("Combat Stats")]
    [SerializeField] public float AttackRange = 1.5f;
    [SerializeField] public float FireRate = 1f;
    [SerializeField] public int Damage = 10;
    [SerializeField] public GameObject ProjectilePrefab;
    [SerializeField] public Transform FirePoint;

    [Header("Strategy & Targeting")]
    [SerializeField] public float DetectionRange = 5f;
    [SerializeField] public float StoppingDistanceMultiplier = 0.9f;
    [SerializeField] public LayerMask TargetLayers;

    [Header("Movement Control")]
    [SerializeField] public bool IsStationary = false;

    [Header("Identity")]
    public int teamID;

    public override void OnNetworkSpawn()
    {
        teamID = (int)OwnerClientId;

        if (teamID == 0) GetComponentInChildren<Renderer>().material.color = Color.red;
        else GetComponentInChildren<Renderer>().material.color = Color.blue;
    }
}