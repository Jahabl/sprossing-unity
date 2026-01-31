using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

public class SaveData
{
    public int[] gridSize;
    public int season;

    public List<SavedTile> layer0;
    public List<SavedTile> layer1;
    public List<SavedTile> layer2;
    public List<SavedTile> layer3;
    public List<SavedTile> layer4;
    public List<SavedTile> layer5;
    public List<SavedTile> layer6;
    public List<SavedTile> layer7;
    public List<SavedTile> layer8;

    public List<SavedObject> objects;

    public float[] playerPosition;
    public int[] playerDirection;

    public SaveData(Vector2Int gridSize, PlayerController player, int season)
    {
        this.gridSize = new int[2];
        this.gridSize[0] = gridSize.x;
        this.gridSize[1] = gridSize.y;

        playerPosition = new float[2];
        playerDirection = new int[3];

        objects = new List<SavedObject>();

        playerPosition[0] = player.transform.position.x;
        playerPosition[1] = player.transform.position.y;

        playerDirection[0] = player.LastDirection.x;
        playerDirection[1] = player.LastDirection.y;
        playerDirection[2] = player.layer;

        this.season = season;
    }
}