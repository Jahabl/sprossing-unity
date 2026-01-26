using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WorldManager : MonoBehaviour
{
    [SerializeField] private PlayerController player;
    [SerializeField] private NodeGrid nodeGrid;

    [SerializeField] private SeasonalRuleTile[] allTiles;

    private Tilemap[] tilemaps;

    [SerializeField] private Transform objectParent;

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
                case 6:
                    saveData.layer6 = tiles;
                    break;
                case 7:
                    saveData.layer7 = tiles;
                    break;
                case 8:
                    saveData.layer8 = tiles;
                    break;
            }
        }

        foreach (Transform child in objectParent)
        {
            SpriteRenderer sprite = child.GetComponent<SpriteRenderer>();
            saveData.objects.Add(new SavedObject(child.name, child.position, sprite.sortingOrder));
        }

        SaveManager.SaveData(saveData);
    }

    public void ClearMap()
    {
        foreach (Tilemap tilemap in tilemaps)
        {
            tilemap.ClearAllTiles();
        }

        foreach (Transform child in objectParent)
        {
            Destroy(child.gameObject);
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
                case 6:
                    savedTiles = saveData.layer6;
                    break;
                case 7:
                    savedTiles = saveData.layer7;
                    break;
                case 8:
                    savedTiles = saveData.layer8;
                    break;
            }

            for (int j = 0; j < savedTiles.Count; j++)
            {
                tilemaps[i].SetTile(savedTiles[j].position, allTiles[(int) savedTiles[j].tileType]);
            }
        }

        foreach (SavedObject child in saveData.objects)
        {
            GameObject newHouse = Resources.Load<GameObject>($"{child.prefabName}");
            newHouse = Instantiate(newHouse, new Vector3(child.position[0], child.position[1], 0f), Quaternion.identity, objectParent);
            newHouse.GetComponent<SpriteRenderer>().sortingOrder = child.layer;
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

        int tileLayer = layer - 7;

        Vector3Int tilePosition = tilemaps[0].WorldToCell(position);

        if (tilemaps[tileLayer + 2].GetTile<SeasonalRuleTile>(tilePosition) != null)
            return;

        SeasonalRuleTile tile = tilemaps[tileLayer + 1].GetTile<SeasonalRuleTile>(tilePosition);

        if (tile == null)
        {
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
                tilemaps[tileLayer + 1].SetTile(tilePosition, null);

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

        if (tilemaps[tileLayer + 2].GetTile<SeasonalRuleTile>(tilePosition) != null)
            return;

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

                    tile = tilemaps[tileLayer + 1].GetTile<SeasonalRuleTile>(tilePosition + Vector3Int.up);
                    if (tile != null && tile.tileType == TileType.Waterfall)
                        return;
                    
                    tilemaps[tileLayer + 1].SetTile(tilePosition, allTiles[1]);
                    tilemaps[tileLayer + 3].SetTile(tilePosition + Vector3Int.up, allTiles[0]);

                    nodeGrid.UpdateNodeInGrid(position + new Vector3(0f, GetComponent<Grid>().cellSize.y, 0f), tilePosition + Vector3Int.up);
                    nodeGrid.UpdateNodeInGrid(position, tilePosition);

                    break;
                case TileType.Cliff:
                    tile = tilemaps[tileLayer + 3].GetTile<SeasonalRuleTile>(tilePosition + Vector3Int.up);

                    if (tilemaps[tileLayer + 5].GetTile<SeasonalRuleTile>(tilePosition + Vector3Int.up) != null)
                        return;

                    if (tile != null && tile.tileType == TileType.Grass) //remove cliff
                    {
                        for (int x = -1; x <= 1; x++)
                        {
                            for (int y = -1; y <= 1; y++)
                            {
                                if (y == 0 || x == 0)
                                {
                                    tile = tilemaps[tileLayer + 4].GetTile<SeasonalRuleTile>(tilePosition + Vector3Int.up + new Vector3Int(x, y, 0));
                                    if (tile != null && (tile.tileType == TileType.Water || tile.tileType == TileType.Cliff || tile.tileType == TileType.Waterfall))
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

            if (tilemaps[tileLayer + 2].GetTile<SeasonalRuleTile>(tilePosition + Vector3Int.down) != null)
                return;

            tile = tilemaps[tileLayer + 1].GetTile<SeasonalRuleTile>(tilePosition + Vector3Int.down);

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

        if (tile == null || tilemaps[tileLayer + 2].GetTile<SeasonalRuleTile>(tilePosition) != null)
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
            case TileType.Waterfall: //remove water
                if (tileLayer > 2)
                {
                    tile = tilemaps[tileLayer - 2].GetTile<SeasonalRuleTile>(tilePosition + Vector3Int.down);

                    if (tile != null && tile.tileType == TileType.Waterfall)
                    {
                        tilemaps[tileLayer + 1].SetTile(tilePosition, null);
                        tilemaps[tileLayer].SetTile(tilePosition, allTiles[0]);
                        tilemaps[tileLayer - 2].SetTile(tilePosition + Vector3Int.down, allTiles[1]);

                        nodeGrid.UpdateNodeInGrid(position, tilePosition);
                    }
                    else
                    {
                        tile = tilemaps[tileLayer + 3].GetTile<SeasonalRuleTile>(tilePosition + Vector3Int.up);
                        if (tile != null && tile.tileType == TileType.Waterfall)
                        {
                            tilemaps[tileLayer + 1].SetTile(tilePosition, null);
                            tilemaps[tileLayer].SetTile(tilePosition, allTiles[0]);
                            tilemaps[tileLayer + 3].SetTile(tilePosition + Vector3Int.up, allTiles[1]);

                            nodeGrid.UpdateNodeInGrid(position + new Vector3(0f, GetComponent<Grid>().cellSize.y, 0f), tilePosition + Vector3Int.up);
                        }
                    }
                }
                else if (tileLayer < 9)
                {
                    tile = tilemaps[tileLayer + 4].GetTile<SeasonalRuleTile>(tilePosition + Vector3Int.up);
                    if (tile != null && tile.tileType == TileType.Waterfall)
                    {
                        tilemaps[tileLayer + 1].SetTile(tilePosition, allTiles[1]);
                        tilemaps[tileLayer + 3].SetTile(tilePosition + Vector3Int.up, allTiles[0]);
                        tilemaps[tileLayer + 4].SetTile(tilePosition + Vector3Int.up, null);

                        nodeGrid.UpdateNodeInGrid(position + new Vector3(0f, GetComponent<Grid>().cellSize.y, 0f), tilePosition + Vector3Int.up);
                    }
                }

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

        if (tilemaps[tileLayer + 2].GetTile<SeasonalRuleTile>(tilePosition) != null)
            return;

        if (tile != null)
        {
            switch (tile.tileType)
            {
                case TileType.Path: //place ramp
                case TileType.Grass:
                    if (tilemaps[tileLayer + 2].GetTile<SeasonalRuleTile>(tilePosition + Vector3Int.up) != null)
                        return;

                    tile = tilemaps[tileLayer + 1].GetTile<SeasonalRuleTile>(tilePosition + Vector3Int.up);
                    if (tile != null && tile.tileType == TileType.Cliff)
                    {
                        tilemaps[tileLayer + 1].SetTile(tilePosition, allTiles[4]);
                        tilemaps[tileLayer + 1].SetTile(tilePosition + Vector3Int.up, allTiles[4]);

                        nodeGrid.UpdateNodeInGrid(position, tilePosition);
                        nodeGrid.UpdateNodeInGrid(position + new Vector3(0f, GetComponent<Grid>().cellSize.y, 0f), tilePosition + Vector3Int.up);
                    }

                    break;
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
        else if (tileLayer > 0)
        {
            tile = tilemaps[tileLayer - 2].GetTile<SeasonalRuleTile>(tilePosition);
            if (tile != null && tile.tileType == TileType.Cliff)
            {
                tile = tilemaps[tileLayer - 3].GetTile<SeasonalRuleTile>(tilePosition + Vector3Int.down);
                if (tile != null && tile.tileType == TileType.Grass)
                {
                    tilemaps[tileLayer - 2].SetTile(tilePosition, allTiles[4]);
                    tilemaps[tileLayer - 2].SetTile(tilePosition + Vector3Int.down, allTiles[4]);

                    nodeGrid.UpdateNodeInGrid(position - new Vector3(0f, GetComponent<Grid>().cellSize.y, 0f), tilePosition + Vector3Int.down);
                    nodeGrid.UpdateNodeInGrid(position, tilePosition);
                }
            }
        }
    }

    public void PlaceHouse(Vector3 position, int layer)
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

        Vector3Int tilePosition = tilemaps[0].WorldToCell(position);

        if (tilemaps[layer - 5].GetTile<SeasonalRuleTile>(tilePosition) != null)
            return;

        GameObject house = Resources.Load<GameObject>("House");
        house = Instantiate(house, position, Quaternion.identity, objectParent);
        house.GetComponent<SpriteRenderer>().sortingOrder = layer - 5;

        for (int x = -1; x <= 1; x++)
        {
            for (int y = 0; y <= 1; y++)
            {
                tilemaps[layer - 5].SetTile(tilePosition + new Vector3Int(x, y, 0), allTiles[1]);
                nodeGrid.UpdateNodeInGrid(position + new Vector3(cellSize.x * x, cellSize.y * y, 0f), tilePosition + new Vector3Int(x, y, 0));
            }
        }
    }

    public Vector3 GetRandomPoint(Vector3 currPosition, float radius)
    {
        Vector2 randomPos = Random.insideUnitCircle * radius + new Vector2(currPosition.x, currPosition.y);
        Node randomNode = nodeGrid.GetNodeFromWorldPosition(new Vector3(randomPos.x, randomPos.y, 0f));

        if (randomNode == null)
        {
            return currPosition;
        }

        return randomNode.worldPosition;
    }

    public void PlaceBridge(Vector3 position, int layer, Vector3Int direction, int width)
    {
        if ((layer - 1) % 3 != 0) //on ramp
            return;

        int tileLayer = layer - 7;

        Vector3Int tilePosition = tilemaps[0].WorldToCell(position);

        if (tilemaps[tileLayer + 2].GetTile<SeasonalRuleTile>(tilePosition) != null)
            return;

        SeasonalRuleTile tile = tilemaps[tileLayer + 1].GetTile<SeasonalRuleTile>(tilePosition - direction);
        if (tile != null && tile.tileType != TileType.Path)
            return;

        tile = tilemaps[tileLayer + 1].GetTile<SeasonalRuleTile>(tilePosition);

        if (tile == null)
        {
            tile = tilemaps[tileLayer].GetTile<SeasonalRuleTile>(tilePosition);
        }

        Vector3 cellSize = GetComponent<Grid>().cellSize;

        if (tile != null)
        {
            int found = 0;
            switch (tile.tileType)
            {
                case TileType.Water: //place bridge
                    if (direction.x == 0) //up or down
                    {
                        for (int x = -1; x <= width; x++)
                        {
                            for (int i = 0; i < 4; i++)
                            {
                                tile = tilemaps[tileLayer + 1].GetTile<SeasonalRuleTile>(tilePosition + new Vector3Int(x, direction.y * i, 0));
                                if (i < 3 && (tile == null || (tile.tileType != TileType.Water && tile.tileType != TileType.Waterfall)))
                                {
                                    if (i > 1)
                                        found++;
                                    else
                                        return;
                                }
                                else if (i == 3 && tile != null && tile.tileType != TileType.Path)
                                {
                                    return;
                                }
                            }
                        }

                        int length = 3 - found / 3;

                        for (int i = 0; i < length; i++)
                        {
                            for (int w = 0; w < width; w++)
                            {
                                tilemaps[tileLayer + 1].SetTile(tilePosition + new Vector3Int(w, direction.y * i, 0), null);
                                tilemaps[tileLayer].SetTile(tilePosition + new Vector3Int(w, direction.y * i, 0), allTiles[0]);
                                nodeGrid.UpdateNodeInGrid(position + new Vector3(w, direction.y * cellSize.y * i, 0f), tilePosition + direction * i);
                            }
                        }

                        GameObject bridge = Resources.Load<GameObject>($"BridgeV{width}{length}");
                        if (bridge == null)
                        {
                            Debug.Log($"BridgeV{width}{length}");
                            return;
                        }

                        //compensate for player position (bottom)
                        if (direction.y < 0f)
                        {
                            bridge = Instantiate(bridge, position + (length / 2f - 1f) * cellSize.y * (Vector3)direction, Quaternion.identity, objectParent);
                        }
                        else
                        {
                            bridge = Instantiate(bridge, position + length / 2f * cellSize.y * (Vector3)direction, Quaternion.identity, objectParent);
                        }

                        bridge.GetComponent<SpriteRenderer>().sortingOrder = layer - 6;
                    }
                    else //left or right
                    {
                        for (int y = -1; y <= width; y++)
                        {
                            for (int i = 0; i < 4; i++)
                            {
                                tile = tilemaps[tileLayer + 1].GetTile<SeasonalRuleTile>(tilePosition + new Vector3Int(direction.x * i, y, 0));
                                if (i < 3 && (tile == null || (tile.tileType != TileType.Water && tile.tileType != TileType.Waterfall)))
                                {
                                    if (i > 1)
                                        found++;
                                    else
                                        return;
                                }
                                else if (i == 3 && tile != null && tile.tileType != TileType.Path)
                                {
                                    return;
                                }
                            }
                        }

                        for (int i = 0; i < 3 - found / 3; i++)
                        {
                            for (int w = 0; w < width; w++)
                            {
                                tilemaps[tileLayer + 1].SetTile(tilePosition + new Vector3Int(direction.x * i, w, 0), null);
                                tilemaps[tileLayer].SetTile(tilePosition + new Vector3Int(direction.x * i, w, 0), allTiles[0]);
                                nodeGrid.UpdateNodeInGrid(position + new Vector3(direction.x * cellSize.x * i, w, 0f), tilePosition + direction * i);
                            }
                        }

                        GameObject bridge = Resources.Load<GameObject>($"BridgeH{width}{3 - found / 3}");
                        if (bridge == null)
                        {
                            Debug.Log($"BridgeV{width}{3 - found / 3}");
                            return;
                        }

                        bridge = Instantiate(bridge, position + (Vector3) direction * ((3 - found / 3) / 2f - cellSize.x * 0.5f), Quaternion.identity, objectParent);
                        bridge.GetComponent<SpriteRenderer>().sortingOrder = layer - 6;
                    }

                    break;
            }
        }
    }
}