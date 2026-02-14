using System;
using System.Collections.Generic;
using UnityEngine;

public class PathRequestManager : MonoBehaviour
{
    public static PathRequestManager instance;

    private readonly Queue<PathRequest> requestQueue = new Queue<PathRequest>();
    private PathRequest currRequest;
    [SerializeField] private Pathfinding pathfinding;
    private bool isProcessing;

    private void Start()
    {
        instance = this;
    }

    struct PathRequest
    {
        public Vector3 pathStart;
        public Vector3 pathEnd;
        public Action<Node[], bool> callback;

        public PathRequest(Vector3 pathStart, Vector3 pathEnd, Action<Node[], bool> callback)
        {
            this.pathStart = pathStart;
            this.pathEnd = pathEnd;
            this.callback = callback;
        }
    }

    public static void RequestPath(Vector3 pathStart, Vector3 pathEnd, Action<Node[], bool> callback)
    {
        PathRequest newRequest = new PathRequest(pathStart, pathEnd, callback);
        instance.requestQueue.Enqueue(newRequest);
        instance.TryProcessNext();
    }

    private void TryProcessNext()
    {
        if (!isProcessing && requestQueue.Count > 0)
        {
            currRequest = requestQueue.Dequeue();
            isProcessing = true;

            pathfinding.StartFindPath(currRequest.pathStart, currRequest.pathEnd);
        }
    }

    public void FinishedProcessingPath(Node[] path, bool wasSuccess)
    {
        currRequest.callback(path, wasSuccess);
        isProcessing = false;

        TryProcessNext();
    }
}
