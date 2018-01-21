using UnityEngine;
using System.Collections.Generic;

public class Unit : MonoBehaviour {
    public Vector2Int XY = new Vector2Int(0, 0);
    public int currentNodeID = -1;
    public List<Node> currentPath;
    public GameObject __testObject;

    void Update()
    {
        CheckMovement();    //yo richard the path or the neighbours aren't correct you can see with the balls i believe
    }

    void CheckMovement()
    {
        if (currentPath != null)
        {
            int currNode = 0;
            while (currNode < currentPath.Count - 1)
            {
                Vector3 start = new Vector3(currentPath[currNode].transform.position.x
                    , 0, currentPath[currNode].transform.position.z) +
                    new Vector3(0, 1f, 0);
                Vector3 end = new Vector3(currentPath[currNode + 1].transform.position.x, 0,
                    currentPath[currNode].transform.position.z) +
                    new Vector3(0, 1f, 0);

                Debug.DrawLine(start, end, Color.blue);
                Destroy(Instantiate(__testObject, new Vector3(currentPath[currNode].transform.position.x, 0, currentPath[currNode].transform.position.z), Quaternion.identity), 0.5f);

                currNode++;
            }
        }
    }
}
