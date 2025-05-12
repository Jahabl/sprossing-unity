using System.Collections.Generic;
using UnityEngine;

public class SaveData
{
    public List<SavedTile> layer0;
    public List<SavedTile> layer1;
    public List<SavedTile> layer2;
    public List<SavedTile> layer3;
    public List<SavedTile> layer4;
    public List<SavedTile> layer5;
    public List<SavedTile> layer6;
    public List<SavedTile> layer7;
    public List<SavedTile> layer8;

    public float[] playerPosition;
    public int[] playerDirection;

    public SaveData(PlayerController player)
    {
        playerPosition = new float[2];
        playerDirection = new int[3];

        playerPosition[0] = player.transform.position.x;
        playerPosition[1] = player.transform.position.y;

        playerDirection[0] = player.LastDirection.x;
        playerDirection[1] = player.LastDirection.y;
        playerDirection[2] = player.layer;
    }
}