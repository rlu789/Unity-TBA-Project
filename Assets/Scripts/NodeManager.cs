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

    public Node destNode, initNode;
    public Node potentialUnitNode;

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
        //if (potentialUnitNode == null)
        //    potentialUnitNode = node;
        if (TurnHandler.Instance.currentState == TurnHandlerStates.PLAYERTURN)
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

            if (selectedNode != null && node.potientalUnit == null && node.currentUnit == null)   //selecting a node with another node selected
            {
                //check if trying to move unit
                if (selectedNode.currentUnitGO != null)
                {
                    //potentialUnitNode.potientalUnit = null; // delete potential unit for previously selectednode
                    AssignPath(selectedNode, node);
                    //potentialUnitNode = node; // assign new potential unit node
                    //potentialUnitNode.potientalUnit = selectedNode.currentUnitGO.GetComponent<Unit>(); // set potential unit variable of the potential unit node
                }
                Deselect();
                //Select(node);
                return;
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
        if (!hovering) selectedNode.myRenderer.material = selectedNode.material;
        else selectedNode.myRenderer.material = selectedNode.hoverMaterial; //if you are still hovering over this node, return to hovering material

        selectedNode = null;
    }

    public void AssignPath(Node init, Node dest)
    {
        Unit unit = init.currentUnit;
        if (unit == null) return;

        Path<Node> path = Pathfindingv2.FindPath(init, dest);
        if (path == null) return;
        
        unit.SetUnitPath(path.ToList());
        PathHelper.Instance.DeleteCurrentPath();
        initNode = init; destNode = dest;
        unitsWithAssignedPaths.Add(unit);
    }

    public void ShowPath(Node init, Node dest)
    {

        Unit unit = init.currentUnit;
        if (unit == null) return;

        Path<Node> path = Pathfindingv2.FindPath(init, dest);
        if (path == null) return;

        PathHelper.Instance.DrawCurrentPath(path.ToList(), unit.moveSpeed);
    }
}
