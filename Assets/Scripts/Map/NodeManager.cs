using UnityEngine;
using System.Collections.Generic;

public class NodeManager : MonoBehaviour {

    public static NodeManager Instance;

    public GameObject movementUIObjectLine;
    public GameObject movementUIObjectTarget;
    public Material moveGood;
    public Material moveBad;
    [Space(10)]
    [Header("Don't change these v")]
    public Node selectedNode;

    public List<Unit> unitsWithAssignedPaths;

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
        if (TurnHandler.Instance.currentState == TurnHandlerStates.PLAYERMOVE)
        {
            if (selectedNode == null)   //selecting a node with no other nodes selected
            {
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
                }
                Deselect();
                Select(node);
            }
        }

    }

    void Select(Node node)
    {
        node.myRenderer.material = node.selectedMaterial;
        selectedNode = node;
    }

    public void Deselect(bool hovering = false)
    {
        UIHelper.Instance.ToggleVisible(UIType.Statistics, false);
        if (!hovering) selectedNode.myRenderer.material = selectedNode.material;
        else selectedNode.myRenderer.material = selectedNode.hoverMaterial; //if you are still hovering over this node, return to hovering material

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
}
