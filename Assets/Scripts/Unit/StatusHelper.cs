using UnityEngine;

//The actual things the status can do. Damage over time (each turn), increase movespeed, change action amount, reduce damage done, etc.
public enum StatusType { DOT, MoveSpeed, Actions, OutgoingDamage, IncomingDamage }

[System.Serializable]
public class Effect
{
    public Effect(StatusType _type, int _strength, bool _initialEffect)
    {
        type = _type;
        strength = _strength;
        initialEffect = _initialEffect;
    }

    public StatusType type;
    public int strength;
    public bool initialEffect; //initial effects are applied immediately and then removed
}

[System.Serializable]
public class Status //need to change key value pair so it can be serialized
{
    public Status(string _name, Effect[] _effects, int _duration, GameObject _visual) //add bool for buffs or debuffs
    {
        name = _name;
        effects = _effects;
        duration = _duration;
        visual = _visual;
    }

    public Status(Status prevStatus)
    {
        name = prevStatus.name;
        effects = prevStatus.effects;
        duration = prevStatus.duration;
        visual = prevStatus.visual;
    }

    public string name;
    public Effect[] effects;
    public int duration;
    public GameObject visual;
    public GameObject visualIns;    //instantiated visual effect

    public bool IsEmpty()
    {
        if (name == "" || effects == null || effects.Length == 0) return true;
        return false;
    }
}

public class StatusHelper : MonoBehaviour {

    public static StatusHelper Instance;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("StatusHelper already exists!");
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void CheckStatus(Unit unit)
    {
        //loop backwards so we can remove without affecting loop
        if (unit == null || unit.dead) return;
        for (int i = unit.statuses.Count - 1; i >= 0; --i)
        {
            if (unit == null || unit.statuses == null || unit.statuses.Count == 0 || unit.statuses[i] == null) return; //something weird happened
            if (unit.statuses[i].duration <= 0)
            {
                RemoveStatus(unit, i);
                continue;
            }
            else ApplyEffects(unit.statuses[i], unit);

            //if (unit.statuses[i].duration <= 0) RemoveStatus(unit, i);
        }
    }

    void ApplyEffects(Status status, Unit unit)
    {
        foreach (Effect eff in status.effects)
        {
            if (eff.initialEffect) continue;    //initial effects are check once then skipped
            UseEffect(eff, unit);
        }
        //reduce the effect duration
        status.duration--;
        //TODO: want to do a visual effect here ?
    }

    public void ApplyInitialEffects(Status status, Unit unit)
    {
        foreach (Effect eff in status.effects)
        {
            if (!eff.initialEffect) continue;    //only initial effects are used
            UseEffect(eff, unit);
        }
        //TODO: want to do a visual effect here ?
    }

    void UseEffect(Effect eff, Unit unit)
    {
        switch (eff.type)
        {
            case StatusType.DOT:
                Debug.Log("Damage over time called.");
                unit.TakeDamage(eff.strength);
                break;
            case StatusType.MoveSpeed:
                Debug.Log("Speed change called.");
                break;
            case StatusType.Actions:
                Debug.Log("Action amount change called.");
                break;
            case StatusType.OutgoingDamage:
                Debug.Log("Out. Damage reduction called.");
                break;
            case StatusType.IncomingDamage:
                Debug.Log("Inc. Damage reduction called.");
                break;
        }
    }

    void RemoveStatus(Unit unit, int index)
    {
        ParticleSystem[] psa = unit.statuses[index].visualIns.GetComponentsInChildren<ParticleSystem>();
        foreach (ParticleSystem ps in psa)
        {
            ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        Destroy((unit.statuses[index].visualIns), 2f);
        unit.statuses.RemoveAt(index);
    }

    public void RemoveAllStatuses(Unit unit)
    {
        for (int i = unit.statuses.Count - 1; i >= 0; --i)
        {
            RemoveStatus(unit, i);
        }
    }
}
