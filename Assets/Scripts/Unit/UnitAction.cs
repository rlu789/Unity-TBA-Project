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
    [Space(10)]
    public GameObject spawnEffect;
    public int spawnEffectLife;
    public GameObject hitEffect;
    public int hitEffectLife;

    //public float effectDelay;
    //probably want to move all the effect stuff to a bullet/spawner class that will move towards a target and use the effect or wait a delay and use an effect
    //yeah ok not probably ill make that class later its a good idea

    [HideInInspector]
    public int currentCooldown = 0;

    //List<Buffs> buffs;  //buffs/debuffs to apply to units
    //List<Effects> effects;  //effects to apply to the terrain

    public void ActivateAction(Node targetNode, Unit owner)
    {
        if (spawnEffect != null) Object.Destroy(Object.Instantiate(spawnEffect, owner.currentNode.transform.position, Quaternion.identity), spawnEffectLife);
        if (hitEffect != null) Object.Destroy(Object.Instantiate(hitEffect, targetNode.transform.position, Quaternion.identity), hitEffectLife);

        if (healthCost != 0) owner.TakeDamage(healthCost);
        if (manaCost != 0) owner.stats.currentMana -= manaCost;

        if (targetNode.currentUnit == null) return;

        targetNode.currentUnit.TakeDamage(damage);

        //if (action.aoe != 0) ;
        //if (action.cooldown != 0) ;
        //check for current cooldown and decrease it each turn
    }
    //public void AddAction(UnitAction action, Unit unit) { } //add actions attached to items/equipment to a unit when it is spawned
    //public void ReduceCooldown() { } //use at the end of each turn
}
