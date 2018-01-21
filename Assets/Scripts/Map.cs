using UnityEngine;

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
        PopulateNeighbors();
        nodes[0, 0].SpawnUnit(unitDude);    //generate a guy at 0,0
    }

    void GenerateNodes(NodeTerrain[,] nodeData)
    {
        GameObject node;
        for (int i = 0; i < nodeData.GetLength(1); ++i)
        {
            for (int j = 0; j < nodeData.GetLength(0); ++j)
            {
                if (i % 2 == 0) node = Instantiate(nodeTypes[(int)nodeData[j, i]].nodePrefab, new Vector3(j * nodeSize, 0, -i * nodeSize), Quaternion.identity);
                else node = Instantiate(nodeTypes[(int)nodeData[j, i]].nodePrefab, new Vector3((j * nodeSize) - nodeSize / 2, 0, -i * nodeSize), Quaternion.identity);

                nodesGO[j, i] = node;
                nodes[j, i] = node.GetComponent<Node>();
                nodes[j, i].SetupFields(indexOfID++, j, i);
            }
        }
    }

    public void PopulateNeighbors()
    {
        foreach (Node node in nodes)
        {
            /*
            node.neighbors.Add(nodes[node.nodeXY.x+1, node.nodeXY.y]);
            node.neighbors.Add(nodes[node.nodeXY.x-1, node.nodeXY.y]);
            node.neighbors.Add(nodes[node.nodeXY.x+1, node.nodeXY.y+1]);
            node.neighbors.Add(nodes[node.nodeXY.x-1, node.nodeXY.y-1]);
            node.neighbors.Add(nodes[node.nodeXY.x+1, node.nodeXY.y-1]);
            node.neighbors.Add(nodes[node.nodeXY.x-1, node.nodeXY.y+1]);
            */
        }
    }
}