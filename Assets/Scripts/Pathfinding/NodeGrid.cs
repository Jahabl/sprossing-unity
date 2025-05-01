using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class NodeGrid : MonoBehaviour
{
    [SerializeField] private Grid grid;

    public Node[,] nodes;
    private Vector2Int gridSize;
    private Vector3 bottomLeft;
    private Tilemap[] tilemaps;

    public int MaxSize
    {
        get
        {
            return gridSize.x * gridSize.y;
        }
    }

    private void Awake()
    {
        gridSize = Vector2Int.zero;
        tilemaps = new Tilemap[grid.transform.childCount];

        for (int i = 0; i < grid.transform.childCount; i++)
        {
            tilemaps[i] = grid.transform.GetChild(i).GetComponent<Tilemap>();

            if (tilemaps[i].cellBounds.size.x > gridSize.x)
            {
                gridSize.x = tilemaps[i].cellBounds.size.x;
            }

            if (tilemaps[i].cellBounds.size.y > gridSize.y)
            {
                gridSize.y = tilemaps[i].cellBounds.size.y;
            }
        }

        bottomLeft = new Vector3(-gridSize.x / 2f * grid.cellSize.x + grid.cellSize.x, -gridSize.y / 2f * grid.cellSize.y, 0f);

        GenerateGrid();
    }

    private void GenerateGrid()
    {
        nodes = new Node[gridSize.x, gridSize.y];
        string temp = "";

        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                for (int i = 0; i < tilemaps.Length; i += 2) //bottom to top
                {
                    SeasonalRuleTile tile = tilemaps[i].GetTile<SeasonalRuleTile>(new Vector3Int(x - gridSize.x / 2, y - gridSize.y / 2, 0));
                    if (tile != null)
                    {
                        if (nodes[x,y] == null)
                        {
                            nodes[x,y] = new Node(bottomLeft + new Vector3(x * grid.cellSize.x, y * grid.cellSize.y, 0), x, y);
                        }

                        if (nodes[x,y].gridID > 0)
                        {
                            nodes[x,y].gridID *= tile.tileType == TileType.Grass ? i / 2 * 3 + 7 : 0;
                        }
                        else
                        {
                            nodes[x,y].gridID = tile.tileType == TileType.Grass ? i / 2 * 3 + 7 : -i;
                        }

                        tile = tilemaps[i + 1].GetTile<SeasonalRuleTile>(new Vector3Int(x - gridSize.x / 2, y - gridSize.y / 2, 0));
                        if (tile != null)
                        {
                            nodes[x, y].movementPenalty = tile.tileType == TileType.Path ? 0 : 5;

                            if (tile.tileType == TileType.Ramp)
                            {
                                if (nodes[x, y].gridID <= 0) //top of ramp
                                {
                                    nodes[x, y].gridID = nodes[x, y - 1].gridID + 1;
                                }
                                else //bottom of ramp
                                {
                                    nodes[x, y].gridID++;
                                }
                            }
                        }
                    }
                }
                temp += nodes[x, y].gridID + "-";
            }
            temp += "\n";
        }

        Debug.Log(temp);
    }

    public Node GetNodeFromWorldPosition(Vector3 worldPosition)
    {
        Vector3Int cellPosition = tilemaps[0].WorldToCell(worldPosition);
        int checkX = cellPosition.x + gridSize.x / 2;
        int checkY = cellPosition.y + gridSize.y / 2;

        if (checkX >= 0 && checkX < gridSize.x && checkY >= 0 && checkY < gridSize.y)
        {
            return nodes[checkX,checkY];
        }

        return null;
    }

    public List<Node> GetNeighbors(Node centerNode, int layer)
    {
        List<Node> neighbors = new List<Node>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x != 0 || y != 0)
                {
                    int checkX = centerNode.gridX + x;
                    int checkY = centerNode.gridY + y;

                    if (checkX >= 0 && checkX < gridSize.x && checkY >= 0 && checkY < gridSize.y)
                    {
                        if ((x + y)%2 == 0) //is diagonal
                        {
                            if (!nodes[checkX, centerNode.gridY].IsWalkable(layer) || !nodes[centerNode.gridX, checkY].IsWalkable(layer))
                            {
                                continue;
                            }
                        }

                        neighbors.Add(nodes[checkX, checkY]);
                    }
                }
            }
        }

        return neighbors;
    }

    public void UpdateNodeInGrid(Vector3 center, int layer, bool isWalkable)
    {
        //Node centerNode = GetNodeFromWorldPosition(center);
        //nodes[centerNode.gridX, centerNode.gridY].isWalkable[layer] = isWalkable;
    }
}