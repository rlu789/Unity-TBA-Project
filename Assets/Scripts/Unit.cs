using UnityEngine;
using System.Collections.Generic;

public class Unit : MonoBehaviour {
    public Vector2Int XY = new Vector2Int(0, 0);
    public int currentNodeID = -1;
    public List<Node> currentPath;

    void Update()
    {
        if (currentPath != null)
        {
            int currNode = 0;
            while (currNode < currentPath.Count - 1)
            {
                Vector3 start = new Vector3(currentPath[currNode].transform.position.x
                    , 0, currentPath[currNode].transform.position.z) +
                    new Vector3(0, 1f, 0);
                Vector3 end = new Vector3 (currentPath[currNode + 1].transform.position.x, 0, 
                    currentPath[currNode].transform.position.z) +
                    new Vector3(0, 1f, 0);

                Debug.DrawLine(start, end, Color.blue);

                currNode++;
            }
        }
    }
}
