using UnityEngine;
using System.Collections.Generic;

public class PossibleAction
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
        List<Node> nodes = action.GetNodesInRange(target, true);
        foreach (Node n in nodes)
        {
            fitness += DetermineNodeFitness(n);
        }
        
        if (fitness < 0) fitness = 0;
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

        int e = (enemy) ? -1 : 1;   //treat normal fitness gain on an ally as opposite
        int tempFitness;
        int ret = 0;

        foreach (Effect eff in action.status.effects)
        {
            tempFitness = 0;
            if (eff.type == StatusType.DOT) tempFitness += eff.strength;
            if (eff.type == StatusType.IncomingDamage) tempFitness += eff.strength * 2;
            if (eff.type == StatusType.OutgoingDamage || eff.type == StatusType.MoveSpeed) tempFitness -= eff.strength * 2;
            if (eff.type == StatusType.Actions) tempFitness -= eff.strength * 4;

            if (!eff.initialEffect) tempFitness *= action.status.duration;  //multiply the fitness by how long it lasts

            tempFitness *= e;   //what is good against an enemy is bad to a friend

            tempFitness /= 3;   //need to flesh out the system more so for now just reducing it so its not spammed
            ret += tempFitness;
        }
        return ret;
    }

    public List<Node> path;
    public UnitAction action;
    public Node target;
    public int fitness;
}
//TODO: make this work for ally AI
//      make AI consider running away
public class AIHelper : MonoBehaviour {

    public static AIHelper Instance;

    List<PossibleAction> possibleActions = new List<PossibleAction>();

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("AIHelper already exists!");
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void AIGetTurn(Unit unit)    //finds a target to move towards and attack (need to add supporting allies)
    {
        possibleActions.Clear();    //clear previous units actions

        List<Unit> possibleTargets = new List<Unit>();
        int maxActionRange = 0;
        int maxActionAndMoveRange = 0;
        UnitAction maxAction = new UnitAction();
        List<Node> start = new List<Node>();    //Sometimes remaining where you are is the best movement
        start.Add(unit.currentNode);

        maxAction = GetMaxRangeAction(unit, ref maxActionRange);
        maxActionAndMoveRange = maxActionRange + unit.stats.moveSpeed;   //the full distance the unit could move + its max attack range;

        possibleTargets = GetPossibleTargets(unit, maxActionAndMoveRange);  //get the targets 

        foreach (Unit target in possibleTargets)
        {
            List<Node> pathList = new List<Node>();
            Path<Node> path = NodeManager.Instance.CheckPath(unit.currentNode, target.currentNode, unit);    //find the closest node to the target we can get to
            if (path == null)
            {
                pathList.Add(unit.currentNode);
            }
            else pathList = unit.GetValidPath(path.ToList());
            if (maxAction == null)
            {
                Debug.Log("No actions available!");
                break;
            }
            List<Node> nodesInRange = maxAction.GetNodesInRange(pathList[pathList.Count - 1]);  //is this enemy in range from the closest we can get
            if (nodesInRange.Contains(target.currentNode)) AssignActions(unit, target, pathList); //get possible actions for this path

            nodesInRange = maxAction.GetNodesInRange(unit.currentNode);                     //is this enemy in range from the starting position
            if (nodesInRange.Contains(target.currentNode)) AssignActions(unit, target, start); //get possible actions for starting node
        }

        HehIGuessItsTimeIMadeMyChoice(unit);
    }

    List<Unit> GetPossibleTargets(Unit unit, int range)   //TODO: bool for target allies aswell (unit has healing?)
    {
        List<Unit> possibleTargets = new List<Unit>();
        foreach (GameObject enemyGO in Map.Instance.unitDudeFriends)    //(friends means enemies for the enemies)
        {
            Unit enemy = enemyGO.GetComponent<Unit>();

            if (Pathfindingv2.EstimateXY(unit.currentNode, enemy.currentNode) > range) continue; //out of range, skip this target

            possibleTargets.Add(enemy); //enemy within max possible range, going to pathfind towards it
        }

        foreach (GameObject allyGO in Map.Instance.unitDudeEnemies) //get allies (for healing)
        {
            Unit ally = allyGO.GetComponent<Unit>();

            if (Pathfindingv2.EstimateXY(unit.currentNode, ally.currentNode) > range) continue;

            possibleTargets.Add(ally);
        }
        return possibleTargets;
    }

    void AssignActions(Unit unit, Unit target, List<Node> path)
    {
        PossibleAction act;

        for (int i = 0; i < unit.cards.selectedActions.Count; ++i)
        {
            if (unit.cards.selectedActions[i].range == 0)
            {
                act = new PossibleAction(path, unit.cards.selectedActions[i], unit.currentNode, 0);
                act.DetermineFitness();
                possibleActions.Add(act);
                continue;
            }
            List<Node> nodesInRange = unit.cards.selectedActions[i].GetNodesInRange(path[path.Count - 1]);
            if (!nodesInRange.Contains(target.currentNode)) continue;

            act = new PossibleAction(path, unit.cards.selectedActions[i], target.currentNode, 0);
            act.DetermineFitness();
            possibleActions.Add(act);
        }
    }

    public void ConfirmBestAction(Unit unit)    //Call on enemy action turn
    {                                           //currently only checking enemies, TODO: check allies (healing)
        possibleActions.Clear();

        int maxActionRange = 0;
        List<Unit> possibleTargets = new List<Unit>();
        List<Node> start = new List<Node>();
        start.Add(unit.currentNode);

        GetMaxRangeAction(unit, ref maxActionRange);

        possibleTargets = GetPossibleTargets(unit, maxActionRange);

        foreach (Unit target in possibleTargets)
        {
            AssignActions(unit, target, start);
        }

        HehIGuessItsTimeIMadeMyChoice(unit, false);
    }

    void HehIGuessItsTimeIMadeMyChoice(Unit unit, bool move = true)
    {
        List<PossibleAction> trueActions = new List<PossibleAction>();

        foreach (PossibleAction act in possibleActions)
        {
            for (int i = 0; i < act.fitness; ++i) trueActions.Add(act);
            //act.DebugLogMe();
        }

        int index = 0;

        if ((possibleActions.Count == 0 || trueActions.Count == 0) && move) //just move towards a random enemy (or ally)
        {
            if (!unit.stats.hugFriends && Map.Instance.unitDudeFriends.Count != 0)
            {
                index = Random.Range(0, Map.Instance.unitDudeFriends.Count);
                NodeManager.Instance.AssignPath(unit.currentNode, Map.Instance.unitDudeFriends[index].GetComponent<Unit>().currentNode);
            }
            else if (Map.Instance.unitDudeEnemies.Count != 0)
            {
                index = Random.Range(0, Map.Instance.unitDudeEnemies.Count);
                NodeManager.Instance.AssignPath(unit.currentNode, Map.Instance.unitDudeEnemies[index].GetComponent<Unit>().currentNode);
            }
            return;
        }
        else if ((possibleActions.Count == 0 || trueActions.Count == 0) && !move)
        {
            return;
        }

        index = Random.Range(0, trueActions.Count);

        if (move) unit.SetUnitPath(trueActions[index].path);
        unit.SetAction(trueActions[index].action, trueActions[index].target);
    }

    UnitAction GetMaxRangeAction(Unit unit, ref int maxActionRange) //returns highest range action of unit, sets maxActionRange to that actions range
    {
        UnitAction ret = null;
        foreach (UnitAction act in unit.cards.selectedActions)    //get the max range of all units actions
        {
            if (act.range > maxActionRange)
            {
                maxActionRange = act.range;
                ret = act;
            }
        }
        return ret;
    }
}
