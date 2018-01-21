using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TileType { // No mono behaviour; a raw basic class
    public string name;
    public GameObject tileVisualPrefab;

    public float movementCost = 1;
    //public bool isWalkable = true;

}
