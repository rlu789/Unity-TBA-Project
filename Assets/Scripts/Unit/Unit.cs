using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour {

	public int tileX, tileY;
    public TileMap map;
    public List<Node> currentPath;
    
    public int moveSpeed;

    void Start()
    {
        map = GameObject.FindWithTag("Map").GetComponent<TileMap>();
        tileX = (int)this.transform.position.x; tileY = (int)this.transform.position.y;
    }

    void Update()
    {
        if (currentPath != null)
        {
            int currNode = 0;
            while (currNode < currentPath.Count - 1)
            {
                Vector3 start = map.TileCoordToWorldCoord(currentPath[currNode].x, currentPath[currNode].y) +
                    new Vector3(0, 0, -1f);
                Vector3 end = map.TileCoordToWorldCoord(currentPath[currNode + 1].x, currentPath[currNode + 1].y) +
                    new Vector3(0, 0, -1f);

                Debug.DrawLine(start, end, Color.red);

                currNode++;
            }
        }
    }

    public void MoveNextTile()
    {
        float remainingMovement = moveSpeed;

		while(remainingMovement > 0) {
			if(currentPath==null)
				return;

			// Get cost from current tile to next tile
			remainingMovement -= map.CostToEnterTile(currentPath[0].x, currentPath[0].y, currentPath[1].x, currentPath[1].y );

			// Move us to the next tile in the sequence
			tileX = currentPath[1].x;
			tileY = currentPath[1].y;

			transform.position = map.TileCoordToWorldCoord( tileX, tileY );	// Update our unity world position

			// Remove the old "current" tile
			currentPath.RemoveAt(0);

			if(currentPath.Count == 1) {
				// We only have one tile left in the path, and that tile MUST be our ultimate
				// destination -- and we are standing on it!
				// So let's just clear our pathfinding info.
				currentPath = null;
			}
		}
    }
}
