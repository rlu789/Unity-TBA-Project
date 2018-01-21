using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BaseEnemy {
    public string name;
    
    public enum Type
    {
        Grass,
        Fire,
        Water,
        Electricity
    }

    public enum Rarity
    {
        Common,
        Uncommon,
        Rare,
        SuperRare
    }

    public Type EnemyType;
    public Rarity rarity;

    public float baseHP;
    public float curHP;

    public float baseMP;
    public float curMP;

    public float baseATK;
    public float curATK;
    public float baseDEF;
    public float curDEF;
}
