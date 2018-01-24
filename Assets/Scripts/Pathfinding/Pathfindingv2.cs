using System.Collections.Generic;
using UnityEngine;

public class Pathfindingv2 : MonoBehaviour
{
    public static Path<Node> FindPath(Node start, Node destination, List<Node> _closed = null)
    {
        if (!destination.passable || destination.moveCost >= 100) return null;
        var closed = new HashSet<Node>();
        var queue = new PriorityQueue<double, Path<Node>>();
        queue.Enqueue(0, new Path<Node>(start));

        if (_closed != null)    //blacklisted nodes that we can't end on
        {
            foreach (Node node in _closed)
            {
                closed.Add(node);
            }
        }

        while (!queue.IsEmpty)
        {
            var path = queue.Dequeue();

            if (closed.Contains(path.LastStep)) continue;

            if (path.LastStep.Equals(destination)) return path;

            closed.Add(path.LastStep);

            foreach (Node n in path.LastStep.neighbours)
            {
                double d = Distance(path.LastStep, n);
                var newPath = path.AddStep(n, d);
                queue.Enqueue(newPath.TotalCost + Estimate(n, destination), newPath);
            }
        }

        return null;
    }

    static double Distance(Node nodeA, Node nodeB)  //distance to the next node is the cost it takes to move there
    {
        if (TurnHandler.Instance.currentState == TurnHandlerStates.PLAYERMOVE)
        {
            if (nodeB.potentialUnit != null && nodeB.potentialUnit.isEnemy || nodeB.currentUnit != null && nodeB.currentUnit.isEnemy)
            {
                return Mathf.Infinity;
            }
        }
        else
        {
            if (nodeB.potentialUnit != null && !nodeB.potentialUnit.isEnemy || nodeB.currentUnit != null && !nodeB.currentUnit.isEnemy)
            {
                return Mathf.Infinity;
            }
        }

        return nodeB.moveCost;
    }

    static double Estimate(Node node, Node destNode)    //minimum grid movements to make it to the destination node (assuming a direct path)
    {
        float dx = Mathf.Abs(destNode.XY.x - node.XY.x);
        float dy = Mathf.Abs(destNode.XY.y - node.XY.y);
        int z1 = -(node.XY.x + node.XY.y);
        int z2 = -(destNode.XY.x + destNode.XY.y);
        float dz = Mathf.Abs(z2 - z1);

        return Mathf.Max(dx, dy, dz);
    }
}