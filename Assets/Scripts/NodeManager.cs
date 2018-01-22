using UnityEngine;
using System.Collections.Generic;

public class NodeManager : MonoBehaviour {

    public static NodeManager Instance;

    public Node selectedNode;

    public Node destNode, initNode;

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
                AssignPath(selectedNode, node);
            }
            Deselect();
            Select(node);
            return;
        }
    }

    void Select(Node node)
    {
        node.myRenderer.material = node.selectedMaterial;
        selectedNode = node;
    }

    void Deselect(bool hovering = false)
    {
        if (!hovering) selectedNode.myRenderer.material = selectedNode.material;
        else selectedNode.myRenderer.material = selectedNode.hoverMaterial; //if you are still hovering over this node, return to hovering material

        selectedNode = null;
    }

    void AssignPath(Node init, Node dest)
    {
        Unit unit = init.currentUnit;
        Map.Instance.AStarLite(unit, dest);
        initNode = init; destNode = dest;
    }

    public void MoveUnit()    //moves unit on selected tile
    {
        List<Node> pathToFollow = initNode.currentUnit.GetPath();   //get the path to follow, based on the max distance the unit can move this turn
        if (pathToFollow == null) return;
        if (pathToFollow.Count == 0) return;

        Node _destNode = pathToFollow[pathToFollow.Count - 1];  //the destination is the furthest node we can reach

        //set values on initial and destination nodes
        _destNode.currentUnitGO = initNode.currentUnitGO;
        _destNode.currentUnit = initNode.currentUnit;
        initNode.currentUnitGO = null;
        initNode.currentUnit = null;

        _destNode.currentUnitGO.transform.position = _destNode.transform.position;    //TODO: lerp/animate the the tile instead

        //set units new node values
        Unit unitComponent = _destNode.currentUnitGO.GetComponent<Unit>();
        unitComponent.XY = _destNode.XY;
        unitComponent.currentNodeID = _destNode.nodeID;

        Deselect();
        Select(_destNode);
        initNode = _destNode;
    }
}
