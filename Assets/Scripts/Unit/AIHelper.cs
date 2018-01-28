using UnityEngine;
using System.Collections.Generic;

//IT SEEMS that enemies one range out of their target range wont approach or something, eg. enemyHealthyboy is two range away and wont come into range, but will if he is 3 away

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
    }

    public void DetermineFitness()  //need a way to determine if another unit is already acting on this unit, so they dont overkill too much
    {
        if (action == null) {
            fitness = 0;
            return;
        }
        Unit targetUnit = target.currentUnit;
        if (targetUnit == null)
        {
            fitness = 0;  //for now ignore the fact that there could be AOE etc.
            Debug.Log("My target node ahs no unit!");
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
            if (targetUnit.stats.currentHealth < (targetUnit.stats.maxHealth / 2)) fitness -= action.damage;    //if under 50% health, double the fitness gain
        }
        fitness -= action.healthCost;
        fitness -= action.manaCost;
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
        List<Node> possibleNodes = new List<Node>();
        int maxActionRange = 0;
        int maxActionAndMoveRange = 0;
        UnitAction maxAction = new UnitAction();

        foreach (UnitAction act in unit.actions)    //get the max range of all units actions
        {
            if (act.range > maxActionRange)
            {
                maxActionRange = act.range;
                maxAction = act;
            }
        }
        maxActionAndMoveRange = maxActionRange + unit.stats.moveSpeed;   //the full distance the unit could move + its max attack range;

        foreach (GameObject enemyGO in Map.Instance.unitDudeFriends)    //(friends means enemies for the enemies)
        {
            Unit enemy = enemyGO.GetComponent<Unit>();

            if (Pathfindingv2.EstimateXY(unit.currentNode, enemy.currentNode) > maxActionAndMoveRange) continue; //out of range, skip this target

            possibleTargets.Add(enemy); //enemy within max possible range, going to pathfind towards it
        }

        foreach (Unit enemy in possibleTargets)
        {
            List<Node> pathList = new List<Node>();
            Path<Node> path = NodeManager.Instance.CheckPath(unit.currentNode, enemy.currentNode, unit);    //find the closest node to the enemy we can get to
            if (path == null)
            {
                pathList.Add(unit.currentNode);
            }
            else pathList = unit.GetValidPath(path.ToList());

            List<Node> nodesInRange = maxAction.GetNodesInRange(pathList[pathList.Count - 1]);
            if (!nodesInRange.Contains(enemy.currentNode)) continue;    //cant reach enemy with any action from closest node, skip

            AssignActions(unit, enemy, pathList);   //get possible actions for this path
        }

        HehIGuessItsTimeIMadeMyChoice(unit);
    }

    void AssignActions(Unit unit, Unit target, List<Node> path)
    {
        PossibleAction act;

        for (int i = 0; i < unit.actions.Length; ++i)
        {
            if (unit.actions[i].range == 0)
            {
                possibleActions.Add(new PossibleAction(path, unit.actions[i], path[path.Count - 1], 1));
                continue;
            }
            List<Node> nodesInRange = unit.actions[i].GetNodesInRange(path[path.Count - 1]);
            if (!nodesInRange.Contains(target.currentNode)) continue;

            act = new PossibleAction(path, unit.actions[i], target.currentNode, 0);
            act.DetermineFitness();
            possibleActions.Add(act);
        }
    }

    public void ConfirmBestAction(Unit unit)    //Call on enemy action turn
    {
        possibleActions.Clear();

        List<Unit> possibleTargets = new List<Unit>();
        int maxActionRange = 0;
        UnitAction maxAction = new UnitAction();
        List<Node> start = new List<Node>();
        start.Add(unit.currentNode);

        foreach (UnitAction act in unit.actions) 
        {
            if (act.range > maxActionRange)
            {
                maxActionRange = act.range;
                maxAction = act;
            }
        }

        foreach (GameObject enemyGO in Map.Instance.unitDudeFriends)
        {
            Unit enemy = enemyGO.GetComponent<Unit>();

            if (Pathfindingv2.EstimateXY(unit.currentNode, enemy.currentNode) > maxActionRange) continue;

            AssignActions(unit, enemy, start);
        }

        HehIGuessItsTimeIMadeMyChoice(unit, false);
    }

    void HehIGuessItsTimeIMadeMyChoice(Unit unit, bool move = true)
    {
        List<PossibleAction> trueActions = new List<PossibleAction>();

        foreach (PossibleAction act in possibleActions)
        {
            for (int i = 0; i < act.fitness; ++i) trueActions.Add(act);
            act.DebugLogMe();
        }
        int index = Random.Range(0, possibleActions.Count);

        if ((possibleActions.Count == 0 || trueActions.Count == 0) && move) //just move towards a random enemy
        {
            index = Random.Range(0, Map.Instance.unitDudeFriends.Count);
            NodeManager.Instance.AssignPath(unit.currentNode, Map.Instance.unitDudeFriends[index].GetComponent<Unit>().currentNode);
            return;
        }
        else if ((possibleActions.Count == 0 || trueActions.Count == 0) && !move)
        {
            return;
        }

        index = Random.Range(0, trueActions.Count);

        if (move) unit.SetUnitPath(trueActions[index].path);
        unit.readyAction = trueActions[index].action;
        unit.targetActionNode = trueActions[index].target;
    }
}
