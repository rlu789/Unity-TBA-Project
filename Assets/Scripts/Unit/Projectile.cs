using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Transform target;
    private Unit targetUnit;

    public float speed = 70f;
    public GameObject impactEffect;
    public GameObject travelEffect;
    private ParticleSystem travelEffectPS;
    public GameObject spawnEffect;
    public float spawnEffectLife;
    
    UnitAction action;
    Vector3 firePoint;

    public void Setup(UnitAction _action, Vector3 _firePoint, Transform _target, Unit _targetUnit)
    {
        action = _action;
        firePoint = _firePoint;
        target = _target;
        targetUnit = _targetUnit;
    }

    private void Start()
    {
        if (travelEffect)
        {
            travelEffect = Instantiate(travelEffect);
            travelEffectPS = travelEffect.GetComponent<ParticleSystem>();
        }
        if (spawnEffect != null) Destroy(Instantiate(spawnEffect, transform.position, Quaternion.identity), spawnEffectLife);
    }

    void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 dir = target.position - transform.position;
        float distanceThisFrame = speed * Time.deltaTime;

        if (dir.magnitude <= distanceThisFrame)
        {
            HitTarget();
            return;
        }
        transform.Translate(dir.normalized * distanceThisFrame, Space.World);
        transform.LookAt(target);
        if (travelEffect)
        {
            travelEffect.transform.position = transform.position;
        }
    }
    void HitTarget()
    {
        GameObject effectIns = Instantiate(impactEffect, transform.position, transform.rotation);

        Destroy(effectIns, 5f);

        action.ActivateAction(targetUnit);

        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        TurnHandler.Instance.waitingForAction--;
        if (travelEffect)
        {
            travelEffectPS.Stop();
            Destroy(travelEffect, 1f);
        }
    }
}