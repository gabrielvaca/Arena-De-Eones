using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    [SerializeField]
    private int maxHealth = 100;

    [SerializeField]
    private bool destroyOnDeath = true;

    public UnityEvent OnDamaged;
    public UnityEvent OnHealed;
    public UnityEvent OnDied;

    public int CurrentHealth { get; private set; }
    public bool IsAlive => CurrentHealth > 0;

    private bool invulnerable;

    void Awake()
    {
        CurrentHealth = Mathf.Max(1, maxHealth);
    }

    public void TakeDamage(int damage)
    {
        if (damage <= 0 || invulnerable || CurrentHealth <= 0) return;

        CurrentHealth = Mathf.Max(0, CurrentHealth - damage);
        OnDamaged?.Invoke();

        if (CurrentHealth == 0) Die();
    }

    public void Heal(int amount)
    {
        if (amount <= 0 || CurrentHealth <= 0) return;

        CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
        OnHealed?.Invoke();
    }

    private void Die()
    {
        OnDied?.Invoke();
        if (destroyOnDeath) Destroy(gameObject);
    }

    public void SetInvulnerable(float seconds)
    {
        if (seconds <= 0) return;
        StartCoroutine(InvulnerabilityRoutine(seconds));
    }

    private IEnumerator InvulnerabilityRoutine(float seconds)
    {
        invulnerable = true;
        yield return new WaitForSeconds(seconds);
        invulnerable = false;
    }
}