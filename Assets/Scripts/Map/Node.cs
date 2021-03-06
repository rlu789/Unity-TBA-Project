﻿using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class Node : MonoBehaviour {

    public Material hoverMaterial;
    public Material hoverMaterialBad;
    public Material selectedMaterial;
    public Material selectedMaterialBad;
    public Material readyMaterial;
    public Transform firePoint;

    //Object fields, don't touch
    [Space(10)]
    public GameObject currentUnitGO;
    public Unit currentUnit;

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
    public bool passable = true;
    bool ready = false;

    public List<Node> neighbours  = new List<Node>();

    public void SetupFields(int ID, int gridX, int gridY, int _moveCost)
    {
        nodeID = ID;
        XY = new Vector2Int(gridX, gridY);
        moveCost = _moveCost;
        myRenderer = GetComponent<Renderer>();
        material = myRenderer.material;
        //List<Node> neighbours = new List<Node>();
    }

    public void SpawnUnit(GameObject unitGO, int team, bool playerControlled, int ownerID = 0)
    {
        if (currentUnitGO != null)
        {
            Debug.Log("Node (" + nodeID + ") already has a unit!");
            return;
        }
        currentUnitGO = Instantiate(unitGO, new Vector3(transform.position.x, transform.position.y, transform.position.z), Quaternion.identity);
        if (team == 0)
            Map.Instance.teamZero.Add(currentUnitGO);
        else
            Map.Instance.teamOne.Add(currentUnitGO);
        Unit unitComponent = currentUnitGO.GetComponent<Unit>();
        currentUnit = unitComponent;
        unitComponent.XY = XY;
        unitComponent.currentNodeID = nodeID;
        unitComponent.ownerID = ownerID;
        unitComponent.currentNode = this;
        unitComponent.playerControlled = playerControlled;
    }

    public double DistanceToEnemy()
    {
        double dist = int.MaxValue;
        double est;
        foreach (GameObject unitGO in Map.Instance.teamZero)
        {
            est = Pathfindingv2.Estimate(this, unitGO.GetComponent<Unit>().currentNode);
            if (est < dist) dist = est;
        }
        return dist;
    }

    public void SetHexReady(bool active)
    {
        if (active)
        {
            myRenderer.material = readyMaterial;
            ready = true;
        }
        else
        {
            myRenderer.material = material;
            ready = false;
        }
    }

    public void SetHexDefault()
    {
        if (ready) myRenderer.material = readyMaterial;
        else myRenderer.material = material;
    }

    public void SetHexHighlighted()
    {
        myRenderer.material = hoverMaterial;
    }

    public void SetHexSelected()
    {
        myRenderer.material = selectedMaterial;
    }

    public void SetHexSelectedBad()
    {
        myRenderer.material = selectedMaterialBad;
    }

    private void OnMouseUp()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;
        NodeManager.Instance.SelectNode(this);
    }

    private void OnMouseEnter()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;
        NodeManager.Instance.NodeHoverEnter(this);
    }

    private void OnMouseExit()
    {
        NodeManager.Instance.NodeHoverExit(this);
    }
}
