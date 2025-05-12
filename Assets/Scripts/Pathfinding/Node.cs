using System;
using UnityEngine;

public class Node : IHeapItem<Node>
{
    public Vector3 worldPosition;
    public int gridX, gridY;
    public int gridID;
    public int movementPenalty;
    public Node parent;
    private int heapIndex;
    public int layer;

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

    public int GetLevel(int layer, Vector3Int direction) //player movement
    {
        if (gridID <= 0)
        {
            return gridID;
        }

        if (layer == gridID || gridID % layer == 0)
        {
            return layer;
        }
        else if (gridID - layer <= 6 && gridID - layer >= 5)
        {
            return layer;
        }
        else if (direction.x == 0 && Math.Abs(layer - gridID) == 1) //ramp
        {
            return gridID;
        }
        else if (direction.x == 0 && gridID % (layer + 1) == 0) //ramp
        {
            return layer + 1;
        }

        return 0;
    }

    public bool IsWalkable(int layer) //NPC movement
    {
        if (gridID <= 0)
        {
            return false;
        }

        if (layer == gridID || gridID % layer == 0)
        {
            return true;
        }
        else if (gridID - layer <= 6 && gridID - layer >= 5)
        {
            return true;
        }

        return false;
    }
}
