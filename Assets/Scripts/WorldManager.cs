using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WorldManager : MonoBehaviour
{
    [SerializeField] private MovementController player;

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
    }
}