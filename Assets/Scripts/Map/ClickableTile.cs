using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickableTile : MonoBehaviour {
    public int tileX;
    public int tileY;
    public TileMap map;

	void OnMouseUp()
    {
        Debug.Log("fe");
        if (map.selectedUnit != null)
            map.MoveSelectedUnitTo(tileX, tileY);
    }
}
