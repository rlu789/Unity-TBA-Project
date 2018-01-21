using UnityEngine;
using System.Collections.Generic;

public class Node : MonoBehaviour {

    public Material hoverMaterial;
    public Material selectedMaterial;

    //Object fields, don't touch
    [Space(10)]
    public GameObject currentUnitGO;
    public Unit currentUnit;
    [HideInInspector]
    public int nodeID;
    [HideInInspector]
    public Vector2Int nodeXY;
    [HideInInspector]
    public Renderer myRenderer;
    [HideInInspector]
    public Material material;

    public List<Node> neighbours  = new List<Node>();


    public void SetupFields(int ID, int gridX, int gridY)
    {
        nodeID = ID;
        nodeXY = new Vector2Int(gridX, gridY);

        myRenderer = GetComponent<Renderer>();
        material = myRenderer.material;
        List<Node> neighbours = new List<Node>();
    }

    public void SpawnUnit(GameObject unitGO)
    {
        if (currentUnitGO != null)
        {
            Debug.Log("Node (" + nodeID + ") already has a unit!");
            return;
        }
        currentUnitGO = Instantiate(unitGO, new Vector3(transform.position.x, transform.position.y, transform.position.z), Quaternion.identity);
        Unit unitComponent = currentUnitGO.GetComponent<Unit>();
        currentUnit = unitComponent;
        unitComponent.XY = nodeXY;
        unitComponent.currentNodeID = nodeID;
    }

    private void OnMouseUp()
    {
        NodeManager.Instance.SelectNode(this);
    }

    private void OnMouseEnter()
    {
        if (NodeManager.Instance.selectedNode != this) myRenderer.material = hoverMaterial;
    }

    private void OnMouseExit()
    {
        if (NodeManager.Instance.selectedNode != this) myRenderer.material = material;
    }
}
