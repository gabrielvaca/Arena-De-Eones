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

    private TowerVisual _towerVisual;
    private float _fireCooldown;
    private Transform _currentTarget;
    private Health _health;

    private void Awake()
    {
        this._health = GetComponent<Health>();
        if (_health != null) _health.OnDied.AddListener(OnTowerDeath);
        _towerVisual = GetComponent<TowerVisual>();
    }

    void Update()
    {
        if (!IsServer) return; // Solo servidor calcula lógica

        if (_health != null && !_health.IsAlive) return;

        UpdateTarget();

        if (_currentTarget == null) return;

        // NOTA: La rotación (_towerVisual) también solo se ve en el servidor con este código.
        // Si quieres que el cliente vea rotar la torre, necesitarías un NetworkTransform en la parte que gira.
        if (_towerVisual)
        {
            _towerVisual.AimAt(_currentTarget.position);
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

        // CAMBIO IMPORTANTE: Usamos RPC para que todos vean la animación
        PlayAttackAnimationClientRpc();
    }

    // El servidor grita: "¡Animación!" y todos obedecen
    [ClientRpc]
    private void PlayAttackAnimationClientRpc()
    {
        if (animator != null)
        {
            animator.SetTrigger("CastSpell");
        }
    }

    // Esta función la llama el Evento de la Animación (si lo usas)
    // O puedes llamarla directo en HandleShooting si quitas la animación
    public void SpawnProjectile()
    {
        if (!IsServer) return; // Solo servidor crea objetos

        if (projectilePrefab == null || firePoint == null || _currentTarget == null) return;

        GameObject projObj = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);

        Projectile projectile = projObj.GetComponent<Projectile>();
        if (projectile != null) projectile.Initialize(_currentTarget, damage);

        NetworkObject netObj = projObj.GetComponent<NetworkObject>();
        if (netObj != null) netObj.Spawn();
    }

    private void OnTowerDeath()
    {
        if (IsServer)
        {
            if (GetComponent<NetworkObject>() != null) GetComponent<NetworkObject>().Despawn();
            else Destroy(gameObject);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}