using UnityEngine;
using UnityEngine.AI;
using Unity.Netcode;
using System.Linq;

public class UnitAI : NetworkBehaviour
{
    private Unit _unitData;
    private NavMeshAgent agent;
    private Animator animator;
    private string tagObjetivo;

    private bool _isStationary;

    private float _attackRange;
    private float _fireRate;
    private int _damage;
    private GameObject _projectilePrefab;
    private Transform _firePoint;
    private LayerMask _targetLayers;
    private float _detectionRange;
    private const float TOWER_ATTACK_RANGE = 4.0f;

    private float _fireCooldown;
    private Transform _currentTarget;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        _unitData = GetComponent<Unit>();
        agent = GetComponent<NavMeshAgent>();

        if (_unitData != null)
        {
            _attackRange = _unitData.AttackRange;
            _fireRate = _unitData.FireRate;
            _damage = _unitData.Damage;
            _projectilePrefab = _unitData.ProjectilePrefab;
            _firePoint = _unitData.FirePoint;
            _targetLayers = _unitData.TargetLayers;
            _detectionRange = _unitData.DetectionRange;

            _isStationary = _unitData.IsStationary;

            if (agent != null)
            {
                agent.stoppingDistance = _attackRange * _unitData.StoppingDistanceMultiplier;
            }

            if (_isStationary && agent != null)
            {
                Destroy(agent);
                agent = null;
            }
        }
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        if (OwnerClientId == 0)
        {
            tagObjetivo = "TorreP2";
        }
        else
        {
            tagObjetivo = "TorreP1";
        }
    }

    void Update()
    {
        if (!IsServer) return;

        UpdateTarget();

        if (_currentTarget != null)
        {
            float distToTarget = Vector3.Distance(transform.position, _currentTarget.position);

            float effectiveAttackRange = _attackRange;
            if (_currentTarget.GetComponent<Tower>() != null)
            {
                effectiveAttackRange = TOWER_ATTACK_RANGE;
            }

            if (distToTarget <= effectiveAttackRange)
            {
                if (!_isStationary && agent != null && agent.isActiveAndEnabled) agent.isStopped = true;

                Vector3 lookPos = _currentTarget.position - transform.position;
                lookPos.y = 0;
                Quaternion rotation = Quaternion.LookRotation(lookPos);
                transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * 5f);

                HandleShooting();
            }
            else
            {
                if (_isStationary) return;

                if (agent != null && agent.isActiveAndEnabled && agent.isStopped)
                {
                    agent.isStopped = false;
                }
                agent.SetDestination(_currentTarget.position);
            }
        }
        else
        {
            if (_isStationary) return;

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

            if (_currentTarget.GetComponent<Health>()?.IsAlive == false || dist > _detectionRange)
            {
                _currentTarget = null;
            }
            else
            {
                return;
            }
        }

        LayerMask targetMask = _targetLayers;
        Collider[] hits = Physics.OverlapSphere(transform.position, _detectionRange, targetMask);

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

        if (best != null)
        {
            _currentTarget = best;
            return;
        }
    }

    private void HandleShooting()
    {
        _fireCooldown -= Time.deltaTime;
        if (_fireCooldown > 0f) return;

        _fireCooldown = 1f / _fireRate;

        if (_currentTarget == null) return;

        ApplyDamageToTarget();
        PlayAttackAnimationClientRpc();
    }

    private void ApplyDamageToTarget()
    {
        Health targetHealth = _currentTarget.GetComponent<Health>();
        if (targetHealth != null)
        {
            targetHealth.RequestTakeDamageServerRpc(_damage);
        }
    }

    [ClientRpc]
    private void PlayAttackAnimationClientRpc()
    {
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        if (_projectilePrefab != null && _firePoint != null && _currentTarget != null)
        {
            SpawnVisualProjectileClientRpc(_firePoint.position, _firePoint.rotation, _currentTarget.GetComponent<NetworkObject>().NetworkObjectId);
        }
    }

    [ClientRpc]
    private void SpawnVisualProjectileClientRpc(Vector3 position, Quaternion rotation, ulong targetNetId)
    {
        if (_projectilePrefab == null || NetworkManager.Singleton == null || !NetworkManager.Singleton.SpawnManager.SpawnedObjects.ContainsKey(targetNetId)) return;

        NetworkObject targetNetObj = NetworkManager.Singleton.SpawnManager.SpawnedObjects[targetNetId];

        GameObject projObj = Instantiate(_projectilePrefab, position, rotation);

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
            _currentTarget = masCercano.transform;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (_unitData == null)
        {
            _unitData = GetComponent<Unit>();
            if (_unitData == null) return;
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _unitData.DetectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _unitData.AttackRange);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, TOWER_ATTACK_RANGE);
    }
}