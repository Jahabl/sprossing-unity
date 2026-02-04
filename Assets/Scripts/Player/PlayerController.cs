using System.Collections;
using UnityEngine;
using static UnityEngine.InputSystem.InputAction;

public class PlayerController : MovementController
{
    [SerializeField] private Grid grid;
    [SerializeField] private float timeToMove = 0.4f;
    [SerializeField] private float timeToTurn = 0.1f;

    private bool isButtonDown;
    private Vector3Int input;

    [SerializeField] private SpriteRenderer bubble;

    private string lastAction = "";
    private string state = "Neutral";

    protected override void SetSortingOrder(int layer)
    {
        base.SetSortingOrder(layer);
        bubble.sortingOrder = spriteRenderer.sortingOrder + 3;
    }

    private void Update()
    {
        switch (state)
        {
            case "Pathing":
                if (isButtonDown)
                {
                    if (!isBusy && lastAction != "Pathing")
                    {
                        lastAction = "Pathing";
                        Pathing();
                    }
                    else if (!isBusy)
                    {
                        lastAction = "Moving";
                        StartCoroutine(MovePlayer(false));
                    }
                }
                else
                {
                    if (!isBusy && lastAction != "Pathing")
                    {
                        lastAction = "Pathing";
                        Pathing();
                    }
                    else if (!isBusy)
                    {
                        animator.PlayIdleAnimation(GetDirection(LastDirection));
                    }
                }

                break;
            case "Terraforming":
                if (isButtonDown)
                {
                    if (!isBusy && lastAction != "Terraforming")
                    {
                        lastAction = "Terraforming";
                        Terraform();
                    }
                    else if (!isBusy)
                    {
                        lastAction = "Moving";
                        StartCoroutine(MovePlayer(false));
                    }
                }
                else
                {
                    if (!isBusy && lastAction != "Terraforming")
                    {
                        lastAction = "Terraforming";
                        Terraform();
                    }
                    else if (!isBusy)
                    {
                        animator.PlayIdleAnimation(GetDirection(LastDirection));
                    }
                }

                break;
            case "Waterscaping":
                if (isButtonDown)
                {
                    if (!isBusy && lastAction != "Waterscaping")
                    {
                        lastAction = "Waterscaping";
                        Waterscape();
                    }
                    else if (!isBusy)
                    {
                        lastAction = "Moving";
                        StartCoroutine(MovePlayer(false));
                    }
                }
                else
                {
                    if (!isBusy && lastAction != "Waterscaping")
                    {
                        lastAction = "Waterscaping";
                        Waterscape();
                    }
                    else if (!isBusy)
                    {
                        animator.PlayIdleAnimation(GetDirection(LastDirection));
                    }
                }

                break;
            default:
                if (!isBusy && isButtonDown)
                {
                    StartCoroutine(MovePlayer(true));
                }
                else if (!isBusy)
                {
                    animator.PlayIdleAnimation(GetDirection(LastDirection));
                }

                break;
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

    private IEnumerator MovePlayer(bool canTurn)
    {
        Vector3Int currInput = input;

        isBusy = true;

        string dir = GetDirection(currInput);
        bool hasMoved = false;

        if (currInput != LastDirection) //turn first
        {
            //https://discussions.unity.com/t/vector2-angle-how-do-i-get-if-its-cw-or-ccw/101180/5
            bool clockwise = Mathf.Sign(LastDirection.x * currInput.y - LastDirection.y * currInput.x) <= 0;
            int nrOfTurns = Mathf.RoundToInt(Vector3.Angle(currInput, LastDirection) / 45f);

            Debug.Log(nrOfTurns);

            if (!canTurn)
            {
                if (nrOfTurns != 4)
                {
                    isBusy = false;
                    lastAction = state;
                    animator.PlayIdleAnimation(GetDirection(LastDirection));

                    yield break;
                }
                else
                {
                    dir = GetDirection(LastDirection);
                }
            }
            else
            {
                string[] turns = GetTurns(GetDirection(LastDirection), nrOfTurns, clockwise);

                if (currInput.x != 0 || currInput.y != 0)
                {
                    LastDirection = currInput;
                }

                for (int i = 0; i < nrOfTurns; i++)
                {
                    animator.PlayTurnAnimation(turns[i]);
                    yield return new WaitForSeconds(timeToTurn);
                }

                hasMoved = true;
            }
        }
        
        if (!hasMoved)
        {
            float elapsedTime = 0f;

            Vector3Int gridPosition = grid.WorldToCell(transform.position);

            startPosition = transform.position;
            targetPosition = grid.CellToWorld(gridPosition + currInput);

            animator.PlayWalkAnimation(dir);

            float moveTime = timeToMove * (targetPosition - startPosition).magnitude;

            int targetLayer = worldManager.GetPositionLevel(targetPosition, layer, currInput);

            if (targetLayer > 0)
            {
                if (currInput.x != 0 && currInput.y != 0) //diagonal movement
                {
                    Vector3 checkPosition = grid.CellToWorld(gridPosition + new Vector3Int(currInput.x, 0, 0));

                    if (worldManager.GetPositionLevel(checkPosition, layer, Vector3Int.zero) == layer)
                    {
                        checkPosition = grid.CellToWorld(gridPosition + new Vector3Int(0, currInput.y, 0));

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

            if (canTurn)
            {
                if (currInput.x != 0 || currInput.y != 0)
                {
                    LastDirection = currInput;
                }
            }
        }

        yield return null;

        isBusy = false;
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
        isBusy = true;

        bool wasSuccess;
        if (LastDirection.x != 0 && LastDirection.y != 0) //can't be on diagonal
        {
            wasSuccess = false;
        }
        else
        {
            wasSuccess = worldManager.Pathing(transform.position + new Vector3(LastDirection.x * grid.cellSize.x, LastDirection.y * grid.cellSize.y, 0), layer);
        }

        isBusy = false;

        if (!wasSuccess)
        {
            EnableBubble();
        }
    }

    public void Terraform()
    {
        isBusy = true;

        bool wasSuccess;
        if (LastDirection.x != 0 && LastDirection.y != 0) //can't be on diagonal
        {
            wasSuccess = false;
        }
        else
        {
            wasSuccess = worldManager.Terraform(transform.position + new Vector3(LastDirection.x * grid.cellSize.x, LastDirection.y * grid.cellSize.y, 0), layer);
        }

        isBusy = false;

        if (!wasSuccess)
        {
            EnableBubble();
        }
    }

    public void Waterscape()
    {
        isBusy = true;

        bool wasSuccess;
        if (LastDirection.x != 0 && LastDirection.y != 0) //can't be on diagonal
        {
            wasSuccess = false;
        }
        else
        {
            wasSuccess = worldManager.Waterscape(transform.position + new Vector3(LastDirection.x * grid.cellSize.x, LastDirection.y * grid.cellSize.y, 0), layer);
        }

        isBusy = false;

        if (!wasSuccess)
        {
            EnableBubble();
        }
    }

    public void ChangeState(string state)
    {
        this.state = state;
        lastAction = "";
    }

    public void PlaceRamp()
    {
        bool wasSuccess;
        if (LastDirection.x != 0) //not facing right direction
        {
            wasSuccess = false;
        }
        else if (isBusy)
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
        else if (isBusy)
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
        else if (isBusy)
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
        else if (isBusy)
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
        else if (isBusy)
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
        else if (isBusy)
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