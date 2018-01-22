using UnityEngine;
using System.Collections.Generic;

public class NodeManager : MonoBehaviour {

    public static NodeManager Instance;

    public Node selectedNode;

    public Node destNode, initNode;

    public Node potentialUnitNode;

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

            if (selectedNode != null)   //selecting a node with another node selected
            {
                //check if trying to move unit
                if (selectedNode.currentUnitGO != null)
                {
                    if (node.potientalUnit == null && node.currentUnit == null)
                    {
                        //potentialUnitNode.potientalUnit = null; // delete potential unit for previously selectednode
                        AssignPath(selectedNode, node);
                        //potentialUnitNode = node; // assign new potential unit node
                        //potentialUnitNode.potientalUnit = selectedNode.currentUnitGO.GetComponent<Unit>(); // set potential unit variable of the potential unit node
                    }
                }
                Deselect();
                Select(node);
                return;
            }
        }
        else
            Debug.Log("Theres a unit here lol");
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
        Map.Instance.AStarLite(unit, dest);
        initNode = init; destNode = dest;
    }
}
