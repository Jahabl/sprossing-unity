using System.Collections;
using UnityEngine;

public class NPCController : MovementController
{
    [SerializeField] private float timeToMove = 0.4f;
    [SerializeField] private float timeToTurn = 0.1f;
    [SerializeField] private float waitTime = 0.8f;
    [SerializeField] private float radius = 7.5f;
    private Node[] path;
    private bool isBusy;

    private void Update()
    {
        if (!isBusy)
        {
            isBusy = true;
            PathRequestManager.RequestPath(transform.position, layer, worldManager.GetRandomPoint(transform.position, radius), OnPathFound);
        }
    }

    private void OnPathFound(Node[] path, bool wasSuccess)
    {
        if (wasSuccess)
        {
            this.path = path;
            StartCoroutine(FollowPath());
        }
        else
        {
            isBusy = false;
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

            if (direction != LastDirection) //turn first
            {
                //https://discussions.unity.com/t/vector2-angle-how-do-i-get-if-its-cw-or-ccw/101180/5
                bool clockwise = Mathf.Sign(LastDirection.x * direction.y - LastDirection.y * direction.x) <= 0;
                int nrOfTurns = Mathf.RoundToInt(Vector3.Angle(direction, LastDirection) / 45f);

                string[] turns = GetTurns(GetDirection(LastDirection), nrOfTurns, clockwise);

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
            LastDirection = direction;

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

        animator.PlayIdleAnimation(GetDirection(LastDirection));

        yield return new WaitForSeconds(waitTime);

        isBusy = false;
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