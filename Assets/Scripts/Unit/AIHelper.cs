using UnityEngine;
using System.Collections.Generic;

//currently only moves towards enemies and fires a random ability
//max range is fixed, but sometimes enemies in range just wont perform any action, this happens with the healthy guy (melee only) a lot
//need to split action and movement into two for cleaner code
//do second pass for possible actions to make sure we have the best action unit can do from this node
    //maybe do healers last so they can find the ideal position based on its allies new nodes

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
        Debug.Log("Path is from node (" + path[0].nodeID + ") to node (" + path[path.Count - 1].nodeID + ")");
        Debug.Log("Target is node (" + target.nodeID + ")");
        Debug.Log("Action is  (" + action.name + ")");
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
        Debug.Log("Unit " + unit.stats._class + " max range is " + maxActionAndMoveRange);

        foreach (GameObject enemyGO in Map.Instance.unitDudeFriends)    //(friends means enemies for the enemies)
        {
            Unit enemy = enemyGO.GetComponent<Unit>();

            Debug.Log("Enemy position: " + enemy.XY + " | unit position: " + unit.XY + ". Range check with an estimate of " + Pathfindingv2.EstimateXY(unit.currentNode, enemy.currentNode)
                      + ". Range = " + maxActionAndMoveRange);

            if (Pathfindingv2.EstimateXY(unit.currentNode, enemy.currentNode) > maxActionAndMoveRange) continue; //out of range, skip this target

            Debug.Log("pass");

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

        int index = Random.Range(0, possibleActions.Count);

        if (possibleActions.Count == 0) //just move towards a random enemy
        {
            index = Random.Range(0, Map.Instance.unitDudeFriends.Count);
            NodeManager.Instance.AssignPath(unit.currentNode, Map.Instance.unitDudeFriends[index].GetComponent<Unit>().currentNode);
            return;
        }

        unit.SetUnitPath(possibleActions[index].path);
        unit.readyAction = possibleActions[index].action;
        unit.targetActionNode = possibleActions[index].target;
    }

    void AssignActions(Unit unit, Unit target, List<Node> path)
    {
        //int fitness = 0;
        //TODO fitness calculation

        for (int i = 0; i < unit.actions.Length; ++i)
        {
            Debug.Log("Action " + unit.actions[i].name + " range check with an estimate of " + Pathfindingv2.Estimate(path[path.Count - 1], target.currentNode)
                      + ". Range = " + unit.actions[i].range);
            if (unit.actions[i].range == 0)
            {
                possibleActions.Add(new PossibleAction(path, unit.actions[i], path[path.Count - 1], 1));
                continue;
            }
            List<Node> nodesInRange = unit.actions[i].GetNodesInRange(path[path.Count - 1]);
            if (!nodesInRange.Contains(target.currentNode)) continue;

            //if (Pathfindingv2.Estimate(path[path.Count - 1], target.currentNode) > unit.actions[i].range) continue;  //out of range
            Debug.Log("Pass");
            possibleActions.Add(new PossibleAction(path, unit.actions[i], target.currentNode, 1));
        }
    }
}
