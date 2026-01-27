using System.IO;
using UnityEngine;

public static class SaveManager
{
    private static readonly string filePath = Application.persistentDataPath + "/testLevel.json";
    public static void SaveData(SaveData saveData)
    {
        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(filePath, json);
    }

    public static SaveData LoadData()
    {
        string json = File.ReadAllText(filePath);
        return JsonUtility.FromJson<SaveData>(json);
    }

    public static void DeleteData()
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }
}