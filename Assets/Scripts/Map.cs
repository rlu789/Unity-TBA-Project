using UnityEngine;
using System.Collections.Generic;

enum NodeTerrain { Ground, Wall, Water }

public class Map : MonoBehaviour
{
    public static Map Instance;
    int indexOfID = 0;

    public NodeType[] nodeTypes;
    NodeTerrain[,] nodeMap; //maps each spot on the map to an int for generation

    GameObject[,] nodesGO;  //the GameObject for each hex on the map
    Node[,] nodes;  //the node component of each hex on the map

    [Space(10)]
    public int mapSizeX = 10;
    public int mapSizeY = 8;
    public int nodeSize = 2;
    [Space(10)]
    public GameObject unitDude;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Map already exists!");
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        GenerateMap(mapSizeX, mapSizeY);
    }

    void GenerateMap(int length, int height)    //generates map data
    {
        nodeMap = new NodeTerrain[length, height];
        nodesGO = new GameObject[length, height];
        nodes = new Node[length, height];

        //randomise/select terrain types here
        nodeMap[3, 3] = NodeTerrain.Wall;
        nodeMap[3, 4] = NodeTerrain.Wall;
        nodeMap[3, 5] = NodeTerrain.Wall;

        nodeMap[5, 1] = NodeTerrain.Water;
        nodeMap[5, 2] = NodeTerrain.Water;
        nodeMap[6, 1] = NodeTerrain.Water;
        nodeMap[6, 2] = NodeTerrain.Water;

        GenerateNodes(nodeMap); //generate a grid of nodes based on the data in nodeMap
        //PopulateNeighbors();
        nodes[0, 0].SpawnUnit(unitDude);    //generate a guy at 0,0
    }

    void GenerateNodes(NodeTerrain[,] nodeData)
    {
        GameObject node;
        for (int y = 0; y < nodeData.GetLength(1); ++y)
        {
            for (int x = 0; x < nodeData.GetLength(0); ++x)
            {
                if (y % 2 == 0) node = Instantiate(nodeTypes[(int)nodeData[x, y]].nodePrefab, new Vector3(x * nodeSize, 0, -y * nodeSize), Quaternion.identity);
                else node = Instantiate(nodeTypes[(int)nodeData[x, y]].nodePrefab, new Vector3((x * nodeSize) - nodeSize / 2, 0, -y * nodeSize), Quaternion.identity);

                nodesGO[x, y] = node;
                nodes[x, y] = node.GetComponent<Node>();
                nodes[x, y].SetupFields(indexOfID++, x, y);
            }
        }

        VisitNeighbours();
    }

    public void VisitNeighbours()
    {
        for (int x = 0; x < mapSizeX; x++)
        {
            for (int y = 0; y < mapSizeY; y++)
            {
                if (x != mapSizeX - 1)
                {
                    nodes[x, y].neighbours.Add(nodes[nodes[x, y].nodeXY.x + 1, nodes[x, y].nodeXY.y]);
                    if (y != mapSizeY - 1)
                        nodes[x, y].neighbours.Add(nodes[nodes[x, y].nodeXY.x + 1, nodes[x, y].nodeXY.y + 1]);
                    if (y > 0)
                        nodes[x, y].neighbours.Add(nodes[nodes[x, y].nodeXY.x + 1, nodes[x, y].nodeXY.y - 1]);
                }
                if (x > 0)
                {
                    nodes[x, y].neighbours.Add(nodes[nodes[x, y].nodeXY.x - 1, nodes[x, y].nodeXY.y]);
                    if (y > 0)
                        nodes[x, y].neighbours.Add(nodes[nodes[x, y].nodeXY.x - 1, nodes[x, y].nodeXY.y - 1]);
                    if (y != mapSizeY - 1)
                        nodes[x, y].neighbours.Add(nodes[nodes[x, y].nodeXY.x - 1, nodes[x, y].nodeXY.y + 1]);
                }
            }
        }
    }

    public float CostToEnterTile(int sourceX, int sourceY, int targetX, int targetY)
    {

        NodeType tt = nodeTypes[(int)nodeMap[targetX, targetY]];

        float cost = tt.moveCost;

        return cost;

    }

    public void AStarLite(Unit unit, Node destNode)
    {
        // Clear out our unit's old path.
        unit.currentPath = null;

        Dictionary<Node, float> dist = new Dictionary<Node, float>();
        Dictionary<Node, Node> prev = new Dictionary<Node, Node>();

        // Setup the "Q" -- the list of nodes we haven't checked yet.
        List<Node> unvisited = new List<Node>();

        Node source = nodes[
                            unit.XY.x,
                            unit.XY.y
                            ];

        Node target = nodes[
                            destNode.nodeXY.x,
                            destNode.nodeXY.y
                            ];

        dist[source] = 0;
        prev[source] = null;

        // Initialize everything to have INFINITY distance, since
        // we don't know any better right now. Also, it's possible
        // that some nodes CAN'T be reached from the source,
        // which would make INFINITY a reasonable value
        foreach (Node v in nodes)
        {
            if (v != source)
            {
                dist[v] = Mathf.Infinity;
                prev[v] = null;
            }

            unvisited.Add(v);
        }

        while (unvisited.Count > 0)
        {
            // "u" is going to be the unvisited node with the smallest distance.
            Node u = null;

            foreach (Node possibleU in unvisited)
            {
                if (u == null || dist[possibleU] < dist[u])
                {
                    u = possibleU;
                }
            }

            if (u == target)
            {
                break;  // Exit the while loop!
            }

            unvisited.Remove(u);

            foreach (Node v in u.neighbours)
            {
                //float alt = dist[u] + u.DistanceTo(v);
                Debug.Log(dist[v]);
                Debug.Log(u);
                float alt = dist[u] + CostToEnterTile(u.nodeXY.x, u.nodeXY.y, v.nodeXY.x, v.nodeXY.y);
                if (alt < dist[v])
                {
                    dist[v] = alt;
                    prev[v] = u;
                }
            }
        }

        // If we get there, the either we found the shortest route
        // to our target, or there is no route at ALL to our target.

        if (prev[target] == null)
        {
            // No route between our target and the source
            return;
        }

        List<Node> currentPath = new List<Node>();

        Node curr = target;

        // Step through the "prev" chain and add it to our path
        while (curr != null)
        {
            currentPath.Add(curr);
            curr = prev[curr];
        }

        // Right now, currentPath describes a route from out target to our source
        // So we need to invert it!

        currentPath.Reverse();

        unit.currentPath = currentPath;
    }
}