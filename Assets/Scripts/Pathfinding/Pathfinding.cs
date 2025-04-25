using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Pathfinding : MonoBehaviour
{
    private NodeGrid nodes;
    private PathRequestManager requestManager;

    private void Start()
    {
        nodes = GetComponent<NodeGrid>();
        requestManager = GetComponent<PathRequestManager>();
    }

    public void StartFindPath(Vector3 startPosition, Vector3 targetPosition)
    {
        StartCoroutine(FindPath(startPosition, targetPosition));
    }

    IEnumerator FindPath(Vector3 startPosition, Vector3 targetPosition)
    {
        Vector3[] waypoints = new Vector3[0];
        bool pathWasFound = false;

        Node startNode = nodes.GetNodeFromWorldPosition(startPosition);
        Node targetNode = nodes.GetNodeFromWorldPosition(targetPosition);

        if (startPosition == targetPosition || startNode == null || targetNode == null || !targetNode.isWalkable[0])
        {
            requestManager.FinishedProcessingPath(waypoints, pathWasFound);
            yield break;
        }

        Heap<Node> openSet = new Heap<Node>(nodes.MaxSize);
        HashSet<Node> closedSet = new HashSet<Node>();

        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            Node currentNode = openSet.RemoveFirst();
            closedSet.Add(currentNode);

            if (currentNode == targetNode) //found path
            {
                pathWasFound = true;
                break;
            }

            foreach (Node neighbor in nodes.GetNeighbors(currentNode))
            {
                if (!neighbor.isWalkable[0] || closedSet.Contains(neighbor))
                {
                    continue;
                }

                int newCost = currentNode.gCost + GetDistance(currentNode, neighbor);

                if (newCost < neighbor.gCost || !openSet.Contains(neighbor))
                {
                    neighbor.gCost = newCost;
                    neighbor.hCost = GetDistance(neighbor, targetNode);
                    neighbor.parent = currentNode;

                    if (!openSet.Contains(neighbor))
                    {
                        openSet.Add(neighbor);
                    }
                }
            }
        }

        yield return null;

        if (pathWasFound)
        {
            waypoints = RetracePath(startNode, targetNode);
        }

        requestManager.FinishedProcessingPath(waypoints, pathWasFound);
    }

    private int GetDistance(Node nodeA, Node nodeB)
    {
        int distX = Math.Abs(nodeA.gridX - nodeB.gridX);
        int distY = Math.Abs(nodeA.gridY - nodeB.gridY);

        if (distX > distY)
        {
            return 14 * distY + 10 * (distX -  distY);
        }
        else
        {
            return 14 * distX + 10 * (distY -  distX);
        }
    }

    private Vector3[] RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }

        path.Reverse();

        Vector3[] waypoints = new Vector3[path.Count];

        for (int i = 0; i < path.Count; i++)
        {
            waypoints[i] = path[i].worldPosition;
        }

        return waypoints;
    }
}
