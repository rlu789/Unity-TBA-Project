using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class NodeManager : MonoBehaviour {

    public static NodeManager Instance;

    public GameObject movementUIObjectLine;
    public GameObject movementUIObjectTarget;
    public GameObject movementUIObjectTargetGO;
    public Material moveGood;
    public Material moveBad;
    [Space(10)]
    [Header("Don't change these v")]
    public Node selectedNode;

    public List<Unit> unitsWithAssignedPaths;

    public List<Node> nodesInRange = new List<Node>();
    bool rangeMode = false;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("NodeManager already exists!");
            Destroy(gameObject);
            return;
        }
        Instance = this;

    }

    public void SelectNode(Node node)
    {
        if (TurnHandler.Instance.currentState == TurnHandlerStates.ENEMYMOVE || TurnHandler.Instance.currentState == TurnHandlerStates.ENEMYACT) return;

        if (node.currentUnit != null && node.currentUnit.isEnemy )
        {
            return;
        }
        if (selectedNode == null)   //selecting a node with no other nodes selected
        {
            if (node.currentUnit != null && node.currentUnit.GetComponent<UnitStateMachine>().state == States.END) return; // Cannot select unit if its turn is over
            Select(node);
            return;
        }

        if (selectedNode == node)   //selecting a node that is already selected
        {
            Deselect(true);
            return;
        }

        if (selectedNode != null)   //selecting a node with another node selected
        {
            if (TurnHandler.Instance.currentState == TurnHandlerStates.PLAYERMOVE)
            {
                //check if trying to move unit
                if (selectedNode.currentUnitGO != null)
                {
                    if (node.potentialUnit != null)
                    {
                        Debug.Log("Node has a potential unit! Unit: " + node.potentialUnit);    //put some message here
                        return;
                    }
                    if (node.currentUnit != null)
                    {
                        Debug.Log("Node has a unit! Unit: " + node.currentUnit);
                        return;
                    }
                    AssignPath(selectedNode, node);
                    Deselect();
                    return;
                }
                Deselect();
                Select(node);
            }
            if (TurnHandler.Instance.currentState == TurnHandlerStates.PLAYERACT)
            {
                if (nodesInRange.Contains(node))
                {
                    selectedNode.currentUnit.GetComponent<UnitStateMachine>().state = States.PERFORM;
                    selectedNode.currentUnit.GetComponent<Unit>().targetActionNode = node;
                }
                else
                {
                    Deselect();
                    selectedNode.currentUnit.GetComponent<UnitStateMachine>().state = States.ACT;
                    selectedNode.currentUnit.GetComponent<Unit>().targetActionNode = null;
                }
            }
        }
    }

    void Select(Node node)
    {
        node.myRenderer.material = node.selectedMaterial;
        selectedNode = node;

        if (node.currentUnit != null)
        {
            UIHelper.Instance.SetStatistics(node.currentUnit);
            UIHelper.Instance.SetUnitActions(node.currentUnit);
            if (TurnHandler.Instance.currentState == TurnHandlerStates.PLAYERACT)
            {
                if (selectedNode.currentUnitGO != null)
                {
                    nodesInRange = node.currentUnit.FindRange();
                    foreach (Node n in nodesInRange)
                    {
                        n.myRenderer.material = n.hoverMaterialBad;
                    }
                    movementUIObjectTargetGO = Instantiate(movementUIObjectTarget, node.transform.position, Quaternion.identity);
                }
            }
        }
    }

    public void Deselect(bool hovering = false)
    {
        Destroy(movementUIObjectTargetGO);
        foreach (Node n in nodesInRange)
        {
            n.myRenderer.material = n.material;
        }
        nodesInRange.Clear();
        UIHelper.Instance.ToggleAllVisible(false);
        if (!hovering) selectedNode.myRenderer.material = selectedNode.material;
        else selectedNode.myRenderer.material = selectedNode.hoverMaterial; //if you are still hovering over this node, return to hovering material
        PathHelper.Instance.DeleteCurrentPath();
        selectedNode = null;
    }

    public void AssignPath(Node init, Node dest)
    {
        Unit unit = init.currentUnit;
        if (unit == null) return;

        Path<Node> path = CheckPath(init, dest, unit);
        if (path == null)
            return;

        unit.SetUnitPath(path.ToList());
        PathHelper.Instance.DeleteCurrentPath();
        //TODO FIX THIS
        if (!unitsWithAssignedPaths.Contains(unit))
            unitsWithAssignedPaths.Add(unit);
    }

    public void ShowPath(Node init, Node dest)
    {
        Unit unit = init.currentUnit;
        if (unit == null) return;
        Path<Node> path = CheckPath(init, dest, unit);
        if (path == null)
            return;
        PathHelper.Instance.DrawCurrentPath(path.ToList(), unit.stats.moveSpeed);
    }

    Path<Node> CheckPath(Node init, Node dest, Unit unit)
    {
        Path<Node> path = null;
        List<Node> BLACKLISTNEVERENTERTHESENODESEVER = new List<Node>();
        do
        {
            if (path != null)
            {
                List<Node> pathList = unit.GetValidPath(path.ToList()); //if the path is not null, we got a path that ended on a bad hex. Get that final hex and add it to the blacklist
                BLACKLISTNEVERENTERTHESENODESEVER.Add(pathList[pathList.Count - 1]);
            }

            path = Pathfindingv2.FindPath(init, dest, BLACKLISTNEVERENTERTHESENODESEVER);
            if (path == null) return null;   //couldn't path there
        }
        while (!unit.IsPathValid(path.ToList()));
        return path;
    }

    public void UnassignUnitPath()
    {
        unitsWithAssignedPaths[unitsWithAssignedPaths.Count - 1].DeleteUnitPath();
        unitsWithAssignedPaths.RemoveAt(unitsWithAssignedPaths.Count - 1);
    }
    
    //REDO
    public void PerformButton()
    {
        selectedNode.currentUnit.PerformAction();
        Destroy(movementUIObjectTargetGO);
        foreach (Node n in nodesInRange)
        {
            n.myRenderer.material = n.material;
        }
        nodesInRange.Clear();
        selectedNode = null;

        foreach (GameObject u in Map.Instance.unitDudeFriends)
        {
            if (u.GetComponent<UnitStateMachine>().state != States.END)
                break;
            if (u.Equals(Map.Instance.unitDudeFriends[Map.Instance.unitDudeFriends.Count - 1]))
            {
                TurnHandler.Instance.NextState();
            }
        }
    }
}
