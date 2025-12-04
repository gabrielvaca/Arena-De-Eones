using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifeTime = 5f;

    private Transform _target;
    private int _damage;
    
    public void Initialize(Transform newTarget, int damage)
    {
        _target = newTarget;
        _damage = damage;
    }

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    public void SetTarget(Transform newTarget)
    {
        _target = newTarget;
    }

    private void Update()
    {
        if (_target == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 dir = _target.position - transform.position;
        float distanceThisFrame = speed * Time.deltaTime;

        if (dir.magnitude <= distanceThisFrame)
        {
            HitTarget();
            return;
        }

        transform.Translate(dir.normalized * distanceThisFrame, Space.World);
        transform.forward = dir.normalized;
    }

    private void HitTarget()
    {
        Health targetHealth = _target.GetComponent<Health>();
        if (targetHealth != null)
        {
            targetHealth.TakeDamage(_damage);
        }

        Destroy(gameObject);
    }
}