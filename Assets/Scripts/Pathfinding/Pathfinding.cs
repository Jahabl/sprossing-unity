using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Pathfinding : MonoBehaviour
{
    [SerializeField] private NodeGrid nodes;
    [SerializeField] private PathRequestManager requestManager;

    public void StartFindPath(Vector3 startPosition, Vector3 targetPosition)
    {
        StartCoroutine(FindPath(startPosition, targetPosition));
    }

    IEnumerator FindPath(Vector3 startPosition, Vector3 targetPosition)
    {
        Node[] waypoints = new Node[0];
        bool pathWasFound = false;

        Node startNode = nodes.GetNodeFromWorldPosition(startPosition);
        Node targetNode = nodes.GetNodeFromWorldPosition(targetPosition);

        if (startPosition == targetPosition || startNode == null || targetNode == null)
        {
            requestManager.FinishedProcessingPath(waypoints, pathWasFound);
            yield break;
        }

        startNode.layer = Mathf.RoundToInt(startPosition.z);
        targetNode.layer = Mathf.RoundToInt(targetPosition.z);

        Heap<Node> openSet = new Heap<Node>(nodes.MaxSize);
        HashSet<Node> closedSet = new HashSet<Node>();

        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            Node currentNode = openSet.RemoveFirst();
            closedSet.Add(currentNode);

            if (currentNode == targetNode) //found path  && currentNode.layer == targetPosition.z
            {
                pathWasFound = true;
                break;
            }

            foreach (Node neighbor in nodes.GetNeighbors(currentNode))
            {
                int newLayer = neighbor.GetLevel(currentNode.layer, new Vector3Int(neighbor.gridX - currentNode.gridX, neighbor.gridY - currentNode.gridY, 0));
                if (newLayer <= 0 || closedSet.Contains(neighbor))
                {
                    continue;
                }

                neighbor.layer = newLayer;
                int newCost = currentNode.gCost + GetDistance(currentNode, neighbor) + neighbor.movementPenalty;

                if (newCost < neighbor.gCost || !openSet.Contains(neighbor))
                {
                    neighbor.gCost = newCost;
                    neighbor.hCost = GetDistance(neighbor, targetNode);
                    neighbor.parent = currentNode;

                    if (!openSet.Contains(neighbor))
                    {
                        openSet.Add(neighbor);
                    }
                    else
                    {
                        openSet.UpdateItem(neighbor);
                    }
                }
            }
        }

        yield return null;

        if (pathWasFound)
        {
            waypoints = RetracePath(startNode, targetNode);
            pathWasFound = waypoints.Length > 0;
        }

        requestManager.FinishedProcessingPath(waypoints, pathWasFound);
    }

    private int GetDistance(Node nodeA, Node nodeB)
    {
        int deltaX = Math.Abs(nodeA.gridX - nodeB.gridX);
        int deltaY = Math.Abs(nodeA.gridY - nodeB.gridY);
        int deltaZ = Math.Abs(nodeA.layer - nodeB.layer);

        if (deltaX > deltaY)
        {
            return 14 * deltaY + 10 * (deltaX -  deltaY) + 5 * deltaZ;
        }
        else
        {
            return 14 * deltaX + 10 * (deltaY -  deltaX) + 5 * deltaZ;
        }
    }

    private Node[] RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }

        path.Reverse();

        return path.ToArray();
    }
}
