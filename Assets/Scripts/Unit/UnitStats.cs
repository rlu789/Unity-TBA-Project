using UnityEngine;

public enum Class { GENERIC, Dude, VERYSmart, HealthyBoy, Pope }

[System.Serializable]
public class UnitStats{
    public int maxHealth = 20;
    public int moveSpeed = 2;
    public string displayName = "";
    public Class _class = Class.Dude;
    public int maxMana = 3;
    public int armor = 0;

    [HideInInspector]
    public int currentHealth;
    [HideInInspector]
    public int currentMana;
    [HideInInspector]
    public int currentMovement;

}
