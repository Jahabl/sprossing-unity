using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WorldManager : MonoBehaviour
{
    [SerializeField] private PlayerController player;
    [SerializeField] private NodeGrid nodeGrid;

    [SerializeField] private SeasonalRuleTile[] allTiles;

    private Tilemap[] tilemaps;

    private void Start()
    {
        tilemaps = new Tilemap[transform.childCount];

        for (int i = 0; i < transform.childCount; i++)
        {
            tilemaps[i] = transform.GetChild(i).GetComponent<Tilemap>();
        }
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.N))
        {
            Seasons newSeason = allTiles[0].season;

            switch (newSeason)
            {
                case Seasons.Spring:
                    newSeason = Seasons.Summer;
                    break;
                case Seasons.Summer:
                    newSeason = Seasons.Autumn;
                    break;
                case Seasons.Autumn:
                    newSeason = Seasons.Winter;
                    break;
                case Seasons.Winter:
                    newSeason = Seasons.Spring;
                    break;
            }

            foreach (SeasonalRuleTile tile in allTiles)
            {
                tile.season = newSeason;
            }

            foreach (Tilemap map in tilemaps)
            {
                map.RefreshAllTiles();
            }
        }
    }

    public void SaveMap()
    {
        SaveData saveData = new SaveData(player);

        for (int i = 0; i < tilemaps.Length; i++)
        {
            List<SavedTile> tiles = new List<SavedTile>();
            BoundsInt bounds = tilemaps[i].cellBounds;

            for (int x = bounds.xMin; x < bounds.xMax; x++)
            {
                for (int y = bounds.yMin; y < bounds.yMax; y++)
                {
                    SavedTile savedTile = new SavedTile();
                    SeasonalRuleTile ruleTile = tilemaps[i].GetTile<SeasonalRuleTile>(new Vector3Int(x, y, 0));
                    
                    if (ruleTile != null)
                    {
                        savedTile.position = new Vector3Int(x, y, 0);
                        savedTile.tileType = ruleTile.tileType;

                        tiles.Add(savedTile);
                    }
                }
            }

            switch (i)
            {
                case 0:
                    saveData.layer0 = tiles;
                    break;
                case 1:
                    saveData.layer1 = tiles;
                    break;
                case 2:
                    saveData.layer2 = tiles;
                    break;
                case 3:
                    saveData.layer3 = tiles;
                    break;
                case 4:
                    saveData.layer4 = tiles;
                    break;
                case 5:
                    saveData.layer5 = tiles;
                    break;
            }
        }

        SaveManager.SaveData(saveData);
    }

    public void ClearMap()
    {
        foreach (Tilemap tilemap in tilemaps)
        {
            tilemap.ClearAllTiles();
        }
    }

    public void LoadMap()
    {
        SaveData saveData = SaveManager.LoadData();

        player.transform.position = new Vector3(saveData.playerPosition[0], saveData.playerPosition[1], 0f);
        player.SetLastDirection(saveData.playerDirection);
        player.SetLayer(saveData.playerDirection[2]);

        for (int i = 0; i < tilemaps.Length; i++)
        {
            List<SavedTile> savedTiles = new List<SavedTile>();

            switch (i)
            {
                case 0:
                    savedTiles = saveData.layer0;

                    break;
                case 1:
                    savedTiles = saveData.layer1;

                    break;
                case 2:
                    savedTiles = saveData.layer2;

                    break;
                case 3:
                    savedTiles = saveData.layer3;

                    break;
                case 4:
                    savedTiles = saveData.layer4;

                    break;
                case 5:
                    savedTiles = saveData.layer5;

                    break;
            }

            for (int j = 0; j < savedTiles.Count; j++)
            {
                tilemaps[i].SetTile(savedTiles[j].position, allTiles[(int) savedTiles[j].tileType]);
            }
        }

        nodeGrid.GenerateGrid();
    }

    public int GetPositionLevel(Vector3 position, int layer, Vector3Int direction)
    {
        Node targetNode = nodeGrid.GetNodeFromWorldPosition(position);
        if (targetNode == null)
        {
            return 0;
        }

        return targetNode.GetLevel(layer, direction);
    }

    public void Pathing(Vector3 position, int layer)
    {
        if ((layer - 1) % 3 != 0) //on ramp
            return;

        int tileLayer = layer - 6;

        Vector3Int tilePosition = tilemaps[0].WorldToCell(position);

        SeasonalRuleTile tile = tilemaps[tileLayer].GetTile<SeasonalRuleTile>(tilePosition);

        if (tile == null)
        {
            tileLayer--;
            tile = tilemaps[tileLayer].GetTile<SeasonalRuleTile>(tilePosition);
        }

        if (tile == null)
            return;

        switch (tile.tileType)
        {
            case TileType.Grass: //add path
                tilemaps[tileLayer + 1].SetTile(tilePosition, allTiles[3]);

                nodeGrid.UpdateNodeInGrid(position, tilePosition);
                break;
            case TileType.Path: //remove path
                tilemaps[tileLayer].SetTile(tilePosition, null);

                nodeGrid.UpdateNodeInGrid(position, tilePosition);
                break;
        }
    }

    public void Terraform(Vector3 position, int layer)
    {
        if ((layer - 1) % 3 != 0) //on ramp
            return;

        int tileLayer = layer - 7;

        Vector3Int tilePosition = tilemaps[0].WorldToCell(position);

        SeasonalRuleTile tile = tilemaps[tileLayer + 1].GetTile<SeasonalRuleTile>(tilePosition);

        if (tile == null)
        {
            tile = tilemaps[tileLayer].GetTile<SeasonalRuleTile>(tilePosition);
        }

        if (tile != null)
        {
            switch (tile.tileType)
            {
                case TileType.Path: //add cliff
                case TileType.Grass:
                    if (layer - 4 >= tilemaps.Length)
                        return;
                    
                    tilemaps[tileLayer + 1].SetTile(tilePosition, allTiles[1]);
                    tilemaps[tileLayer + 3].SetTile(tilePosition + Vector3Int.up, allTiles[0]);

                    nodeGrid.UpdateNodeInGrid(position + new Vector3(0f, GetComponent<Grid>().cellSize.y, 0f), tilePosition + Vector3Int.up);
                    nodeGrid.UpdateNodeInGrid(position, tilePosition);

                    break;
                case TileType.Cliff:
                    tile = tilemaps[tileLayer + 3].GetTile<SeasonalRuleTile>(tilePosition + Vector3Int.up);

                    if (tile != null && tile.tileType == TileType.Grass) //remove cliff
                    {
                        for (int x = -1; x <= 1; x++)
                        {
                            for (int y = -1; y <= 1; y++)
                            {
                                if (y == 0 || x == 0)
                                {
                                    tile = tilemaps[tileLayer + 4].GetTile<SeasonalRuleTile>(tilePosition + Vector3Int.up + new Vector3Int(x, y, 0));
                                    if (tile != null && (tile.tileType == TileType.Water || tile.tileType == TileType.Cliff))
                                    {
                                        return;
                                    }
                                }
                            }
                        }

                        tilemaps[tileLayer + 3].SetTile(tilePosition + Vector3Int.up, null);
                        tilemaps[tileLayer + 4].SetTile(tilePosition + Vector3Int.up, null); //remove overlay
                        tilemaps[tileLayer + 1].SetTile(tilePosition, null);

                        nodeGrid.UpdateNodeInGrid(position + new Vector3(0f, GetComponent<Grid>().cellSize.y, 0f), tilePosition + Vector3Int.up);
                        nodeGrid.UpdateNodeInGrid(position, tilePosition);
                    }

                    break;
            }
        }
        else if (layer - 7 > 2) //add cliff
        {
            tileLayer -= 3;
            tile = tilemaps[tileLayer + 1].GetTile<SeasonalRuleTile>(tilePosition - Vector3Int.up);

            if (tile == null)
            {
                tile = tilemaps[tileLayer].GetTile<SeasonalRuleTile>(tilePosition);
            }

            if (tile != null && (tile.tileType == TileType.Grass || tile.tileType == TileType.Ramp))
            {
                tilemaps[tileLayer + 1].SetTile(tilePosition - Vector3Int.up, allTiles[1]);
                tilemaps[tileLayer + 3].SetTile(tilePosition, allTiles[0]);

                nodeGrid.UpdateNodeInGrid(position - new Vector3(0f, GetComponent<Grid>().cellSize.y, 0f), tilePosition - Vector3Int.up);
                nodeGrid.UpdateNodeInGrid(position, tilePosition);
            }
        }
    }

    public void Waterscape(Vector3 position, int layer)
    {
        if ((layer - 1) % 3 != 0) //on ramp
            return;

        int tileLayer = layer - 7;

        Vector3Int tilePosition = tilemaps[0].WorldToCell(position);

        SeasonalRuleTile tile = tilemaps[tileLayer + 1].GetTile<SeasonalRuleTile>(tilePosition);

        if (tile == null)
        {
            tile = tilemaps[tileLayer].GetTile<SeasonalRuleTile>(tilePosition);
        }

        if (tile == null)
            return;

        switch (tile.tileType)
        {
            case TileType.Path:
            case TileType.Grass: //add water
                bool invalidNeighbor = false;

                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        if (y == 0 || x == 0)
                        {
                            SeasonalRuleTile tileA = tilemaps[tileLayer].GetTile<SeasonalRuleTile>(tilePosition + new Vector3Int(x, y, 0));
                            SeasonalRuleTile tileB = tilemaps[tileLayer + 1].GetTile<SeasonalRuleTile>(tilePosition + new Vector3Int(x, y, 0));
                            if (tileA == null || (tileB != null && tileB.tileType == TileType.Cliff))
                            {
                                if (y >= 0)
                                    return;
                                
                                invalidNeighbor = true;
                                break;
                            }
                        }
                    }
                }

                if (invalidNeighbor)
                {
                    SeasonalRuleTile tileA = tilemaps[tileLayer].GetTile<SeasonalRuleTile>(tilePosition + new Vector3Int(0, -1, 0));
                    SeasonalRuleTile tileB = tilemaps[tileLayer - 2].GetTile<SeasonalRuleTile>(tilePosition + new Vector3Int(0, -1, 0));
                    if (tileA != null || tileB == null || tileB.tileType != TileType.Cliff)
                        return;
                    
                    tileA = tilemaps[tileLayer].GetTile<SeasonalRuleTile>(tilePosition + new Vector3Int(0, -2, 0));
                    tileB = tilemaps[tileLayer - 3].GetTile<SeasonalRuleTile>(tilePosition + new Vector3Int(0, -2, 0));
                    if (tileA != null || tileB == null || tileB.tileType != TileType.Grass)
                        return;

                    tilemaps[tileLayer].SetTile(tilePosition, allTiles[1]);
                    tilemaps[tileLayer + 1].SetTile(tilePosition, allTiles[5]);
                    tilemaps[tileLayer - 2].SetTile(tilePosition + new Vector3Int(0, -1, 0), allTiles[5]);

                    nodeGrid.UpdateNodeInGrid(position - new Vector3(0f, GetComponent<Grid>().cellSize.y, 0f), tilePosition + new Vector3Int(0, -1, 0));
                }
                else
                {
                    tilemaps[tileLayer].SetTile(tilePosition, allTiles[1]);
                    tilemaps[tileLayer + 1].SetTile(tilePosition, allTiles[2]);
                }

                nodeGrid.UpdateNodeInGrid(position, tilePosition);
                break;
            case TileType.Water: //remove water
                tilemaps[tileLayer + 1].SetTile(tilePosition, null);
                tilemaps[tileLayer].SetTile(tilePosition, allTiles[0]);

                nodeGrid.UpdateNodeInGrid(position, tilePosition);
                break;
        }
    }

    public void PlaceRemoveRamp(Vector3 position, int layer)
    {
        if ((layer - 1) % 3 != 0) //on ramp
            return;

        int tileLayer = layer - 7;

        Vector3Int tilePosition = tilemaps[0].WorldToCell(position);

        SeasonalRuleTile tile = tilemaps[tileLayer + 1].GetTile<SeasonalRuleTile>(tilePosition);

        if (tile == null)
        {
            tile = tilemaps[tileLayer].GetTile<SeasonalRuleTile>(tilePosition);
        }

        if (tile != null)
        {
            switch (tile.tileType)
            {
                case TileType.Path: //place ramp
                case TileType.Grass:
                    tile = tilemaps[tileLayer + 1].GetTile<SeasonalRuleTile>(tilePosition + Vector3Int.up);
                    if (tile != null && tile.tileType == TileType.Cliff)
                    {
                        tilemaps[tileLayer + 1].SetTile(tilePosition, allTiles[4]);
                        tilemaps[tileLayer + 1].SetTile(tilePosition + Vector3Int.up, allTiles[4]);

                        nodeGrid.UpdateNodeInGrid(position, tilePosition);
                        nodeGrid.UpdateNodeInGrid(position + new Vector3(0f, GetComponent<Grid>().cellSize.y, 0f), tilePosition + Vector3Int.up);
                    }

                    break;
                case TileType.Cliff:
                    return;
                case TileType.Ramp: //remove ramp
                    tile = tilemaps[tileLayer + 1].GetTile<SeasonalRuleTile>(tilePosition + Vector3Int.up);
                    if (tile != null && tile.tileType == TileType.Ramp)
                    {
                        tilemaps[tileLayer + 1].SetTile(tilePosition, null);
                        tilemaps[tileLayer + 1].SetTile(tilePosition + Vector3Int.up, allTiles[1]);

                        nodeGrid.UpdateNodeInGrid(position, tilePosition);
                        nodeGrid.UpdateNodeInGrid(position + new Vector3(0f, GetComponent<Grid>().cellSize.y, 0f), tilePosition + Vector3Int.up);
                    }

                    break;
            }
        }
    }

    public void PlaceHouse(Vector3 position, int layer, GameObject house)
    {
        if ((layer - 1) % 3 != 0) //on ramp
            return;

        Vector3 cellSize = GetComponent<Grid>().cellSize;

        for (int x = -1; x <= 1; x++)
        {
            for (int y = 0; y <= 1; y++)
            {
                Node checkNode = nodeGrid.GetNodeFromWorldPosition(position + new Vector3(cellSize.x * x, cellSize.y * y, 0f));
                if (checkNode.gridID != layer)
                {
                    return;
                }
            }
        }

        GameObject newHouse = Instantiate(house, position, Quaternion.identity);
        newHouse.GetComponent<SpriteRenderer>().sortingOrder = layer - 5;
    }
}