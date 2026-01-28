using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WorldManager : MonoBehaviour
{
    [SerializeField] private PlayerController player;
    [SerializeField] private NodeGrid nodeGrid;

    [SerializeField] private SeasonalRuleTile[] allTiles;
    [SerializeField] private Transform objectParent;

    private Tilemap[] tilemaps;
    private Vector3 cellSize;

    private void Start()
    {
        tilemaps = transform.GetComponentsInChildren<Tilemap>();
        cellSize = GetComponent<Grid>().cellSize;

        if (GlobalManager.singleton.saveData != null)
        {
            ClearMap();
            LoadMap(GlobalManager.singleton.saveData);
        }
        else
        {
            //create world
            //set tiles
            nodeGrid.GenerateGrid(new Vector2Int(20, 19));
        }
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.N))
        {
            Seasons newSeason = allTiles[(int)TileType.Grass].season;

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
        SaveData saveData = new SaveData(nodeGrid.gridSize, player);

        for (int i = 0; i < tilemaps.Length; i++)
        {
            List<SavedTile> tiles = new List<SavedTile>();
            BoundsInt bounds = tilemaps[i].cellBounds;

            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                for (int x = bounds.xMin; x < bounds.xMax; x++)
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

    private void ClearMap()
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

    private void LoadMap(SaveData saveData)
    {
        player.Initialize(saveData.playerDirection, saveData.playerDirection[2]);
        player.transform.position = new Vector3(saveData.playerPosition[0], saveData.playerPosition[1], 0f);

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
            Structure newHouse = Resources.Load<Structure>($"{child.prefabName}");
            newHouse = Instantiate(newHouse, new Vector3(child.position[0], child.position[1], 0f), Quaternion.identity, objectParent);
            newHouse.GetComponent<SpriteRenderer>().sortingOrder = child.layer;
        }

        nodeGrid.GenerateGrid(new Vector2Int(saveData.gridSize[0], saveData.gridSize[1]));
    }

    public void RemoveStructures()
    {
        for (int i = 0; i < objectParent.transform.childCount; i++)
        {
            Structure structure = objectParent.GetChild(i).GetComponent<Structure>();
            Vector3 position = objectParent.GetChild(i).position;
            Vector3Int tilePosition = tilemaps[0].WorldToCell(position);

            int layer = structure.GetComponent<SpriteRenderer>().sortingOrder;

            if (structure.CompareTag("House"))
            {
                for (int y = structure.bottomLeft.y; y < structure.bottomLeft.y + structure.size.y; y++)
                {
                    for (int x = structure.bottomLeft.x; x < structure.bottomLeft.x + structure.size.x; x++)
                    {
                        tilemaps[layer].SetTile(tilePosition + new Vector3Int(x, y, 0), null);
                        nodeGrid.UpdateNodeInGrid(position + new Vector3(cellSize.x * x, cellSize.y * y, 0f), tilePosition + new Vector3Int(x, y, 0));
                    }
                }
            }
            else if (structure.CompareTag("Bridge"))
            {
                for (int y = structure.bottomLeft.y; y < structure.bottomLeft.y + structure.size.y; y++)
                {
                    for (int x = structure.bottomLeft.x; x < structure.bottomLeft.x + structure.size.x; x++)
                    {
                        tilemaps[layer].SetTile(tilePosition + new Vector3Int(x, y, 0), allTiles[(int)TileType.Water]);
                        tilemaps[layer - 1].SetTile(tilePosition + new Vector3Int(x, y, 0), allTiles[(int)TileType.Cliff]);
                        nodeGrid.UpdateNodeInGrid(position + new Vector3(x, cellSize.y * y, 0f), tilePosition + new Vector3Int(x, y, 0));
                    }
                }
            }

            Destroy(structure.gameObject);
        }
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
        {
            return;
        }

        int tileLayer = layer - 7;

        Vector3Int tilePosition = tilemaps[0].WorldToCell(position);

        if (tilemaps[tileLayer + 2].GetTile<SeasonalRuleTile>(tilePosition) != null)
        {
            return;
        }

        SeasonalRuleTile tile = tilemaps[tileLayer + 1].GetTile<SeasonalRuleTile>(tilePosition);

        if (tile == null)
        {
            tile = tilemaps[tileLayer].GetTile<SeasonalRuleTile>(tilePosition);
        }

        if (tile == null)
        {
            return;
        }

        switch (tile.tileType)
        {
            case TileType.Grass: //add path
                tilemaps[tileLayer + 1].SetTile(tilePosition, allTiles[(int)TileType.Path]);

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
        {
            return;
        }

        int tileLayer = layer - 7;

        Vector3Int tilePosition = tilemaps[0].WorldToCell(position);

        if (tilemaps[tileLayer + 2].GetTile<SeasonalRuleTile>(tilePosition) != null)
        {
            return;
        }

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
                    if (layer - 4 >= tilemaps.Length || nodeGrid.IsOnBorder(tilePosition))
                    {
                        return;
                    }

                    tile = tilemaps[tileLayer + 1].GetTile<SeasonalRuleTile>(tilePosition + Vector3Int.up);
                    if (tile != null && tile.tileType == TileType.Waterfall)
                    {
                        return;
                    }

                    for (int y = -1; y <= 1; y++)
                    {
                        for (int x = -1; x <= 1; x++)
                        {
                            if (y == 0 || x == 0)
                            {
                                SeasonalRuleTile tileB = tilemaps[tileLayer + 1].GetTile<SeasonalRuleTile>(tilePosition + new Vector3Int(x, y, 0));
                                if (tileB != null && (tileB.tileType == TileType.Water || tileB.tileType == TileType.Waterfall))
                                {
                                    return;
                                }
                            }
                        }
                    }

                    tilemaps[tileLayer + 1].SetTile(tilePosition, allTiles[(int)TileType.Cliff]);
                    tilemaps[tileLayer + 3].SetTile(tilePosition + Vector3Int.up, allTiles[(int)TileType.Grass]);

                    nodeGrid.UpdateNodeInGrid(position + new Vector3(0f, GetComponent<Grid>().cellSize.y, 0f), tilePosition + Vector3Int.up);
                    nodeGrid.UpdateNodeInGrid(position, tilePosition);

                    break;
                case TileType.Cliff:
                    tile = tilemaps[tileLayer + 3].GetTile<SeasonalRuleTile>(tilePosition + Vector3Int.up);

                    if (tilemaps[tileLayer + 5].GetTile<SeasonalRuleTile>(tilePosition + Vector3Int.up) != null)
                    {
                        return;
                    }

                    if (tile != null && tile.tileType == TileType.Grass) //remove cliff
                    {
                        for (int y = -1; y <= 1; y++)
                        {
                            for (int x = -1; x <= 1; x++)
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
            {
                return;
            }

            tile = tilemaps[tileLayer + 1].GetTile<SeasonalRuleTile>(tilePosition + Vector3Int.down);

            if (tile == null)
            {
                tile = tilemaps[tileLayer].GetTile<SeasonalRuleTile>(tilePosition);
            }

            if (tile != null && (tile.tileType == TileType.Grass || tile.tileType == TileType.Ramp))
            {
                tilemaps[tileLayer + 1].SetTile(tilePosition - Vector3Int.up, allTiles[(int)TileType.Cliff]);
                tilemaps[tileLayer + 3].SetTile(tilePosition, allTiles[(int)TileType.Grass]);

                nodeGrid.UpdateNodeInGrid(position - new Vector3(0f, GetComponent<Grid>().cellSize.y, 0f), tilePosition - Vector3Int.up);
                nodeGrid.UpdateNodeInGrid(position, tilePosition);
            }
        }
    }

    public void Waterscape(Vector3 position, int layer)
    {
        if ((layer - 1) % 3 != 0) //on ramp
        {
            return;
        }

        int tileLayer = layer - 7;

        Vector3Int tilePosition = tilemaps[0].WorldToCell(position);

        SeasonalRuleTile tile = tilemaps[tileLayer + 1].GetTile<SeasonalRuleTile>(tilePosition);

        if (tile == null)
        {
            tile = tilemaps[tileLayer].GetTile<SeasonalRuleTile>(tilePosition);
        }

        if (tile == null || tilemaps[tileLayer + 2].GetTile<SeasonalRuleTile>(tilePosition) != null)
        {
            return;
        }

        switch (tile.tileType)
        {
            case TileType.Path:
            case TileType.Grass: //add water
                bool invalidNeighbor = false;

                for (int y = -1; y <= 1; y++)
                {
                    for (int x = -1; x <= 1; x++)
                    {
                        if (y == 0 || x == 0)
                        {
                            SeasonalRuleTile tileA = tilemaps[tileLayer].GetTile<SeasonalRuleTile>(tilePosition + new Vector3Int(x, y, 0));
                            SeasonalRuleTile tileB = tilemaps[tileLayer + 1].GetTile<SeasonalRuleTile>(tilePosition + new Vector3Int(x, y, 0));
                            if (tileA == null || (tileB != null && tileB.tileType == TileType.Cliff))
                            {
                                if (y >= 0)
                                {
                                    return;
                                }
                                
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
                    {
                        return;
                    }
                    
                    tileA = tilemaps[tileLayer].GetTile<SeasonalRuleTile>(tilePosition + new Vector3Int(0, -2, 0));
                    tileB = tilemaps[tileLayer - 3].GetTile<SeasonalRuleTile>(tilePosition + new Vector3Int(0, -2, 0));
                    if (tileA != null || tileB == null || tileB.tileType != TileType.Grass)
                    {
                        return;
                    }

                    tilemaps[tileLayer].SetTile(tilePosition, allTiles[(int)TileType.Cliff]);
                    tilemaps[tileLayer + 1].SetTile(tilePosition, allTiles[(int)TileType.Waterfall]);
                    tilemaps[tileLayer - 2].SetTile(tilePosition + new Vector3Int(0, -1, 0), allTiles[(int)TileType.Waterfall]);

                    nodeGrid.UpdateNodeInGrid(position - new Vector3(0f, GetComponent<Grid>().cellSize.y, 0f), tilePosition + new Vector3Int(0, -1, 0));
                }
                else
                {
                    tilemaps[tileLayer].SetTile(tilePosition, allTiles[(int)TileType.Cliff]);
                    tilemaps[tileLayer + 1].SetTile(tilePosition, allTiles[(int)TileType.Water]);
                }

                nodeGrid.UpdateNodeInGrid(position, tilePosition);

                break;
            case TileType.Water: //remove water
                tilemaps[tileLayer + 1].SetTile(tilePosition, null);
                tilemaps[tileLayer].SetTile(tilePosition, allTiles[(int)TileType.Grass]);

                nodeGrid.UpdateNodeInGrid(position, tilePosition);

                break;
            case TileType.Waterfall: //remove water
                if (tileLayer > 2)
                {
                    tile = tilemaps[tileLayer - 2].GetTile<SeasonalRuleTile>(tilePosition + Vector3Int.down);

                    if (tile != null && tile.tileType == TileType.Waterfall)
                    {
                        tilemaps[tileLayer + 1].SetTile(tilePosition, null);
                        tilemaps[tileLayer].SetTile(tilePosition, allTiles[(int)TileType.Grass]);
                        tilemaps[tileLayer - 2].SetTile(tilePosition + Vector3Int.down, allTiles[(int)TileType.Cliff]);

                        nodeGrid.UpdateNodeInGrid(position, tilePosition);
                    }
                    else
                    {
                        tile = tilemaps[tileLayer + 3].GetTile<SeasonalRuleTile>(tilePosition + Vector3Int.up);
                        if (tile != null && tile.tileType == TileType.Waterfall)
                        {
                            tilemaps[tileLayer + 1].SetTile(tilePosition, null);
                            tilemaps[tileLayer].SetTile(tilePosition, allTiles[(int)TileType.Grass]);
                            tilemaps[tileLayer + 3].SetTile(tilePosition + Vector3Int.up, allTiles[(int)TileType.Cliff]);

                            nodeGrid.UpdateNodeInGrid(position + new Vector3(0f, GetComponent<Grid>().cellSize.y, 0f), tilePosition + Vector3Int.up);
                        }
                    }
                }
                else if (tileLayer < 9)
                {
                    tile = tilemaps[tileLayer + 4].GetTile<SeasonalRuleTile>(tilePosition + Vector3Int.up);
                    if (tile != null && tile.tileType == TileType.Waterfall)
                    {
                        tilemaps[tileLayer + 1].SetTile(tilePosition, allTiles[(int)TileType.Cliff]);
                        tilemaps[tileLayer + 3].SetTile(tilePosition + Vector3Int.up, allTiles[(int)TileType.Grass]);
                        tilemaps[tileLayer + 4].SetTile(tilePosition + Vector3Int.up, null);

                        nodeGrid.UpdateNodeInGrid(position + new Vector3(0f, GetComponent<Grid>().cellSize.y, 0f), tilePosition + Vector3Int.up);
                    }
                }

                break;
        }
    }

    public void PlaceRamp(Vector3 position, int layer)
    {
        if ((layer - 1) % 3 != 0) //on ramp
        {
            return;
        }

        int tileLayer = layer - 7;

        Vector3Int tilePosition = tilemaps[0].WorldToCell(position);

        SeasonalRuleTile tile = tilemaps[tileLayer + 1].GetTile<SeasonalRuleTile>(tilePosition);

        if (tile == null)
        {
            tile = tilemaps[tileLayer].GetTile<SeasonalRuleTile>(tilePosition);
        }

        if (tilemaps[tileLayer + 2].GetTile<SeasonalRuleTile>(tilePosition) != null)
        {
            return;
        }

        if (tile != null)
        {
            switch (tile.tileType)
            {
                case TileType.Path: //place ramp
                case TileType.Grass:
                    if (tilemaps[tileLayer + 2].GetTile<SeasonalRuleTile>(tilePosition + Vector3Int.up) != null)
                    {
                        return;
                    }

                    tile = tilemaps[tileLayer + 1].GetTile<SeasonalRuleTile>(tilePosition + Vector3Int.up);
                    if (tile != null && tile.tileType == TileType.Cliff)
                    {
                        tilemaps[tileLayer + 1].SetTile(tilePosition, allTiles[(int)TileType.Ramp]);
                        tilemaps[tileLayer + 1].SetTile(tilePosition + Vector3Int.up, allTiles[(int)TileType.Ramp]);

                        nodeGrid.UpdateNodeInGrid(position, tilePosition);
                        nodeGrid.UpdateNodeInGrid(position + new Vector3(0f, GetComponent<Grid>().cellSize.y, 0f), tilePosition + Vector3Int.up);
                    }

                    break;
                /*case TileType.Ramp: //remove ramp
                    tile = tilemaps[tileLayer + 1].GetTile<SeasonalRuleTile>(tilePosition + Vector3Int.up);
                    if (tile != null && tile.tileType == TileType.Ramp)
                    {
                        tilemaps[tileLayer + 1].SetTile(tilePosition, null);
                        tilemaps[tileLayer + 1].SetTile(tilePosition + Vector3Int.up, allTiles[(int)TileType.Cliff]);

                        nodeGrid.UpdateNodeInGrid(position, tilePosition);
                        nodeGrid.UpdateNodeInGrid(position + new Vector3(0f, GetComponent<Grid>().cellSize.y, 0f), tilePosition + Vector3Int.up);
                    }

                    break;*/
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
                    tilemaps[tileLayer - 2].SetTile(tilePosition, allTiles[(int)TileType.Ramp]);
                    tilemaps[tileLayer - 2].SetTile(tilePosition + Vector3Int.down, allTiles[(int)TileType.Ramp]);

                    nodeGrid.UpdateNodeInGrid(position - new Vector3(0f, GetComponent<Grid>().cellSize.y, 0f), tilePosition + Vector3Int.down);
                    nodeGrid.UpdateNodeInGrid(position, tilePosition);
                }
            }
        }
    }

    public void PlaceHouse(Vector3 position, int layer)
    {
        if ((layer - 1) % 3 != 0) //on ramp
        {
            return;
        }

        for (int y = 0; y <= 1; y++)
        {
            for (int x = -1; x <= 1; x++)
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
        {
            return;
        }

        Structure house = Resources.Load<Structure>("House");
        house = Instantiate(house, position, Quaternion.identity, objectParent);
        house.GetComponent<SpriteRenderer>().sortingOrder = layer - 5;

        for (int y = 0; y <= 1; y++)
        {
            for (int x = -1; x <= 1; x++)
            {
                tilemaps[layer - 5].SetTile(tilePosition + new Vector3Int(x, y, 0), allTiles[(int)TileType.Cliff]);
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
        {
            return;
        }

        int tileLayer = layer - 7;

        Vector3Int tilePosition = tilemaps[0].WorldToCell(position);

        if (tilemaps[tileLayer + 2].GetTile<SeasonalRuleTile>(tilePosition) != null)
        {
            return;
        }

        SeasonalRuleTile tile = tilemaps[tileLayer + 1].GetTile<SeasonalRuleTile>(tilePosition - direction);
        if (tile != null && tile.tileType != TileType.Path)
        {
            return;
        }

        tile = tilemaps[tileLayer + 1].GetTile<SeasonalRuleTile>(tilePosition);

        if (tile == null)
        {
            tile = tilemaps[tileLayer].GetTile<SeasonalRuleTile>(tilePosition);
        }

        if (tile != null)
        {
            if (tile.tileType == TileType.Water) //place bridge
            {
                int length = -1;

                if (direction.x == 0) //up or down
                {
                    for (int w = -1; w <= width; w++)
                    {
                        int temp = 0;
                        for (int i = 0; i < 3; i++)
                        {
                            temp = i;
                            
                            tile = tilemaps[tileLayer + 1].GetTile<SeasonalRuleTile>(tilePosition + new Vector3Int(w, direction.y * i, 0));
                            if (tile == null || (tile.tileType != TileType.Water && tile.tileType != TileType.Waterfall))
                            {
                                temp--;
                                if (temp < 0)
                                {
                                    return;
                                }

                                break;
                            }
                        }

                        if (temp < 1 || (length >= 0 && temp != length))
                        {
                            return;
                        }
                        else
                        {
                            length = temp;
                        }
                    }

                    length++;

                    tile = tilemaps[tileLayer + 1].GetTile<SeasonalRuleTile>(tilePosition + new Vector3Int(0, direction.y * length, 0));
                    if (tile != null && tile.tileType != TileType.Path)
                    {
                        return;
                    }

                    Structure bridge = Resources.Load<Structure>($"BridgeV{width}{length}");
                    if (bridge == null)
                    {
                        Debug.Log($"BridgeV{width}{length}");
                        return;
                    }

                    for (int y = 0; y < length; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            tilemaps[tileLayer + 1].SetTile(tilePosition + new Vector3Int(x, direction.y * y, 0), null);
                            tilemaps[tileLayer].SetTile(tilePosition + new Vector3Int(x, direction.y * y, 0), allTiles[(int)TileType.Grass]);
                            nodeGrid.UpdateNodeInGrid(position + new Vector3(x, direction.y * cellSize.y * y, 0f), tilePosition + direction * y);
                        }
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
                    for (int w = -1; w <= width; w++)
                    {
                        int temp = 0;
                        for (int i = 0; i < 3; i++)
                        {
                            temp = i;

                            tile = tilemaps[tileLayer + 1].GetTile<SeasonalRuleTile>(tilePosition + new Vector3Int(direction.x * i, w, 0));
                            if (tile == null || (tile.tileType != TileType.Water && tile.tileType != TileType.Waterfall))
                            {
                                temp--;
                                if (temp < 0)
                                {
                                    return;
                                }

                                break;
                            }
                        }

                        if (temp < 1 || (length >= 0 && temp != length))
                        {
                            return;
                        }
                        else
                        {
                            length = temp;
                        }
                    }

                    length++;

                    tile = tilemaps[tileLayer + 1].GetTile<SeasonalRuleTile>(tilePosition + new Vector3Int(direction.x * length, 0, 0));
                    if (tile != null && tile.tileType != TileType.Path)
                    {
                        return;
                    }

                    Structure bridge = Resources.Load<Structure>($"BridgeH{width}{length}");
                    if (bridge == null)
                    {
                        Debug.Log($"BridgeV{width}{length}");
                        return;
                    }

                    for (int x = 0; x < width; x++)
                    {
                        for (int y = 0; y < length; y++)
                        {
                            tilemaps[tileLayer + 1].SetTile(tilePosition + new Vector3Int(direction.x * y, x, 0), null);
                            tilemaps[tileLayer].SetTile(tilePosition + new Vector3Int(direction.x * y, x, 0), allTiles[(int)TileType.Grass]);
                            nodeGrid.UpdateNodeInGrid(position + new Vector3(direction.x * cellSize.x * y, x, 0f), tilePosition + direction * y);
                        }
                    }

                    bridge = Instantiate(bridge, position + (Vector3)direction * (length / 2f - cellSize.x * 0.5f), Quaternion.identity, objectParent);
                    bridge.GetComponent<SpriteRenderer>().sortingOrder = layer - 6;
                }
            }
        }
    }

    public void ReturnToMenu()
    {
        GlobalManager.singleton.LoadScene("MenuScene");
    }
}