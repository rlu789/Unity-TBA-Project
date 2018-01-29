using UnityEngine;
using System.Collections.Generic;
[System.Serializable]
public class UnitAction {

    public GameObject projectile;
    public string name = "";
    public int manaCost = 0;
    public int healthCost = 0;
    public int range = 0;   //minRange - maxRange
    public int damage = 0;  //minDamage - maxDamage
    public int aoe = 0;
    public int cooldown = 0;
    public int initiative = 0;

    [HideInInspector]
    public int currentCooldown = 0;

    Node targetNode;
    Unit owner;

    public void UseAction(Node _targetNode, Unit _owner)
    {
        targetNode = _targetNode;
        owner = _owner;

        if (healthCost != 0) owner.TakeDamage(healthCost);
        if (manaCost != 0) owner.stats.currentMana -= manaCost;

        SpawnProjectile();
    }

    void SpawnProjectile()
    {
        if (projectile == null)
        {
            ActivateAction();   //only for debug purposes, TODO: class to deal with actions without projectiles
            return;
        }
        GameObject projectileGO = Object.Instantiate(projectile, owner.FirePoint.position, Quaternion.identity);
        if (targetNode.currentUnit == null) projectileGO.GetComponent<Projectile>().Setup(this, owner.FirePoint.position, targetNode.transform); //if we dont find a target just fire at the node
        else projectileGO.GetComponent<Projectile>().Setup(this, owner.FirePoint.position, targetNode.currentUnit.FirePoint);
    }

    public void ActivateAction()    //only call from projectile
    {
        if (targetNode.currentUnit == null) return;

        targetNode.currentUnit.TakeDamage(damage);

        TurnHandler.Instance.singleActionReady = true;
        //if (action.aoe != 0) ;
        //if (action.cooldown != 0) ;
        //check for current cooldown and decrease it each turn
    }

    public List<Node> GetNodesInRange(Node start)
    {
        int _range = range;

        List<Node> nodesInRange = new List<Node>();
        List<Node> tempNodes = new List<Node>();
        nodesInRange.Add(start);
        while (_range > 0)
        {
            foreach (Node n in nodesInRange)
            {
                foreach (Node m in n.neighbours)
                    tempNodes.Add(m);
            }
            foreach (Node n in tempNodes)
            {
                if (!nodesInRange.Contains(n))
                    nodesInRange.Add(n);
            }
            tempNodes.Clear();
            _range--;
        }
        return nodesInRange;
    }

    public bool isEmpty()
    {
        return (name == "") ? true : false;
    }
}
    //public void ReduceCooldown() { } //use at the end of each turn
