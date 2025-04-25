using UnityEngine;

public class Node : IHeapItem<Node>
{
    public bool[] isWalkable;
    public Vector3 worldPosition;
    public int gridX, gridY;
    public Node parent;
    private int heapIndex;

    public int gCost;
    public int hCost;

    public int fCost
    {
        get
        {
            return gCost + hCost;
        }
    }

    public Node(Vector3 worldPosition, int gridX, int gridY)
    {
        isWalkable = new bool[3];

        this.worldPosition = worldPosition;
        this.gridX = gridX;
        this.gridY = gridY;
    }

    public int HeapIndex
    {
        get
        {
            return heapIndex;
        }
        set
        {
            heapIndex = value;
        }
    }

    public int CompareTo(Node nodeToCompare)
    {
        int compare = fCost.CompareTo(nodeToCompare.fCost);
        if (compare == 0)
        {
            compare = hCost.CompareTo(nodeToCompare.hCost);
        }

        return -compare;
    }
}
