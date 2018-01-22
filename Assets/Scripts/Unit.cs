using UnityEngine;
using System.Collections.Generic;

public class Unit : MonoBehaviour {

    [Header("Stats")]
    public int maxHealth = 100;
    int currentHealth;
    public int moveSpeed = 2;

    //Setup fields
    [Header("For unity and debug, don't change")]
    public Vector2Int XY = new Vector2Int(0, 0);
    public int currentNodeID = -1;
    public List<Node> currentPath = new List<Node>();
    public GameObject __testObject;
    //fresh fIelds
    List<Node> movePath = new List<Node>();
    int currMoveIndex = 0;

    void Update()
    {
        if (movePath.Count != 0)
        {
            MoveStep();
        }
        DrawPath();
    }

    void DrawPath()
    {
        if (currentPath != null)
        {
            int currNode = 0;
            while (currNode < currentPath.Count - 1)
            {
                Vector3 start = new Vector3(currentPath[currNode].transform.position.x, 1f, currentPath[currNode].transform.position.z);
                Vector3 end = new Vector3(currentPath[currNode + 1].transform.position.x, 1f, currentPath[currNode + 1].transform.position.z);

                Debug.DrawLine(start, end, Color.red);
                currNode++;
            }
        }
    }

    public List<Node> GetPath()
    {
        int movementRemaining = moveSpeed;
        int currentNode = 0;
        List<Node> pathToFollow = new List<Node>();

        while (currentNode < currentPath.Count - 1 && movementRemaining > 0)
        {
            //currentPath[currentNode].moveCost;   //add the cost from the nodeType to the node
            movementRemaining -= currentPath[currentNode+1].moveCost;    //make this minus the move cost

            if (movementRemaining < 0) break;   //if you can't make it to the node, stop adding to the path

            currentNode++;
            pathToFollow.Add(currentPath[currentNode]);
        }

        currentPath.RemoveRange(0, currentPath.Count - 1);  //empty the path once we've moved all we can this turn

        movePath = pathToFollow;
        currMoveIndex = 0;
        return pathToFollow;
    }

    void MoveStep()
    {
        Debug.Log("oi");
        Debug.Log(movePath[0]);
        Vector3 dir = movePath[currMoveIndex].transform.position - transform.position;
        transform.Translate(dir.normalized * Time.deltaTime*moveSpeed*2, Space.World);
        if ( Vector3.Distance(transform.position, movePath[currMoveIndex].transform.position) <= 0.2f)
        {
            GetNextStep();
        }
    }

    void GetNextStep()
    {
        if (currMoveIndex == movePath.Count )
        {
            movePath.RemoveRange(0, movePath.Count - 1);
            return;
        }
        currMoveIndex++;
    }
}
