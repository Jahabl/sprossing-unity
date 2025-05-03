using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerController : MovementController
{
    [SerializeField] private Grid grid;
    [SerializeField] private WorldManager worldManager;
    [SerializeField] private float timeToMove = 0.4f;
    [SerializeField] private float timeToTurn = 0.1f;
    private bool isMoving;
    private bool isButtonDown;
    private Vector3Int input;

    private void Update()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");

        input.x = Mathf.RoundToInt(moveX);
        input.y = Mathf.RoundToInt(moveY);

        isButtonDown = input.x != 0 || input.y != 0;

        if (!isMoving && isButtonDown)
        {
            StopAllCoroutines();
            StartCoroutine(MovePlayer());
        }
    }

    private IEnumerator MovePlayer()
    {
        isMoving = true;

        while (isButtonDown)
        {
            string dir = GetDirection(input);

            if (input != lastDirection) //turn first
            {
                //https://discussions.unity.com/t/vector2-angle-how-do-i-get-if-its-cw-or-ccw/101180/5
                bool clockwise = Mathf.Sign(lastDirection.x * input.y - lastDirection.y * input.x) <= 0;
                int nrOfTurns = Mathf.RoundToInt(Vector3.Angle(input, lastDirection) / 45f);

                string[] turns = GetTurns(GetDirection(lastDirection), nrOfTurns, clockwise);

                if (input.x != 0 || input.y != 0)
                {
                    lastDirection = input;
                }

                for (int i = 0; i < nrOfTurns; i++)
                {
                    animator.PlayTurnAnimation(turns[i]);
                    yield return new WaitForSeconds(timeToTurn);
                }
            }
            else
            {
                float elapsedTime = 0f;

                Vector3Int gridPosition = grid.WorldToCell(transform.position);

                startPosition = transform.position;
                targetPosition = grid.CellToWorld(gridPosition + input);

                animator.PlayWalkAnimation(dir);

                int targetLayer = worldManager.GetPositionLevel(targetPosition, layer, input);

                if (targetLayer > 0)
                {
                    if (input.x != 0 && input.y != 0) //diagonal movement
                    {
                        Vector3 checkPosition = grid.CellToWorld(gridPosition + new Vector3Int(input.x, 0, 0));

                        if (worldManager.GetPositionLevel(checkPosition, layer, Vector3Int.zero) == layer)
                        {
                            checkPosition = grid.CellToWorld(gridPosition + new Vector3Int(0, input.y, 0));

                            if (worldManager.GetPositionLevel(checkPosition, layer, Vector3Int.zero) == layer)
                            {
                                while (elapsedTime < timeToMove * (targetPosition - startPosition).magnitude)
                                {
                                    transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / timeToMove);
                                    elapsedTime += Time.deltaTime;
                                    yield return null;
                                }

                                transform.position = targetPosition;
                                layer = targetLayer;

                                if ((layer - 1) % 3 != 0)
                                {
                                    spriteRenderer.sortingOrder = layer - 4;
                                }
                                else
                                {
                                    spriteRenderer.sortingOrder = layer - 5;
                                }
                            }
                        }
                    }
                    else
                    {
                        while (elapsedTime < timeToMove * (targetPosition - startPosition).magnitude)
                        {
                            transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / timeToMove);
                            elapsedTime += Time.deltaTime;
                            yield return null;
                        }

                        transform.position = targetPosition;
                        layer = targetLayer;

                        if ((layer - 1) % 3 != 0)
                        {
                            spriteRenderer.sortingOrder = layer - 4;
                        }
                        else
                        {
                            spriteRenderer.sortingOrder = layer - 5;
                        }
                    }
                }

                if (input.x != 0 || input.y != 0)
                {
                    lastDirection = input;
                }
            }

            yield return null;
        }

        animator.PlayIdleAnimation(GetDirection(lastDirection));
        isMoving = false;
    }

    public void Pathing()
    {
        if (lastDirection.x != 0 && lastDirection.y != 0)
            return;

        worldManager.Pathing(transform.position + new Vector3(lastDirection.x * grid.cellSize.x, lastDirection.y * grid.cellSize.y, 0), layer);
    }

    public void Terraform()
    {
        if (lastDirection.x != 0 && lastDirection.y != 0)
            return;

        worldManager.Terraform(transform.position + new Vector3(lastDirection.x * grid.cellSize.x, lastDirection.y * grid.cellSize.y, 0), layer);
    }

    public void Waterscape()
    {
        if (lastDirection.x != 0 && lastDirection.y != 0)
            return;

        worldManager.Waterscape(transform.position + new Vector3(lastDirection.x * grid.cellSize.x, lastDirection.y * grid.cellSize.y, 0), layer);
    }

    public void Ramp()
    {
        if (lastDirection.x != 0 || lastDirection.y != 1)
            return;

        worldManager.PlaceRemoveRamp(transform.position + new Vector3(lastDirection.x * grid.cellSize.x, lastDirection.y * grid.cellSize.y, 0), layer);
    }
}