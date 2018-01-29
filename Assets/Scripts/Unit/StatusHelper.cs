using System.Collections.Generic;
using UnityEngine;

//The actual things the status can do. Damage over time (each turn), increase movespeed, change action amount, reduce damage done, etc.
public enum StatusType { DOT, MoveSpeed, Actions, Damage }

//A status is a collection of StatusType + strength pairs, a duration (rounds) and a visual effect
//eg. Status Stunned = name: Stunned
//                     duration: 2 rounds
//                     visual: some gameobject with the visual effects
//                     pairStatusStrength: MoveSpeed, -100;
//                                         ActionUse, -100;
//Units will have a list of their statusii
[System.Serializable]
public class Status
{
    public Status(string _name, KeyValuePair<StatusType, int>[] _pairStatusStrength, int _duration, GameObject _visual) //add bool for buffs or debuffs
    {
        name = _name;
        pairStatusStrength = _pairStatusStrength;
        duration = _duration;
        visual = _visual;
    }

    public string name;
    public KeyValuePair<StatusType, int>[] pairStatusStrength;
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

    public void ApplyStatus(Unit unit, Status status)
    {
        //put the status in the units list
        //instantiated the visual effect
    }

    public void CheckStatus(Unit unit)
    {
        //loop backwards so we can remove without affecting loop
        for (int i = unit.statuses.Count - 1; i >= 0; --i)
        {
            ApplyStatus(unit.statuses[i], unit);
            if (unit.statuses[i].duration <= 0)
            {
                RemoveStatus(unit, i);
            }
        }
    }

    void ApplyStatus(Status status, Unit unit)
    {
        //apply each effect in the keyValue pair
        foreach (KeyValuePair<StatusType, int> s in status.pairStatusStrength)
        {
            switch (s.Key)
            {
                case StatusType.DOT:
                    Debug.Log("Damage over time called.");
                    unit.TakeDamage(s.Value);
                    break;
                case StatusType.MoveSpeed:
                    Debug.Log("Speed reduction called.");
                    break;
                case StatusType.Actions:
                    Debug.Log("Action amount change called.");
                    break;
                case StatusType.Damage:
                    Debug.Log("Damage reduction called.");
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
