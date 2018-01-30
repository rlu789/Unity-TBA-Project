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

    public void SetSelectedNode(Node node)
    {
        if (selectedNode != null)
            Deselect();
        Select(node);

    }

    public void SelectNode(Node node)
    {
        if (TurnHandler.Instance.currentState == TurnHandlerStates.ENEMYDRAW || TurnHandler.Instance.currentState == TurnHandlerStates.ENEMYTURN) return;

        if (TurnHandler.Instance.currentState == TurnHandlerStates.PLAYERSELECT)
        {
            SelectPlayerSelect(node);
        }
        if (TurnHandler.Instance.currentState == TurnHandlerStates.PLAYERTURN)
        {
            SelectPlayerTurn(node);
        }
    }

    void SelectPlayerTurn(Node node)
    {
        //assume theres a selectednode

        if (selectedNode == node) return;
        if (selectedNode != node)
        {
            if (node.currentUnit.unitStateMachine.state == States.B_SELECTING) return; // player still picking cards
            if (node.currentUnit.unitStateMachine.state == States.B_SELECTINGMOVE) //player has selected a movement card
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
                return;
            }
            if (node.currentUnit.unitStateMachine.state == States.B_SELECTINGACTION) //player has selected an action card
            {
                if (nodesInRange.Contains(node))
                {
                    selectedNode.currentUnit.targetActionNode = node;
                    selectedNode.currentUnit.PerformAction();
                    // Add action to a queue of actions, clear nodes in range and arrow
                    //TurnHandler.Instance.actionQueue.Add(selectedNode.currentUnit);
                    Destroy(movementUIObjectTargetGO);
                    foreach (Node n in nodesInRange)
                    {
                        n.myRenderer.material = n.material;
                    }
                    nodesInRange.Clear();
                }
                else
                {
                    Debug.Log("out of range");
                    selectedNode.currentUnit.GetComponent<Unit>().targetActionNode = null;
                }
            }
        }
    }

    void SelectPlayerSelect(Node node)
    {
        if (selectedNode == null)   //selecting a node with no other nodes selected
        {
            if (node.currentUnit != null && node.currentUnit.unitStateMachine.state == States.WAIT)
            {
                Debug.Log("this unit already has cards selected");
                return; // cant select unit when its in preform state
            }
            if (node.currentUnit != null && node.currentUnit.isEnemy)   //cant select an enemy node without a reason
            {
                return;
            }
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
            if (selectedNode.currentUnitGO == null)
            {
                if (node.currentUnit != null && node.currentUnit.isEnemy)    //cant switch to an enemy node
                {
                    return;
                }
                Deselect();
                Select(node);
                return;
            }
        }
    }

    void Select(Node node)
    {
        //if (node.currentUnit != null && node.currentUnit.unitStateMachine.state == States.END) return; // Cannot select unit if its turn is over

        node.myRenderer.material = node.selectedMaterial;
        selectedNode = node;

        if (node.currentUnit != null)
        {
            UIHelper.Instance.SetStatistics(node.currentUnit);
            UIHelper.Instance.SetUnitActions(node.currentUnit);
            if (selectedNode.currentUnit.unitStateMachine.state == States.B_SELECTINGACTION)
            {
                if (selectedNode.currentUnitGO != null)
                {
                    ShowUnitActionRange(node);
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

        if (unitsWithAssignedPaths.Contains(unit))
            unitsWithAssignedPaths.Remove(unit);
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

    public Path<Node> CheckPath(Node init, Node dest, Unit unit)
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

    public void NodeHoverEnter(Node node)
    {
        if (selectedNode != node && !nodesInRange.Contains(node)) node.myRenderer.material = node.hoverMaterial;    //if node isnt selected and we arent range checking -> show hover material

        if (node.currentUnit != null)
        {
            UIHelper.Instance.SetStatistics(node.currentUnit);
            if (selectedNode != null && selectedNode.currentUnit != null && TurnHandler.Instance.currentState != TurnHandlerStates.PLAYERSELECT) UIHelper.Instance.SetUnitActions(node.currentUnit);
            //basically if we have a selected unit that is trying to use an ability, dont switch the action window
        }

        if (selectedNode != null)
        {
            if (TurnHandler.Instance.currentState == TurnHandlerStates.PLAYERTURN && selectedNode.currentUnit.unitStateMachine.state == States.B_SELECTINGMOVE) ShowPath(selectedNode, node);    //show path if we are in the move turn

            if (selectedNode.currentUnit != null)
            {
                if (selectedNode.currentUnit.unitStateMachine.state == States.B_SELECTINGACTION)  //if we are in ACT state, move the targetting object to node
                {
                    movementUIObjectTargetGO.transform.position = node.transform.position;
                    selectedNode.currentUnitGO.transform.LookAt(new Vector3(node.transform.position.x, selectedNode.currentUnitGO.transform.position.y, node.transform.position.z));
                }
            }
        }
    }

    public void NodeHoverExit(Node node)
    {
        if (selectedNode != node && !nodesInRange.Contains(node)) node.myRenderer.material = node.material;

        if (selectedNode != null)
        {
            UIHelper.Instance.SetStatistics(selectedNode);  //set the windows back to the selected unit
            if (selectedNode.currentUnit != UIHelper.Instance.GetCurrentActingUnit()) UIHelper.Instance.SetUnitActions(selectedNode);   //if the unit is the same as the acting one, dont reset its action window (because i made it get new range and so that makes it get new target which puts the target over the untis head and it still works but its not good looking anyway this whole thing needs some refactoring after some serious paint design docs :rage:
        }
        else UIHelper.Instance.ToggleAllVisible(false); //if there is no selected node, turn off the windows
    }

    public void ShowUnitActionRange(Node node)
    {
        if (selectedNode.currentUnit.unitStateMachine.state != States.B_SELECTINGACTION || selectedNode != node) return;   //only show range for selected units while in ACT turn

        if (movementUIObjectTargetGO != null)   //clean any previous AOE
        {
            Destroy(movementUIObjectTargetGO);
            foreach (Node n in nodesInRange)
            {
                n.myRenderer.material = n.material;
            }
        }

        nodesInRange = node.currentUnit.FindRange();
        foreach (Node n in nodesInRange)
        {
            n.myRenderer.material = n.hoverMaterialBad;
        }
        movementUIObjectTargetGO = Instantiate(movementUIObjectTarget, node.transform.position, Quaternion.identity);
    }
}
