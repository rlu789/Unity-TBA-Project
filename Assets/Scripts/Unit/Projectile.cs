using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Transform target;
    private Node targetNode;

    public float speed = 70f;
    public GameObject projectileGraphics;
    public GameObject impactEffect;
    public GameObject travelEffect;
    private ParticleSystem travelEffectPS;
    public GameObject spawnEffect;
    public float spawnEffectLife;
    public float fireDelay = 0f;
    
    UnitAction action;

    public void Setup(UnitAction _action, Transform _target, Node _targetNode)
    {
        action = _action;
        target = _target;
        targetNode = _targetNode;
    }

    private void Start()
    {
        if (projectileGraphics != null && fireDelay > 0) projectileGraphics.SetActive(false);
        if (travelEffect)
        {
            travelEffect = Instantiate(travelEffect);
            travelEffectPS = travelEffect.GetComponent<ParticleSystem>();
        }
        if (spawnEffect != null) Destroy(Instantiate(spawnEffect, transform.position, Quaternion.identity), spawnEffectLife);
    }

    void Update()
    {
        if (fireDelay > 0)
        {
            fireDelay -= Time.deltaTime;
            return;
        }
        else if (projectileGraphics != null) projectileGraphics.SetActive(true);

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

        action.ActivateAction(targetNode);

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