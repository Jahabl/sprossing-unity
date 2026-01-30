using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class NodeGrid : MonoBehaviour
{
    [SerializeField] private Grid grid;

    public Node[,] nodes;
    public Vector2Int gridSize;
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
        tilemaps = grid.transform.GetComponentsInChildren<Tilemap>();
    }

    public void GenerateGrid(Vector2Int gridSize)
    {
        this.gridSize = gridSize;
        bottomLeft = new Vector3(-gridSize.x / 2f * grid.cellSize.x + grid.cellSize.x, -gridSize.y / 2f * grid.cellSize.y, 0f);

        nodes = new Node[gridSize.x, gridSize.y];
        string temp = "";

        for (int y = 0; y < gridSize.y; y++)
        {
            for (int x = 0; x < gridSize.x; x++)
            {
                for (int i = 0; i < tilemaps.Length; i += 3) //bottom to top
                {
                    SeasonalRuleTile tile = tilemaps[i].GetTile<SeasonalRuleTile>(new Vector3Int(x - gridSize.x / 2, y - gridSize.y / 2, 0));
                    if (tile != null)
                    {
                        if (nodes[x, y] == null)
                        {
                            nodes[x, y] = new Node(bottomLeft + new Vector3(x * grid.cellSize.x, y * grid.cellSize.y, 0), x, y);
                        }

                        if (tile.tileType == TileType.Grass)
                        {
                            nodes[x, y].movementPenalty = 5;

                            tile = tilemaps[i + 2].GetTile<SeasonalRuleTile>(new Vector3Int(x - gridSize.x / 2, y - gridSize.y / 2, 0));
                            if (tile != null)
                            {
                                if (nodes[x, y].gridID <= 0)
                                {
                                    nodes[x, y].gridID = -i;
                                }
                            }
                            else
                            {
                                tile = tilemaps[i + 1].GetTile<SeasonalRuleTile>(new Vector3Int(x - gridSize.x / 2, y - gridSize.y / 2, 0));
                                if (tile != null)
                                {
                                    switch (tile.tileType)
                                    {
                                        case TileType.Cliff:
                                        case TileType.Waterfall:
                                            if (nodes[x, y].gridID <= 0)
                                            {
                                                nodes[x, y].gridID = -i;
                                            }

                                            break;
                                        case TileType.Ramp:
                                            nodes[x, y].gridID = nodes[x, y - 1].gridID + 1;

                                            break;
                                        case TileType.Path:
                                            nodes[x, y].movementPenalty = 0;
                                            goto default;
                                        default:
                                            if (nodes[x, y].gridID > 0)
                                            {
                                                nodes[x, y].gridID *= i + 7;
                                            }
                                            else
                                            {
                                                nodes[x, y].gridID = i + 7;
                                            }

                                            break;
                                    }
                                }
                                else if (nodes[x, y].gridID > 0)
                                {
                                    nodes[x, y].gridID *= i + 7;
                                }
                                else
                                {
                                    nodes[x, y].gridID = i + 7;
                                }
                            }
                        }
                        else
                        {
                            nodes[x, y].gridID = -i;
                        }
                    }
                }

                if (nodes[x, y] != null)
                {
                    temp += nodes[x, y].gridID + "|";
                }
                else
                {
                    temp += "X|";
                }
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
            return nodes[checkX, checkY];
        }

        return null;
    }

    public List<Node> GetNeighbors(Node centerNode)
    {
        List<Node> neighbors = new List<Node>();

        for (int y = -1; y <= 1; y++)
        {
            for (int x = -1; x <= 1; x++)
            {
                if (x != 0 || y != 0)
                {
                    int checkX = centerNode.gridX + x;
                    int checkY = centerNode.gridY + y;

                    if (checkX >= 0 && checkX < gridSize.x && checkY >= 0 && checkY < gridSize.y)
                    {
                        if ((x + y) % 2 == 0) //check diagonal
                        {
                            if (nodes[checkX, centerNode.gridY].GetLevel(centerNode.layer, new Vector3Int(x, 0, 0)) != centerNode.layer || nodes[centerNode.gridX, checkY].GetLevel(centerNode.layer, new Vector3Int(0, y, 0)) != centerNode.layer)
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

    public void UpdateNodeInGrid(Vector3 worldPosition, Vector3Int tilePosition)
    {
        Node updateNode = GetNodeFromWorldPosition(worldPosition);

        if (updateNode == null)
        {
            return;
        }

        nodes[updateNode.gridX, updateNode.gridY] = null;

        for (int i = 0; i < tilemaps.Length; i += 3) //bottom to top
        {
            SeasonalRuleTile tile = tilemaps[i].GetTile<SeasonalRuleTile>(tilePosition);
            if (tile != null)
            {
                if (nodes[updateNode.gridX, updateNode.gridY] == null)
                {
                    nodes[updateNode.gridX, updateNode.gridY] = new Node(worldPosition, updateNode.gridX, updateNode.gridY);
                }

                if (tile.tileType == TileType.Grass)
                {
                    nodes[updateNode.gridX, updateNode.gridY].movementPenalty = 5;

                    tile = tilemaps[i + 2].GetTile<SeasonalRuleTile>(new Vector3Int(updateNode.gridX - gridSize.x / 2, updateNode.gridY - gridSize.y / 2, 0));
                    if (tile != null)
                    {
                        if (nodes[updateNode.gridX, updateNode.gridY].gridID <= 0)
                        {
                            nodes[updateNode.gridX, updateNode.gridY].gridID = -i;
                        }
                    }
                    else
                    {
                        tile = tilemaps[i + 1].GetTile<SeasonalRuleTile>(new Vector3Int(updateNode.gridX - gridSize.x / 2, updateNode.gridY - gridSize.y / 2, 0));
                        if (tile != null)
                        {
                            switch (tile.tileType)
                            {
                                case TileType.Cliff:
                                case TileType.Waterfall:
                                    if (nodes[updateNode.gridX, updateNode.gridY].gridID <= 0)
                                    {
                                        nodes[updateNode.gridX, updateNode.gridY].gridID = -i;
                                    }

                                    break;
                                case TileType.Ramp:
                                    nodes[updateNode.gridX, updateNode.gridY].gridID = nodes[updateNode.gridX, updateNode.gridY - 1].gridID + 1;

                                    break;
                                case TileType.Path:
                                    nodes[updateNode.gridX, updateNode.gridY].movementPenalty = 0;
                                    goto default;
                                default:
                                    if (nodes[updateNode.gridX, updateNode.gridY].gridID > 0)
                                    {
                                        nodes[updateNode.gridX, updateNode.gridY].gridID *= i + 7;
                                    }
                                    else
                                    {
                                        nodes[updateNode.gridX, updateNode.gridY].gridID = i + 7;
                                    }

                                    break;
                            }
                        }
                        else if (nodes[updateNode.gridX, updateNode.gridY].gridID > 0)
                        {
                            nodes[updateNode.gridX, updateNode.gridY].gridID *= i + 7;
                        }
                        else
                        {
                            nodes[updateNode.gridX, updateNode.gridY].gridID = i + 7;
                        }
                    }
                }
            }
        }
    }

    public bool IsOnBorder(Vector3Int tilePosition)
    {
        if (tilePosition.y == gridSize.y / -2)
        {
            return true;
        }

        if (tilePosition.y == gridSize.y / 2 - 1)
        {
            return true;
        }
        
        if (tilePosition.x == gridSize.x / -2)
        {
            return true;
        }

        if (tilePosition.x == gridSize.x / 2 - 1)
        {
            return true;
        }

        return false;
    }
}