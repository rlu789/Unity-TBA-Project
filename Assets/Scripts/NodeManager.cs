using UnityEngine;

public class NodeManager : MonoBehaviour {

    public static NodeManager Instance;

    public Node selectedNode;
    

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
        Debug.Log("Selected node at (" + node.nodeXY.x + "," + node.nodeXY.y + "), ID: " + node.nodeID);

        node.myRenderer.material = node.selectedMaterial;
        selectedNode = node;
    }

    void Deselect(bool hovering = false)
    {
        Debug.Log("Deselected node at (" + selectedNode.nodeXY.x + "," + selectedNode.nodeXY.y + "), ID: " + selectedNode.nodeID);

        if (!hovering) selectedNode.myRenderer.material = selectedNode.material;
        else selectedNode.myRenderer.material = selectedNode.hoverMaterial; //if you are still hovering over this node, return to hovering material

        selectedNode = null;
    }

    void AssignPath(Node initNode, Node destNode)
    {
        Unit unit = initNode.currentUnit;
        Map.Instance.AStarLite(unit, destNode);

        ////set values on initial and destination nodes
        //destNode.currentUnitGO = initNode.currentUnitGO;
        //initNode.currentUnitGO = null;

        //destNode.currentUnitGO.transform.position = destNode.transform.position;    //TODO: lerp/animate the the tile instead
        ////set units new node values
        //Unit unitComponent = destNode.currentUnitGO.GetComponent<Unit>();
        //unitComponent.XY = destNode.nodeXY;
        //unitComponent.currentNodeID = destNode.nodeID;
    }
}
