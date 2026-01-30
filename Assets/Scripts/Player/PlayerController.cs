using System.Collections;
using UnityEngine;
using static UnityEngine.InputSystem.InputAction;

public class PlayerController : MovementController
{
    [SerializeField] private Grid grid;
    [SerializeField] private float timeToMove = 0.4f;
    [SerializeField] private float timeToTurn = 0.1f;
    private bool isMoving;
    private bool isButtonDown;
    private Vector3Int input;

    private void Update()
    {
         if (!isMoving && isButtonDown)
        {
            StopAllCoroutines();
            StartCoroutine(MovePlayer());
        }
    }

    public void HandleInput(CallbackContext context)
    {
        if (context.performed)
        {
            Vector2 newInput = context.ReadValue<Vector2>();
            input = new Vector3Int(Mathf.RoundToInt(newInput.x), Mathf.RoundToInt(newInput.y));

            isButtonDown = input.x != 0 || input.y != 0;
        }
        else if (context.canceled)
        {
            isButtonDown = false;
        }
    }

    private IEnumerator MovePlayer()
    {
        isMoving = true;

        while (isButtonDown)
        {
            string dir = GetDirection(input);

            if (input != LastDirection) //turn first
            {
                //https://discussions.unity.com/t/vector2-angle-how-do-i-get-if-its-cw-or-ccw/101180/5
                bool clockwise = Mathf.Sign(LastDirection.x * input.y - LastDirection.y * input.x) <= 0;
                int nrOfTurns = Mathf.RoundToInt(Vector3.Angle(input, LastDirection) / 45f);

                string[] turns = GetTurns(GetDirection(LastDirection), nrOfTurns, clockwise);

                if (input.x != 0 || input.y != 0)
                {
                    LastDirection = input;
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

                float moveTime = timeToMove * (targetPosition - startPosition).magnitude;

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
                                while (elapsedTime < moveTime)
                                {
                                    transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / moveTime);
                                    elapsedTime += Time.deltaTime;
                                    yield return null;
                                }

                                transform.position = targetPosition;
                                layer = targetLayer;

                                if ((layer - 1) % 3 != 0) //ramp
                                {
                                    spriteRenderer.sortingOrder = layer;
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
                        while (elapsedTime < moveTime)
                        {
                            transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / moveTime);
                            elapsedTime += Time.deltaTime;
                            yield return null;
                        }

                        transform.position = targetPosition;
                        layer = targetLayer;

                        if ((layer - 1) % 3 != 0) //ramp
                        {
                            spriteRenderer.sortingOrder = layer;
                        }
                        else
                        {
                            spriteRenderer.sortingOrder = layer - 5;
                        }
                    }
                }

                if (input.x != 0 || input.y != 0)
                {
                    LastDirection = input;
                }
            }

            yield return null;
        }

        animator.PlayIdleAnimation(GetDirection(LastDirection));
        isMoving = false;
    }

    public void Pathing()
    {
        if (LastDirection.x != 0 && LastDirection.y != 0) //can't be on diagonal
        {
            return;
        }

        worldManager.Pathing(transform.position + new Vector3(LastDirection.x * grid.cellSize.x, LastDirection.y * grid.cellSize.y, 0), layer);
    }

    public void Terraform()
    {
        if (LastDirection.x != 0 && LastDirection.y != 0) //can't be on diagonal
        {
            return;
        }

        worldManager.Terraform(transform.position + new Vector3(LastDirection.x * grid.cellSize.x, LastDirection.y * grid.cellSize.y, 0), layer);
    }

    public void Waterscape()
    {
        if (LastDirection.x != 0 && LastDirection.y != 0) //can't be on diagonal
        {
            return;
        }

        worldManager.Waterscape(transform.position + new Vector3(LastDirection.x * grid.cellSize.x, LastDirection.y * grid.cellSize.y, 0), layer);
    }

    public void PlaceRamp()
    {
        if (LastDirection.x != 0) //not facing right direction
        {
            return;
        }

        worldManager.PlaceRamp(transform.position + new Vector3(LastDirection.x * grid.cellSize.x, LastDirection.y * grid.cellSize.y, 0), layer);
    }

    public void PlaceHouse()
    {
        if (LastDirection.x != 0 || LastDirection.y != 1) //not facing up
        {
            return;
        }

        worldManager.PlaceHouse(transform.position + new Vector3(LastDirection.x * grid.cellSize.x, LastDirection.y * grid.cellSize.y, 0), layer);
    }

    public void PlaceBridge(int width)
    {
        if (LastDirection.x != 0 && LastDirection.y != 0) //can't be on diagonal
        {
            return;
        }

        worldManager.PlaceBridge(transform.position, layer, LastDirection, width);
    }

    public void PlaceFence()
    {
        if (LastDirection.x != 0 && LastDirection.y != 0) //can't be on diagonal
        {
            return;
        }

        worldManager.PlaceFence(transform.position + new Vector3(LastDirection.x * grid.cellSize.x, LastDirection.y * grid.cellSize.y, 0), layer);
    }
}