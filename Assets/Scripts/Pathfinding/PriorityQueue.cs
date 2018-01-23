using System.Collections.Generic;
using System.Linq;

//Generic implementation of a priority queue
class PriorityQueue<P, V>
{
    private readonly SortedDictionary<P, Queue<V>> list = new SortedDictionary<P, Queue<V>>();

    public void Enqueue(P priority, V value)
    {
        Queue<V> q;
        if (!list.TryGetValue(priority, out q)) //check if the path to this node is in the dictionary, which is sorted by the total movement cost
        {
            q = new Queue<V>();
            list.Add(priority, q);  //add the path of nodes
        }
        q.Enqueue(value);
    }

    public V Dequeue()
    {
        //will throw if there isn’t any first element!
        var pair = list.First();
        var v = pair.Value.Dequeue();                       //from the highest priority (lowest total movement cost), get a path
        if (pair.Value.Count == 0) list.Remove(pair.Key);   //nothing left of the top priority. 
        return v;                                           //continue pathfinding from the lowest cost path
    }

    public bool IsEmpty
    {
        get { return !list.Any(); } //is anything left in the queue?
    }
}