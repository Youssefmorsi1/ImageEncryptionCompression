using System.Collections.Generic;
using System;
using ImageEncryptCompress;

class PriorityQueue<node>
{
    private List<Node> list;
    public int Count { get { return list.Count; } }
    public readonly bool IsDescending;

    public PriorityQueue()
    {
        list = new List<Node>();
    }

    public PriorityQueue(bool isdesc)
        : this()
    {
        IsDescending = isdesc;
    }

    public PriorityQueue(int capacity)
        : this(capacity, false)
    { }

    public PriorityQueue(IEnumerable<Node> collection)
        : this(collection, false)
    { }

    public PriorityQueue(int capacity, bool isdesc)
    {
        list = new List<Node>(capacity);
        IsDescending = isdesc;
    }

    public PriorityQueue(IEnumerable<Node> collection, bool isdesc)
        : this()
    {
        IsDescending = isdesc;
        foreach (var item in collection)
            Enqueue(item);
    }


    public void Enqueue(Node x)
    {
        list.Add(x);
        int i = Count - 1;

        while (i > 0)
        {
            int p = (i - 1) / 2;
            if ((IsDescending ? -1 : 1) * Node.NodeComparer(list[p],x) <= 0) break;

            list[i] = list[p];
            i = p;
        }

        if (Count > 0) list[i] = x;
    }

    public Node Dequeue()
    {
        Node target = Peek();
        Node root = list[Count - 1];
        list.RemoveAt(Count - 1);

        int i = 0;
        while (i * 2 + 1 < Count)
        {
            int a = i * 2 + 1;
            int b = i * 2 + 2;
            int c = b < Count && (IsDescending ? -1 : 1) * Node.NodeComparer(list[b], list[a]) < 0 ? b : a;

            if ((IsDescending ? -1 : 1) * Node.NodeComparer( list[c], root) >= 0) break;
            list[i] = list[c];
            i = c;
        }

        if (Count > 0) list[i] = root;
        return target;
    }

    public Node Peek()
    {
        if (Count == 0) throw new InvalidOperationException("Queue is empty.");
        return list[0];
    }


    public void Clear()
    {
        list.Clear();
    }
}