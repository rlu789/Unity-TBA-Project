using UnityEngine;
using System.Collections.Generic;

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

        maxAction = GetMaxRangeAction(unit, ref maxActionRange);
        maxActionAndMoveRange = maxActionRange + unit.stats.moveSpeed;   //the full distance the unit could move + its max attack range;

        possibleTargets = GetPossibleTargets(unit, maxActionAndMoveRange);  //get the targets 

        if (maxAction == null) Debug.Log("No actions available!");
        else foreach (Unit target in possibleTargets)
        {
            List<Node> pathList = new List<Node>();
            Path<Node> path = NodeManager.Instance.CheckPath(unit.currentNode, target.currentNode, unit);    //find the closest node to the target we can get to
            if (path == null)
            {
                pathList.Add(unit.currentNode);
            }
            else pathList = unit.GetValidPath(path.ToList());

            List<Node> nodesInRange = maxAction.GetNodesInRange(pathList[pathList.Count - 1]);  //is this enemy in range from the closest we can get
            if (nodesInRange.Contains(target.currentNode)) AssignActions(unit, target, pathList); //get possible actions for this path
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
            /*
            if (unit.cards.selectedActions[i].range == 0)
            {
                act = new PossibleAction(path, unit.cards.selectedActions[i], unit.currentNode, 0);
                act.DetermineFitness();
                possibleActions.Add(act);
                continue;
            }
            */
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

        possibleActions.Shuffle(new System.Random()); //shuffle list so we don't bias certain nodes when choosing equal fitness targets
        int trimCount = GetMinActionCount(unit);
        possibleActions = TrimActionList(trimCount, unit);

        foreach (PossibleAction act in possibleActions)
        {
            for (int i = 0; i < act.fitness; ++i) trueActions.Add(act); //add based on fitness 
        }

        //TODO:
        //after getting the best action, check if action range is greater than my range to enemy, if so get all the nodes at one distance away and compare to the nodes i can reach from starting point,
        //if i can move there, set the path there and loop the above line again

        foreach (PossibleAction act in trueActions) act.DebugLogMe();

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

    List<PossibleAction> TrimActionList(int goal, Unit unit)
    {
        List<PossibleAction> possibleActionsTrimmed = new List<PossibleAction>();
        List<PossibleAction> currentActionList = new List<PossibleAction>();

        foreach (UnitAction act in unit.cards.selectedActions)
        {
            currentActionList = new List<PossibleAction>();

            foreach (PossibleAction pAct in possibleActions)
            {
                if (pAct.action == act) currentActionList.Add(pAct);
            }
            if (currentActionList.Count == 0) continue;

            currentActionList.Sort();   //sort by descending fitness (still random in the case of equal values since we randomised possible actions earlier)

            for (int i = 0; i < goal; ++i)
            {
                possibleActionsTrimmed.Add(currentActionList[i]);
            }
        }

        return possibleActionsTrimmed;
    }

    int GetMinActionCount(Unit unit)    //by balancing the action possibilities to the lowest count, we only value the fitness not how many possible targets we have available
    {
        int min = int.MaxValue;
        int count;
        foreach (UnitAction act in unit.cards.selectedActions)
        {
            count = GetActionCount(act);
            if (count < min) min = count;
            count = 0;
        }
        return min;
    }

    int GetActionCount(UnitAction action)   //how many times have we considered this action this round?
    {
        int count = 0;
        foreach (PossibleAction pAct in possibleActions)
        {
            if (action == pAct.action) count++;
        }
        return count;
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
