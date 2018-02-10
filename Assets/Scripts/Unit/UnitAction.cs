using UnityEngine;
using System.Collections.Generic;

public enum ActionType
{
    MOVE,
    ACTION
}

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
    public ActionType type = ActionType.ACTION;
    public Class actionClass = Class.GENERIC;
    public Status status;

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
            ActivateAction(targetNode);
            return;
        }
        TurnHandler.Instance.waitingForAction++;
        GameObject projectileGO = Object.Instantiate(projectile, owner.firePoint.position, Quaternion.identity);
        if (targetNode.currentUnit == null) projectileGO.GetComponent<Projectile>().Setup(this, targetNode.firePoint, targetNode); //if we dont find a target just fire at the node
        else projectileGO.GetComponent<Projectile>().Setup(this, targetNode.currentUnit.firePoint, targetNode);
    }

    void ApplyDamageAndStatus(Unit target)
    {
        if (status != null && !status.IsEmpty()) target.ApplyStatus(status);
        target.TakeDamage(damage);
    }

    public void ActivateAction(Node target)    //only call from projectile
    {
        if (aoe > 0) //enemy aoe attack bugged as the nodesInAOE list is never used
        {
            List<Node> nodesInAOE = GetNodesInRange(target, true);
            foreach (Node n in nodesInAOE)
            {
                if (n.currentUnit != null)
                {
                    ApplyDamageAndStatus(n.currentUnit);
                }
            }
            return;
        }
        if (target.currentUnit == null) return;
        ApplyDamageAndStatus(target.currentUnit);

        //if (action.cooldown != 0) ;
        //check for current cooldown and decrease it each turn
    }

    public List<Node> GetNodesInRange(Node start, bool AOE = false)
    {
        int _range = range;
        if (AOE) _range = aoe;  //checking for aoe not range

        List<Node> nodesInRange = new List<Node>();
        List<Node> tempNodes = new List<Node>();
        nodesInRange.Add(start);
        while (_range > 0)
        {
            foreach (Node n in nodesInRange)
            {
                foreach (Node m in n.neighbours) tempNodes.Add(m);    
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

    public bool IsEmpty()
    {
        return (name == "") ? true : false;
    }
}
