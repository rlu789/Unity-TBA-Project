using UnityEngine;
using System.Collections.Generic;

//IF ERROR HERE REMEMBER SELECTED ACTIONS
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
        if (action == null) {
            fitness = 0;
            return;
        }
        Unit targetUnit = target.currentUnit;
        if (targetUnit == null)
        {
            fitness = 0;  //for now ignore the fact that there could be AOE etc.
            return;
        }

        if (!targetUnit.isEnemy)
        {
            fitness += action.damage;
            if (targetUnit.stats.currentHealth - action.damage <= 0) fitness += action.damage;  //if it will kill, double fitness
        }
        else
        {
            fitness -= action.damage;   //negative damage is healing
            if (targetUnit.stats.currentHealth <= (targetUnit.stats.maxHealth / 2)) fitness -= action.damage;    //if under 50% health, double the fitness gain
            if (targetUnit.stats.currentHealth - action.damage > targetUnit.stats.maxHealth) fitness -= (targetUnit.stats.currentHealth - action.damage) - targetUnit.stats.maxHealth;  //only fitness for the actual health regained, not overheal
        }
        fitness -= action.healthCost;
        fitness -= action.manaCost * 2;
        if (fitness < 0) fitness = 0;
}
    public List<Node> path;
    public UnitAction action;
    public Node target;
    public int fitness;
}

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

        for (int i = 0; i < unit.selectedActions.Count; ++i)
        {
            if (unit.selectedActions[i].range == 0)
            {
                act = new PossibleAction(path, unit.selectedActions[i], unit.currentNode, 0);
                act.DetermineFitness();
                possibleActions.Add(act);
                continue;
            }
            List<Node> nodesInRange = unit.selectedActions[i].GetNodesInRange(path[path.Count - 1]);
            if (!nodesInRange.Contains(target.currentNode)) continue;

            act = new PossibleAction(path, unit.selectedActions[i], target.currentNode, 0);
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

        if ((possibleActions.Count == 0 || trueActions.Count == 0) && move) //just move towards a random enemy
        {
            index = Random.Range(0, Map.Instance.unitDudeFriends.Count);
            NodeManager.Instance.AssignPath(unit.currentNode, Map.Instance.unitDudeFriends[index].GetComponent<Unit>().currentNode);
            //TODO: Change this to get ideal range and move there instead
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
        foreach (UnitAction act in unit.selectedActions)    //get the max range of all units actions
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
