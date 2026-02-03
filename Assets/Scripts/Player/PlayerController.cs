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

    [SerializeField] private SpriteRenderer bubble;

    protected override void SetSortingOrder(int layer)
    {
        base.SetSortingOrder(layer);
        bubble.sortingOrder = spriteRenderer.sortingOrder + 3;
    }

    private void Update()
    {
        if (!isMoving && isButtonDown)
        {
            StopCoroutine("MovePlayer");
            StartCoroutine("MovePlayer");
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

                                SetSortingOrder(layer);
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

                        SetSortingOrder(layer);
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

    private void EnableBubble()
    {
        StopCoroutine("HideBubble");
        bubble.gameObject.SetActive(true);
        StartCoroutine("HideBubble");
    }

    IEnumerator HideBubble()
    {
        yield return new WaitForSeconds(0.5f);
        bubble.gameObject.SetActive(false);
    }

    public void Pathing()
    {
        bool wasSuccess;
        if (LastDirection.x != 0 && LastDirection.y != 0) //can't be on diagonal
        {
            wasSuccess = false;
        }
        else
        {
            wasSuccess = worldManager.Pathing(transform.position + new Vector3(LastDirection.x * grid.cellSize.x, LastDirection.y * grid.cellSize.y, 0), layer);
        }

        if (!wasSuccess)
        {
            EnableBubble();
        }
    }

    public void Terraform()
    {
        bool wasSuccess;
        if (LastDirection.x != 0 && LastDirection.y != 0) //can't be on diagonal
        {
            wasSuccess = false;
        }
        else
        {
            wasSuccess = worldManager.Terraform(transform.position + new Vector3(LastDirection.x * grid.cellSize.x, LastDirection.y * grid.cellSize.y, 0), layer);
        }

        if (!wasSuccess)
        {
            EnableBubble();
        }
    }

    public void Waterscape()
    {
        bool wasSuccess;
        if (LastDirection.x != 0 && LastDirection.y != 0) //can't be on diagonal
        {
            wasSuccess = false;
        }
        else
        {
            wasSuccess = worldManager.Waterscape(transform.position + new Vector3(LastDirection.x * grid.cellSize.x, LastDirection.y * grid.cellSize.y, 0), layer);
        }

        if (!wasSuccess)
        {
            EnableBubble();
        }
    }

    public void PlaceRamp()
    {
        bool wasSuccess;
        if (LastDirection.x != 0) //not facing right direction
        {
            wasSuccess = false;
        }
        else
        {
            wasSuccess = worldManager.PlaceRamp(transform.position + new Vector3(LastDirection.x * grid.cellSize.x, LastDirection.y * grid.cellSize.y, 0), layer);
        }

        if (!wasSuccess)
        {
            EnableBubble();
        }
    }

    public void PlaceHouse()
    {
        bool wasSuccess;
        if (LastDirection.x != 0 || LastDirection.y != 1) //not facing up
        {
            wasSuccess = false;
        }
        else
        {
            wasSuccess = worldManager.PlaceHouse(transform.position + new Vector3(LastDirection.x * grid.cellSize.x, LastDirection.y * grid.cellSize.y, 0), layer);
        }

        if (!wasSuccess)
        {
            EnableBubble();
        }
    }

    public void PlaceBridge(int width)
    {
        bool wasSuccess;
        if (LastDirection.x != 0 && LastDirection.y != 0) //can't be on diagonal
        {
            wasSuccess = false;
        }
        else
        {
            wasSuccess = worldManager.PlaceBridge(transform.position, layer, LastDirection, width);
        }

        if (!wasSuccess)
        {
            EnableBubble();
        }
    }

    public void PlaceFence()
    {
        bool wasSuccess;
        if (LastDirection.x != 0 && LastDirection.y != 0) //can't be on diagonal
        {
            wasSuccess = false;
        }
        else
        {
            wasSuccess = worldManager.PlaceFence(transform.position + new Vector3(LastDirection.x * grid.cellSize.x, LastDirection.y * grid.cellSize.y, 0), layer);
        }

        if (!wasSuccess)
        {
            EnableBubble();
        }
    }

    public void PlaceTree()
    {
        bool wasSuccess;
        if (LastDirection.x != 0 && LastDirection.y != 0) //can't be on diagonal
        {
            wasSuccess = false;
        }
        else
        {
            wasSuccess = worldManager.PlaceTree(transform.position + new Vector3(LastDirection.x * grid.cellSize.x, LastDirection.y * grid.cellSize.y, 0), layer);
        }

        if (!wasSuccess)
        {
            EnableBubble();
        }
    }

    public void RemoveStructure()
    {
        bool wasSuccess;
        if (LastDirection.x != 0 && LastDirection.y != 0) //can't be on diagonal
        {
            wasSuccess = false;
        }
        else
        {
            wasSuccess = worldManager.RemoveStructure(transform.position + new Vector3(LastDirection.x * grid.cellSize.x, LastDirection.y * grid.cellSize.y, 0), layer);
        }

        if (!wasSuccess)
        {
            EnableBubble();
        }
    }
}