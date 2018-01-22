using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class Node : MonoBehaviour {

    public Material hoverMaterial;
    public Material hoverMaterialBad;
    public Material selectedMaterial;
    public Material selectedMaterialBad;

    //Object fields, don't touch
    [Space(10)]
    public GameObject currentUnitGO;
    public Unit currentUnit;
    public Unit potientalUnit; // If a unit of going to move here at end of turn
    [HideInInspector]
    public int nodeID;
    [HideInInspector]
    public Vector2Int XY;
    [HideInInspector]
    public Renderer myRenderer;
    [HideInInspector]
    public Material material;
    [HideInInspector]
    public int moveCost;

    public List<Node> neighbours  = new List<Node>();


    public void SetupFields(int ID, int gridX, int gridY, int _moveCost)
    {
        nodeID = ID;
        XY = new Vector2Int(gridX, gridY);
        moveCost = _moveCost;
        myRenderer = GetComponent<Renderer>();
        material = myRenderer.material;
        List<Node> neighbours = new List<Node>();
    }

    public void SpawnUnit(GameObject unitGO, bool isEnemy)
    {
        if (currentUnitGO != null)
        {
            Debug.Log("Node (" + nodeID + ") already has a unit!");
            return;
        }
        currentUnitGO = Instantiate(unitGO, new Vector3(transform.position.x, transform.position.y, transform.position.z), Quaternion.identity);
        if (isEnemy)
            Map.Instance.unitDudeEnemies.Add(currentUnitGO);
        else
            Map.Instance.unitDudeFriends.Add(currentUnitGO);
        Unit unitComponent = currentUnitGO.GetComponent<Unit>();
        currentUnit = unitComponent;
        unitComponent.XY = XY;
        unitComponent.currentNodeID = nodeID;
        unitComponent.isEnemy = isEnemy;
        unitComponent.currentNode = this;
    }

    private void OnMouseUp()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;
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
