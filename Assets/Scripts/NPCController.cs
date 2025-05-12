using System.Collections;
using UnityEngine;

public class NPCController : MovementController
{
    [SerializeField] private float timeToMove = 0.4f;
    [SerializeField] private float timeToTurn = 0.1f;
    [SerializeField] private Transform target;
    private Node[] path;
    private bool isBusy;

    private void Update()
    {
        if (!isBusy)
        {
            isBusy = true;
            PathRequestManager.RequestPath(transform.position, layer, target.position, OnPathFound);
        }
    }

    private void OnPathFound(Node[] path, bool wasSuccess)
    {
        if (wasSuccess)
        {
            this.path = path;
            StartCoroutine(FollowPath());
        }
    }

    IEnumerator FollowPath()
    {
        for (int i = 0; i < path.Length; i++)
        {
            startPosition = transform.position;
            targetPosition = path[i].worldPosition;
            targetPosition.z = 0;

            Vector3 pathDirection = targetPosition - startPosition;
            Vector3Int direction = new Vector3Int(Mathf.RoundToInt(pathDirection.x), Mathf.RoundToInt(pathDirection.y), 0);
            string dir = GetDirection(direction);

            if (direction != lastDirection) //turn first
            {
                //https://discussions.unity.com/t/vector2-angle-how-do-i-get-if-its-cw-or-ccw/101180/5
                bool clockwise = Mathf.Sign(lastDirection.x * direction.y - lastDirection.y * direction.x) <= 0;
                int nrOfTurns = Mathf.RoundToInt(Vector3.Angle(direction, lastDirection) / 45f);

                string[] turns = GetTurns(GetDirection(lastDirection), nrOfTurns, clockwise);

                for (int j = 0; j < nrOfTurns; j++)
                {
                    animator.PlayTurnAnimation(turns[j]);
                    yield return new WaitForSeconds(timeToTurn);
                }
            }

            float elapsedTime = 0f;

            animator.PlayWalkAnimation(dir);

            while (elapsedTime < timeToMove * pathDirection.magnitude)
            {
                transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / timeToMove);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            transform.position = targetPosition;
            lastDirection = direction;

            layer = path[i].layer;

            if ((layer - 1) % 3 != 0)
            {
                spriteRenderer.sortingOrder = layer - 4;
            }
            else
            {
                spriteRenderer.sortingOrder = layer - 5;
            }
        }

        animator.PlayIdleAnimation(GetDirection(lastDirection));
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.black;

        if (path != null && path.Length > 0)
        {
            for (int i = 0; i < path.Length; i++)
            {
                Gizmos.DrawCube(path[i].worldPosition, Vector3.one * 0.25f);

                if (i > 0)
                {
                    Gizmos.DrawLine(path[i - 1].worldPosition, path[i].worldPosition);
                }
            }
        }
    }
}