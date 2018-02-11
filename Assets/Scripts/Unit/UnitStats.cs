using UnityEngine;

public enum Class { GENERIC, Dude, VERYSmart, HealthyBoy, Pope }

[System.Serializable]
public class UnitStats
{
    public int maxHealth = 20;
    public int baseMoveSpeed = 2;
    public string displayName = "";
    public Class _class = Class.Dude;
    public int maxMana = 3;
    public int armor = 0;
    public int baseInitiative;

    [HideInInspector]
    public int moveSpeed = 2;   //movespeed is determined by the cards for players. For enemies the base value is used
    [HideInInspector]
    public int currentHealth;
    [HideInInspector]
    public int currentMana;
    [HideInInspector]
    public int currentMovement;

    //Modified stats from statuses
    public int modMove;
    public int modIncDamage;
    public int modOutDamage;
    //public int modCardDraw;
    //public int modActionCount;

    [Header("AI Fields")]
    public int idealRange = 1;
    public bool hugFriends = false;

    public void ResetStats()    //call at the top of this units turn
    {
        moveSpeed = baseMoveSpeed;
        modMove = 0;
        modIncDamage = 0;
        modOutDamage = 0;
        //modCardDraw = 0;
        //modActionCount = 0;
    }
}
