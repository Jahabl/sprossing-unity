using UnityEngine;

public class MovementController : MonoBehaviour
{
    [SerializeField] protected WorldManager worldManager;

    protected Vector3 startPosition;
    protected Vector3 targetPosition;
    public Vector3Int LastDirection { get; protected set; }

    [SerializeField] protected AnimationController animator;
    [SerializeField] protected SpriteRenderer spriteRenderer;
    public int layer;

    protected bool isBusy;

    private bool wasInit;

    private void Start()
    {
        if (wasInit)
        {
            return;
        }

        LastDirection = new Vector3Int(0, -1, 0);
        animator.PlayIdleAnimation(GetDirection(LastDirection));

        layer = spriteRenderer.sortingOrder + 5;
    }

    //can run before Start() is called
    public void Initialize(int[] direction, int layer)
    {
        LastDirection = new Vector3Int(direction[0], direction[1], 0);
        animator.PlayIdleAnimation(GetDirection(LastDirection));

        SetSortingOrder(layer);
        this.layer = layer;

        wasInit = true;
    }

    protected virtual void SetSortingOrder(int layer)
    {
        if ((layer - 1) % 3 != 0) //ramp
        {
            spriteRenderer.sortingOrder = layer;
        }
        else
        {
            spriteRenderer.sortingOrder = layer - 5;
        }
    }

    protected string GetDirection(Vector3Int direction)
    {
        if (direction.x == 1f)
        {
            if (direction.y == 1f)
            {
                return "NE";
            }
            else if (direction.y == -1f)
            {
                return "SE";
            }
            else
            {
                return "E";
            }
        }
        else if (direction.x == -1f)
        {
            if (direction.y == 1f)
            {
                return "NW";
            }
            else if (direction.y == -1f)
            {
                return "SW";
            }
            else
            {
                return "W";
            }
        }
        else
        {
            if (direction.y == 1f)
            {
                return "N";
            }
            else if (direction.y == -1f)
            {
                return "S";
            }
        }

        return "S";
    }

    protected string[] GetTurns(string start, int nrOfTurns, bool clockwise)
    {
        string[] turns = new string[nrOfTurns];

        string from = start;
        string to = "";

        for (int i = 0; i < nrOfTurns; i++)
        {
            switch (from)
            {
                case "S":
                    if (clockwise)
                    {
                        to = "SW";
                    }
                    else
                    {
                        to = "SE";
                    }

                    break;
                case "SW":
                    if (clockwise)
                    {
                        to = "W";
                    }
                    else
                    {
                        to = "S";
                    }

                    break;
                case "W":
                    if (clockwise)
                    {
                        to = "NW";
                    }
                    else
                    {
                        to = "SW";
                    }

                    break;
                case "NW":
                    if (clockwise)
                    {
                        to = "N";
                    }
                    else
                    {
                        to = "W";
                    }

                    break;
                case "N":
                    if (clockwise)
                    {
                        to = "NE";
                    }
                    else
                    {
                        to = "NW";
                    }

                    break;
                case "NE":
                    if (clockwise)
                    {
                        to = "E";
                    }
                    else
                    {
                        to = "N";
                    }

                    break;
                case "E":
                    if (clockwise)
                    {
                        to = "SE";
                    }
                    else
                    {
                        to = "NE";
                    }

                    break;
                case "SE":
                    if (clockwise)
                    {
                        to = "S";
                    }
                    else
                    {
                        to = "E";
                    }

                    break;
            }

            turns[i] = clockwise ? from + to : to + from;
            from = to;
        }

        return turns;
    }
}