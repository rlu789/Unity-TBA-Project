using UnityEngine;
[System.Serializable]
public class UnitAction {

    public string name = "";
    public int manaCost = 0;
    public int healthCost = 0;
    public int range = 0;   //minRange - maxRange
    public int damage = 0;  //minDamage - maxDamage
    public int aoe = 0;
    public int cooldown = 0;

    [HideInInspector]
    public int currentCooldown = 0;

    //List<Buffs> buffs;  //buffs/debuffs to apply to units
    //List<Effects> effects;  //effects to apply to the terrain

    //public void AddAction(UnitAction action, Unit unit) { } //add actions attached to items/equipment to a unit when it is spawned
    //public void UseAction() { } //uses action
    //public void ReduceCooldown() { } //use at the end of each turn
}
