using System.Collections.Generic;
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
    bool initialEffect; //initial effects are applied immediately and then removed
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

    public string name;
    public Effect[] effects;
    public int duration;
    public GameObject visual;
    public GameObject visualIns;    //instantiated visual effect
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
        for (int i = unit.statuses.Count - 1; i >= 0; --i)
        {
            if (unit.statuses[i].duration <= 0)
            {
                RemoveStatus(unit, i);
            }
            else ApplyStatus(unit.statuses[i], unit);
        }
    }

    void ApplyStatus(Status status, Unit unit)
    {
        foreach (Effect eff in status.effects)
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
        //reduce the effect duration
        status.duration--;
    }

    void RemoveStatus(Unit unit, int index)
    {
        Destroy(unit.statuses[index].visualIns);
        unit.statuses.RemoveAt(index);
    }
}
