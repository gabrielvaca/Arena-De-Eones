using UnityEngine;
using Unity.Netcode;

public class Tower : NetworkBehaviour
{
    [Header("Settings")]
    public int myTeamID;

    [Header("Targeting")]
    [SerializeField] private float range = 5f;
    [SerializeField] private LayerMask targetLayer;
    [SerializeField] private Transform firePoint;

    [Header("Attack")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float fireRate = 1f;
    [SerializeField] private int damage = 10;
    [SerializeField] private Animator animator;

    private EntityVisual _entityVisual;
    private float _fireCooldown;
    private Transform _currentTarget;
    private Health _health;

    private void Awake()
    {
        this._health = GetComponent<Health>();
        if (_health != null) _health.OnDied.AddListener(OnTowerDeath);
        _entityVisual = GetComponent<EntityVisual>();
    }

    void Update()
    {
        if (!IsServer) return;

        if (_health != null && !_health.IsAlive) return;

        UpdateTarget();

        if (_currentTarget == null) return;

        if (_entityVisual)
        {
            _entityVisual.AimAt(_currentTarget.position);
        }

        HandleShooting();
    }

    private void UpdateTarget()
    {
        if (_currentTarget != null)
        {
            float dist = Vector3.Distance(transform.position, _currentTarget.position);
            if (dist > range || _currentTarget.GetComponent<Health>()?.IsAlive == false)
            {
                _currentTarget = null;
            }
            else return;
        }

        // NOTA: La Tropa ya tiene el Collider, por eso esta detección funciona.
        Collider[] hits = Physics.OverlapSphere(transform.position, range, targetLayer);
        Transform best = null;
        float bestDist = Mathf.Infinity;

        foreach (var hit in hits)
        {
            Unit unitScript = hit.GetComponent<Unit>();
            if (unitScript != null && unitScript.teamID == myTeamID) continue;

            float d = Vector3.Distance(transform.position, hit.transform.position);
            if (d < bestDist)
            {
                bestDist = d;
                best = hit.transform;
            }
        }
        _currentTarget = best;
    }

    private void HandleShooting()
    {
        _fireCooldown -= Time.deltaTime;
        if (_fireCooldown > 0f) return;

        _fireCooldown = 1f / fireRate;

        if (projectilePrefab == null || firePoint == null || _currentTarget == null) return;

        PlayAttackAnimationClientRpc();
    }

    [ClientRpc]
    private void PlayAttackAnimationClientRpc()
    {
        if (animator != null)
        {
            animator.SetTrigger("CastSpell");
        }

        // Llamada ClientRpc para instanciar el proyectil visual
        SpawnVisualProjectileClientRpc(firePoint.position, firePoint.rotation, _currentTarget.GetComponent<NetworkObject>().NetworkObjectId);
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

    public void ApplyDamageToTarget() // Llamado desde AnimationEventRelay
    {
        if (!IsServer) return;

        if (_currentTarget == null) return;

        Health targetHealth = _currentTarget.GetComponent<Health>();

        if (targetHealth != null && targetHealth.IsAlive)
        {
            targetHealth.RequestTakeDamageServerRpc(damage);
        }
    }

    private void OnTowerDeath()
    {
        if (IsServer)
        {
            // La destrucción la maneja Health.cs
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}