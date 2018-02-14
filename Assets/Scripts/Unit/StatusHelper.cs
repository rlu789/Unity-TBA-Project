using UnityEngine;
using System;

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
public class Status : IComparable<Status>//need to change key value pair so it can be serialized
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

        priority = GetPriority();
    }

    public string name;
    public Effect[] effects;
    public int duration;
    public GameObject visual;
    public GameObject visualIns;    //instantiated visual effect
    [HideInInspector]
    public int priority;

    public bool IsEmpty()
    {
        if (name == "" || effects == null || effects.Length == 0) return true;
        return false;
    }

    public int GetPriority()    //returns the order this status should be used in. Ideal order is like so: Damage modifiers > Healing > Damage
    {                           //currently if a status has a damage modifier and damage, the damage will be done before all the modifiers are applied. This is okay for healing since its not affected but...
        int priority = 0;       //eg. +1 damage taken, +1 damage taken and 3 damage (becomes 5), +1 damage taken. Should have done 6 damage. So avoid this or fix it.
        foreach (Effect eff in effects)
        {
            if (eff.type == StatusType.IncomingDamage) priority += 100;
            if (eff.type == StatusType.DOT) priority -= eff.strength;
        }
        return priority; //list loops backwards for status checking so higher means it happens first
    }

    public int CompareTo(Status s)
    {
        return priority.CompareTo(s.priority);
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
    //TODO: order statuses in least -> most damage with damage modifiers first. That way if you are healing and burning, the healing will save you
    public void CheckStatuses(Unit unit)    //called at start of units turn
    {
        if (unit == null || unit.dead || unit.statuses == null || unit.statuses.Count == 0) return;

        unit.statuses.Sort();

        for (int i = unit.statuses.Count - 1; i >= 0; --i)
        {
            if (unit == null || unit.dead || unit.statuses == null || unit.statuses.Count == 0) return; //probably need to do this every loop in case we call Die(). :thinking: well designed code
            ApplyEffects(unit.statuses[i], unit);                                                       //could change it to -> get damage for this effect, add it to list, at the end of all effects do the damage in succession
                                                                                                        //this also helps order it so things that cant kill will happen first
        }
    }

    public void ResolveStatuses(Unit unit)  //call at end of units turn
    {
        if (unit == null || unit.dead || unit.statuses == null || unit.statuses.Count == 0) return;
        //loop backwards so we can remove without affecting loop
        for (int i = unit.statuses.Count - 1; i >= 0; --i)
        {
            if (unit == null || unit.dead || unit.statuses == null || unit.statuses.Count == 0) return;
            if (unit.statuses[i].duration <= 0)
            {
                RemoveStatus(unit, i);
            }
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

    void UseEffect(Effect eff, Unit unit, int strengthMod = 1)
    {
        switch (eff.type)
        {
            case StatusType.DOT:
                unit.TakeDamage(eff.strength * strengthMod);
                break;
            case StatusType.MoveSpeed:
                unit.stats.modMove += eff.strength * strengthMod;
                if (unit.isEnemy) unit.stats.moveSpeed += eff.strength * strengthMod;
                break;
            case StatusType.Actions:
                //reduce selecting actions variable on nodemanager (or probably change that variable to a variable on the unit instead)
                break;
            case StatusType.OutgoingDamage:
                unit.stats.modOutDamage += eff.strength * strengthMod;
                break;
            case StatusType.IncomingDamage:
                unit.stats.modIncDamage += eff.strength * strengthMod;
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
        UndoStatMod(unit, index);
        unit.statuses.RemoveAt(index);
    }

    void UndoStatMod(Unit unit, int index)
    {
        foreach (Effect eff in unit.statuses[index].effects)
        {
            if (eff.type == StatusType.IncomingDamage || eff.type == StatusType.OutgoingDamage || eff.type == StatusType.Actions || eff.type == StatusType.MoveSpeed)
            {
                if (!eff.initialEffect) UseEffect(eff, unit, -1);
            }
        }
    }

    public void RemoveAllStatuses(Unit unit)
    {
        for (int i = unit.statuses.Count - 1; i >= 0; --i)
        {
            RemoveStatus(unit, i);
        }
    }
}
