using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tower : MonoBehaviour
{
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
        _health.OnDied.AddListener(OnTowerDeath);
        _towerVisual =  GetComponent<TowerVisual>();    
    }

    void Update()
    {
        if (!_health.IsAlive) return;
        
        UpdateTarget();
        
        if (_currentTarget == null) return;

        if (_towerVisual)
        {
            Debug.Log(_currentTarget.name);
            _towerVisual.AimAt(_currentTarget.position);
        }
        
        HandleShooting();
    }
    
    private void UpdateTarget()
    {
       
        // if we still have a target, and it's alive, we keep it
        if (_currentTarget != null)
        {
            float dist = Vector3.Distance(transform.position, _currentTarget.position);
            if (dist <= range) return;
        }

        // Search for a new simple objective: the closets 
        Collider[] hits = Physics.OverlapSphere(transform.position, range, targetLayer);

        Transform best = null;
        float bestDist = Mathf.Infinity;

        foreach (var hit in hits)
        {
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

        // Instantiate a projectile in the fire point
        animator.SetTrigger("CastSpell");
    }

    public void SpawnProjectile()
    {
        if(projectilePrefab == null || firePoint == null || _currentTarget == null) return;
        
        GameObject projObj = Instantiate(
            projectilePrefab,
            firePoint.position,
            firePoint.rotation
        );

        Projectile projectile = projObj.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.Initialize(_currentTarget, damage);
        }
    }
    
    private void OnTowerDeath()
    {
        Destroy(gameObject);
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
