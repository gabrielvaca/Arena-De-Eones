using UnityEngine;
using UnityEngine.AI;
using Unity.Netcode;

public class UnitAI : NetworkBehaviour
{
    private NavMeshAgent agent;
    private Animator animator;
    private string tagObjetivo;

    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float fireRate = 1f;
    [SerializeField] private int damage = 10;

    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;

    private float _fireCooldown;
    private Transform _currentTarget;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        agent = GetComponent<NavMeshAgent>();

        if (OwnerClientId == 0)
        {
            tagObjetivo = "TorreP2";
        }
        else
        {
            tagObjetivo = "TorreP1";
        }

        IrATorreMasCercana();
    }

    void Update()
    {
        if (!IsServer) return;

        UpdateTarget();

        if (_currentTarget != null)
        {
            if (agent != null && agent.isActiveAndEnabled) agent.isStopped = true;

            HandleShooting();
        }
        else
        {
            if (agent != null && agent.isActiveAndEnabled && agent.isStopped)
            {
                agent.isStopped = false;
            }
            IrATorreMasCercana();
        }
    }

    private void UpdateTarget()
    {
        if (_currentTarget != null)
        {
            float dist = Vector3.Distance(transform.position, _currentTarget.position);
            if (dist > attackRange || _currentTarget.GetComponent<Health>()?.IsAlive == false)
            {
                _currentTarget = null;
            }
            else return;
        }

        Collider[] hits = Physics.OverlapSphere(transform.position, attackRange);

        Transform best = null;
        float bestDist = Mathf.Infinity;
        int currentOwnerId = (int)OwnerClientId;

        foreach (var hit in hits)
        {
            Health targetHealth = hit.GetComponent<Health>();
            Unit targetUnit = hit.GetComponent<Unit>();
            Tower targetTower = hit.GetComponent<Tower>();

            int targetTeamID = -1;

            if (targetUnit != null) targetTeamID = targetUnit.teamID;
            else if (targetTower != null) targetTeamID = targetTower.myTeamID;

            if (targetHealth != null && targetHealth.IsAlive && targetTeamID != currentOwnerId)
            {
                float d = Vector3.Distance(transform.position, hit.transform.position);
                if (d < bestDist)
                {
                    bestDist = d;
                    best = hit.transform;
                }
            }
        }
        _currentTarget = best;
    }

    private void HandleShooting()
    {
        _fireCooldown -= Time.deltaTime;
        if (_fireCooldown > 0f) return;

        _fireCooldown = 1f / fireRate;

        if (_currentTarget == null) return;

        ApplyDamageToTarget();
        PlayAttackAnimationClientRpc();
    }

    private void ApplyDamageToTarget()
    {
        Health targetHealth = _currentTarget.GetComponent<Health>();
        if (targetHealth != null)
        {
            targetHealth.RequestTakeDamageServerRpc(damage);
        }
    }

    [ClientRpc]
    private void PlayAttackAnimationClientRpc()
    {
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        if (projectilePrefab != null && firePoint != null && _currentTarget != null)
        {
            SpawnVisualProjectileClientRpc(firePoint.position, firePoint.rotation, _currentTarget.GetComponent<NetworkObject>().NetworkObjectId);
        }
    }

    [ClientRpc]
    private void SpawnVisualProjectileClientRpc(Vector3 position, Quaternion rotation, ulong targetNetId)
    {
        if (projectilePrefab == null || NetworkManager.Singleton == null || !NetworkManager.Singleton.SpawnManager.SpawnedObjects.ContainsKey(targetNetId)) return;

        NetworkObject targetNetObj = NetworkManager.Singleton.SpawnManager.SpawnedObjects[targetNetId];

        GameObject projObj = Instantiate(projectilePrefab, position, rotation);

        Projectile projectile = projObj.GetComponent<Projectile>();
        if (projectile != null) projectile.SetTarget(targetNetObj.transform);
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