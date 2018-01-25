using UnityEngine;
using System.Collections.Generic;

public class PathHelper : MonoBehaviour
{
    public static PathHelper Instance;
    public List<Node> currentPath;
    List<GameObject> pathVisual = new List<GameObject>();

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("PathHelper already exists!");
            Destroy(gameObject);
            return;
        }
        Instance = this;

    }


    void DrawLines(int movement, int current, GameObject GO, List<Node> path = null, List<GameObject> pathV = null)
    {
        #region WTF (theres nothing wrong with this code honestly it might be the best-written code in the whole project so back off)
        if (path == null)
        {
            path = currentPath;
        }
        if (pathV == null)
        {
            pathV = pathVisual;
        }
        #endregion
        if (movement >= 0)
        {
            pathV.Add(Instantiate(GO, path[current].transform.position, Quaternion.identity));
        }
        else
        {
            pathV.Add(Instantiate(GO, path[current].transform.position, Quaternion.identity));

            Renderer[] rends = pathV[current].GetComponentsInChildren<Renderer>();
            foreach (Renderer rendo in rends)
            {
                rendo.material = NodeManager.Instance.moveBad;
            }
        }
    }

    public void DrawCurrentPath(List<Node> _currentPath, int moveSpeed)
    {
        currentPath = _currentPath;
        if (currentPath != null)
        {
            foreach (GameObject objectIns in pathVisual)
            {
                if (objectIns != null) Destroy(objectIns);
            }

            pathVisual = new List<GameObject>();

            int currNode = 0;
            while (currNode < currentPath.Count - 1)
            {
                //Vector3 start = new Vector3(currentPath[currNode].transform.position.x, 1f, currentPath[currNode].transform.position.z);
                //Vector3 end = new Vector3(currentPath[currNode + 1].transform.position.x, 1f, currentPath[currNode + 1].transform.position.z);

                moveSpeed -= currentPath[currNode + 1].moveCost;
                currNode++;

                DrawLines(moveSpeed, currNode - 1, NodeManager.Instance.movementUIObjectLine);
                //set direction for line
                Vector3 dir = currentPath[currNode - 1].transform.position - currentPath[currNode].transform.position;
                pathVisual[currNode - 1].transform.rotation = Quaternion.LookRotation(dir);
                //final node
                if (currNode == currentPath.Count - 1)
                {
                    DrawLines(moveSpeed, currNode, NodeManager.Instance.movementUIObjectTarget);
                }
            }
        }
    }

    public List<GameObject> DrawActualPath(List<Node> _currentPath)
    {
        List<GameObject> _pathVisual = new List<GameObject>();
        if (_currentPath != null)
        {
            int currNode = 0;
            while (currNode < _currentPath.Count - 1)
            {
                //Vector3 start = new Vector3(currentPath[currNode].transform.position.x, 1f, currentPath[currNode].transform.position.z);
                //Vector3 end = new Vector3(currentPath[currNode + 1].transform.position.x, 1f, currentPath[currNode + 1].transform.position.z);
                currNode++;

                DrawLines(420, currNode - 1, NodeManager.Instance.movementUIObjectLine, _currentPath, _pathVisual);
                //set direction for line
                Vector3 dir = _currentPath[currNode - 1].transform.position - _currentPath[currNode].transform.position;
                _pathVisual[currNode - 1].transform.rotation = Quaternion.LookRotation(dir);
                //final node
                if (currNode == _currentPath.Count - 1)
                {
                    DrawLines(420, currNode, NodeManager.Instance.movementUIObjectTarget, _currentPath, _pathVisual);
                }
            }
        }
        return _pathVisual;
    }

    public void DeleteCurrentPath()
    {
        foreach (GameObject haha in pathVisual)
            Destroy(haha);
        pathVisual.Clear();
        currentPath.Clear();
    }
}