using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Unity.Netcode;
using UnityEngine.UI;

public class Health : NetworkBehaviour
{
    [SerializeField]
    private int maxHealth = 100;

    public readonly NetworkVariable<int> CurrentHealth = new NetworkVariable<int>(
        1,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    [SerializeField]
    private bool destroyOnDeath = true;

    public UnityEvent OnDamaged;
    public UnityEvent OnDied;

    private EntityVisual _entityVisual;
    private int _maxHealthCache;

    public bool IsAlive => CurrentHealth.Value > 0;

    void Awake()
    {
        _entityVisual = GetComponent<EntityVisual>();
        _maxHealthCache = maxHealth;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            CurrentHealth.Value = _maxHealthCache;
        }

        _entityVisual?.SetMaxHealth(_maxHealthCache);
        _entityVisual?.UpdateHealthBar(CurrentHealth.Value, _maxHealthCache);

        CurrentHealth.OnValueChanged += OnHealthChanged;

        base.OnNetworkSpawn();
    }

    public override void OnNetworkDespawn()
    {
        CurrentHealth.OnValueChanged -= OnHealthChanged;
        base.OnNetworkDespawn();
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestTakeDamageServerRpc(int damageAmount)
    {
        if (!IsServer || CurrentHealth.Value <= 0) return;

        Debug.Log($"[SERVER] {gameObject.name} recibió {damageAmount} de daño. HP restante: {CurrentHealth.Value - damageAmount}");

        CurrentHealth.Value = Mathf.Max(0, CurrentHealth.Value - damageAmount);
    }

    private void OnHealthChanged(int oldHealth, int newHealth)
    {
        _entityVisual?.UpdateHealthBar(newHealth, _maxHealthCache);

        if (newHealth < oldHealth)
        {
            OnDamaged?.Invoke();
        }

        if (newHealth == 0)
        {
            Die();
        }
    }

    private void Die()
    {
        OnDied?.Invoke();

        if (IsServer && destroyOnDeath)
        {
            NetworkObject netObj = GetComponent<NetworkObject>();
            if (netObj != null)
            {
                netObj.Despawn(destroy: true);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}