using System;
using System.Collections.Generic;
using UnityEngine;

public class PossibleAction : IComparable<PossibleAction>
{
    public PossibleAction(List<Node> _path, UnitAction _action, Node _target, int _fitness)
    {
        path = _path;
        action = _action;
        target = _target;
        fitness = _fitness;
    }

    public void DebugLogMe()
    {
        //Debug.Log("Path is from node (" + path[0].nodeID + ") to node (" + path[path.Count - 1].nodeID + ")");
        //Debug.Log("Target is node (" + target.nodeID + ")");
        Debug.Log("Action is (" + action.name + ")");
        Debug.Log("Fitness score is (" + fitness + ")");
        Debug.Log("Target is " + target.currentUnit);
    }

    public void DetermineFitness()
    {
        List<Node> nodes = action.GetNodesInRange(target, true);    //get all nodes in AOE
        foreach (Node n in nodes)
        {
            fitness += DetermineNodeFitness(n);
        }

        if (fitness <= 0) fitness = 0;
        else if (path[path.Count - 1].DistanceToEnemy() < path[0].DistanceToEnemy()) fitness++;  //if we are moving towards an enemy, increase the fitness.
    }

    int DetermineNodeFitness(Node node)
    {
        int tempFitness = 0;
        if (action == null) return 0;
        Unit targetUnit = node.currentUnit;
        if (targetUnit == null) return 0;

        if (!targetUnit.isEnemy)
        {
            tempFitness += action.damage;
            if (targetUnit.stats.currentHealth - action.damage <= 0) tempFitness += action.damage;  //if it will kill, double fitness
            tempFitness += DetermineStatusFitness(false);
        }
        else
        {
            tempFitness -= action.damage;   //negative damage is healing
            if (targetUnit.stats.currentHealth <= (targetUnit.stats.maxHealth / 2)) tempFitness -= action.damage;    //if under 50% health, double the fitness gain
            if (targetUnit.stats.currentHealth - action.damage > targetUnit.stats.maxHealth) tempFitness -= (targetUnit.stats.currentHealth - action.damage) - targetUnit.stats.maxHealth;  //only fitness for the actual health regained, not overheal
            tempFitness += DetermineStatusFitness(true);
        }
        tempFitness -= action.healthCost;
        tempFitness -= action.manaCost * 2;
        return tempFitness;
    }

    int DetermineStatusFitness(bool enemy)
    {
        if (action.status == null) return 0;
        if (action.status.IsEmpty()) return 0;

        int e = (enemy) ? -1 : 1;   //treat normal fitness gain on an ally as opposite
        int tempFitness;
        int ret = 0;

        foreach (Effect eff in action.status.effects)
        {
            tempFitness = 0;
            if (eff.type == StatusType.DOT) tempFitness += eff.strength;
            if (eff.type == StatusType.IncomingDamage) tempFitness += eff.strength * 2;
            if (eff.type == StatusType.OutgoingDamage || eff.type == StatusType.MoveSpeed) tempFitness -= eff.strength;
            if (eff.type == StatusType.Actions) tempFitness -= eff.strength * 2;

            if (!eff.initialEffect) tempFitness *= action.status.duration;  //multiply the fitness by how long it lasts
            else tempFitness = 0;   //AI don't value intitial effects for now

            tempFitness *= e;   //what is good against an enemy is bad to a friend

            ret += tempFitness;
        }
        return ret;   //need to flesh out the system more so for now just reducing it so its not spammed
    }

    public int CompareTo(PossibleAction p)
    {
        return -1 * fitness.CompareTo(p.fitness);   //descending order because higher fitness values are better (this is bad practice, use linq and sort by descending manually when needed or loop backwards)
    }

    public List<Node> path;
    public UnitAction action;
    public Node target;
    public int fitness;
}