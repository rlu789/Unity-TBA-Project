using System.Collections;
using System.Collections.Generic;

//Path is a node linked to a previous node, with a cost equal to the total cost of all the nodes before it
public class Path<Node> : IEnumerable<Node>
{
    public Node LastStep { get; private set; }
    public Path<Node> PreviousSteps { get; private set; }
    public double TotalCost { get; private set; }

    private Path(Node lastStep, Path<Node> previousSteps, double totalCost)
    {
        LastStep = lastStep;
        PreviousSteps = previousSteps;
        TotalCost = totalCost;
    }

    public Path(Node start) : this(start, null, 0) { }

    public Path<Node> AddStep(Node step, double stepCost)
    {
        return new Path<Node>(step, this, TotalCost + stepCost);
    }

    public List<Node> ToList()  //reverses and returns the path as a list
    {
        List<Node> ret = new List<Node>();

        foreach (Node node in this) ret.Add(node);

        ret.Reverse();
        return ret;
    }

    public IEnumerator<Node> GetEnumerator()
    {
        for (var p = this; p != null; p = p.PreviousSteps) yield return p.LastStep;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}