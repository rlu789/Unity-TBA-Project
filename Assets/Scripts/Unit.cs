﻿using UnityEngine;
using System.Collections.Generic;

public class Unit : MonoBehaviour {

    [Header("Stats")]
    public int maxHealth = 100;
    int currentHealth;
    public int moveSpeed = 2;
    int currentMovement;
    public bool isEnemy;

    //Setup fields
    [Header("For unity and debug, don't change")]
    public Vector2Int XY = new Vector2Int(0, 0);
    public int currentNodeID = -1;
    public List<Node> currentPath = new List<Node>();
    public GameObject __testObject;
    //fresh fIelds
    List<Node> movePath = new List<Node>();
    List<GameObject> pathVisual = new List<GameObject>();
    int currMoveIndex = 0;
    public Node currentNode;

    bool pathHasChanged = false;

    GameObject[,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,][,,,,,,,,,,,,,,,,,,,,,,,,,,,][,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,] loadBearingArray;

    private void Start()
    {
        currentHealth = maxHealth;
        currentMovement = moveSpeed;
    }

    void Update()
    {
        if (movePath.Count != 0)
        {
            MoveStep();
        }
    }
    

    void MoveStep()
    {
        Vector3 dir = movePath[currMoveIndex].transform.position - transform.position;      //sets our target direction to the next node along the path
        transform.Translate(dir.normalized * Time.deltaTime * (moveSpeed), Space.World);    //moves towards our target
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(dir), (moveSpeed * 2) * Time.deltaTime); //rotates in the direction we are going

        if ( Vector3.Distance(transform.position, movePath[currMoveIndex].transform.position) <= 0.1f)  //if we are close to the node, we can start moving towards the next node
        {
            GetNextStep();
        }
    }

    void GetNextStep()
    {
        currMoveIndex++;
        if (currMoveIndex == movePath.Count)
        {
            transform.position = movePath[currMoveIndex - 1].transform.position;    //make sure we are right on the node when we are finished
            movePath.RemoveRange(0, movePath.Count);                                //clear out the list of nodes to move to
            currMoveIndex = 0;                                                      //reset our move index when finished
            return;
        }
    }

    public void MoveUnit()    //moves unit on selected tile
    {
        if ((TurnHandler.Instance.currentState == TurnHandlerStates.PLAYERTURN && !isEnemy) || (TurnHandler.Instance.currentState == TurnHandlerStates.ENEMYTURN && isEnemy))
        {
            //Debug.Log(currentPath[0]);

            List<Node> pathToFollow = currentPath;   //get the path to follow, based on the max distance the unit can move this turn
            for (int i = 0; i < currentPath.Count; ++i)
            {
                if (currentPath[i] != null)
                {
                    Debug.Log(currentPath[i] + " at index: " + i);
                }
                else
                    Debug.Log("Nothing here");
            }
            movePath = currentPath;
            //currentPath.Clear();
            foreach (GameObject haha in pathVisual)
                Destroy(haha);
            pathVisual.Clear();
            if (pathToFollow == null)
                return;
            if (pathToFollow.Count == 0)
                return;

            Node _destNode = pathToFollow[pathToFollow.Count - 1];  //the destination is the furthest node we can reach

            //set values on initial and destination nodes
            _destNode.currentUnitGO = gameObject;
            _destNode.currentUnit = this;
            _destNode.potientalUnit = null;
            Map.Instance.nodes[XY.x, XY.y].currentUnitGO = null;
            Map.Instance.nodes[XY.x, XY.y].currentUnit = null;

            //set units new node values
            Unit unitComponent = _destNode.currentUnitGO.GetComponent<Unit>();
            unitComponent.XY = _destNode.XY;
            unitComponent.currentNodeID = _destNode.nodeID;
            currentNode = _destNode;
            
            //NodeManager.Instance.Deselect();
            //Select(_destNode);
            //initNode = _destNode;
        }
    }

    public void SetUnitPath(List<Node> path)
    {
        int movementRemaining = moveSpeed;
        int currentNodeInt = 0;
        currentPath.Add(currentNode);
        while (currentNodeInt < path.Count - 1 && movementRemaining > 0)
        {
            Debug.Log(currentNodeInt);
            movementRemaining -= path[currentNodeInt + 1].moveCost;    //reduce our remaning movement by the cost

            if (movementRemaining < 0) break;   //if you can't make it to the node, stop adding to the path

            currentNodeInt++;
            currentPath.Add(path[currentNodeInt]);
        }
        pathVisual = PathHelper.Instance.DrawActualPath(currentPath);
    }

    //public void TogglePathVisual(bool toggle)
    //{
    //    if (pathVisual.Count == 0) return;

    //    foreach (GameObject GO in pathVisual)
    //    {
    //        GO.SetActive(toggle);
    //    }
    //}
}