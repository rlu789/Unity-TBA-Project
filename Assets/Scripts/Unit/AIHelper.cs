using UnityEngine;
using System.Collections.Generic;

//currently only moves towards enemies and fires a random ability
//something is off because sometimes enemies wont move far enough to fire even though they could possibly do so
//enemies also cant stay still and use an action atm
//also enemies hit themselves a lot :thinking:

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

        foreach (UnitAction act in unit.actions)    //get the max range of all units actions
        {
            if (act.range > maxActionRange) maxActionRange = act.range;
        }
        maxActionAndMoveRange = maxActionRange + unit.stats.moveSpeed;   //the full distance the unit could move + its max attack range;

        foreach (GameObject enemyGO in Map.Instance.unitDudeFriends)    //(friends means enemies for the enemies)
        {
            Unit enemy = enemyGO.GetComponent<Unit>();

            if (Pathfindingv2.Estimate(unit.currentNode, enemy.currentNode) > maxActionAndMoveRange) continue; //out of range, skip this target

            possibleTargets.Add(enemy); //enemy within max possible range, going to pathfind towards it
        }

        foreach (Unit enemy in possibleTargets)
        {
            Path<Node> path = NodeManager.Instance.CheckPath(unit.currentNode, enemy.currentNode, unit);    //find the closest node to the enemy we can get to
            if (path == null) continue;                                                              //instead of this, just stay still and try to use action on target
            List<Node> pathList = unit.GetValidPath(path.ToList());
            if (pathList == null) continue;

            if (Pathfindingv2.Estimate(pathList[pathList.Count - 1], enemy.currentNode) > maxActionRange) continue; //cant reach enemy with any action from closest node, skip

            AssignActions(unit, enemy, pathList);   //get possible actions for each target
        }
        /*
        foreach (PossibleAction pa in possibleActions)
        {
            pa.DebugLogMe();
        }
        */
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

    public void AssignActions(Unit unit, Unit target, List<Node> path)
    {
        //int fitness = 0;
        //TODO fitness calculation

        for (int i = 0; i < unit.actions.Length; ++i)
        {
            Debug.Log("Action " + unit.actions[i].name + " range check with an estimate of " + Pathfindingv2.Estimate(path[path.Count - 1], target.currentNode)
                      + ". Range = " + unit.actions[i].range);

            if (Pathfindingv2.Estimate(path[path.Count - 1], target.currentNode) > unit.actions[i].range) continue;  //out of range
            Debug.Log("Pass");
            possibleActions.Add(new PossibleAction(path, unit.actions[i], target.currentNode, 1));
        }
    }
}
