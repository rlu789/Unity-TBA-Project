using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Transform target;
    private Node targetNode;

    public float speed = 70f;
    public GameObject projectileGraphics;
    public GameObject impactEffect;
    public GameObject travelEffect;
    private ParticleSystem[] travelEffectPS;
    public GameObject spawnEffect;
    public float spawnEffectLife;
    public float fireDelay = 0f;
    [Header("Spawn the hit effect from ground level?")]
    public bool hitEffectGrounded = false;
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
            travelEffectPS = travelEffect.GetComponentsInChildren<ParticleSystem>();
            if (projectileGraphics != null && fireDelay > 0) travelEffect.SetActive(false);
        }
        if (spawnEffect != null) Destroy(Instantiate(spawnEffect, transform.position, Quaternion.identity), spawnEffectLife);
        //else if hiteffectgrounded spawn it on the ground (if i want the spawn effects to be grounded also)
    }

    void Update()
    {
        if (fireDelay > 0)
        {
            fireDelay -= Time.deltaTime;
            return;
        }
        else if (projectileGraphics != null)
        {
            projectileGraphics.SetActive(true);
            if (travelEffect) travelEffect.SetActive(true);
        }

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
            travelEffect.transform.LookAt(target);
        }
    }
    void HitTarget()
    {
        GameObject effectIns;
        if (!hitEffectGrounded) effectIns = Instantiate(impactEffect, target.transform.position, transform.rotation);
        else effectIns = Instantiate(impactEffect, targetNode.firePoint.position, transform.rotation);

        Destroy(effectIns, 5f);

        action.ActivateAction(targetNode);

        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        TurnHandler.Instance.waitingForAction--;
        if (travelEffect)
        {
            foreach (ParticleSystem ps in travelEffectPS) ps.Stop();

            Destroy(travelEffect, 2f);
        }
    }
}