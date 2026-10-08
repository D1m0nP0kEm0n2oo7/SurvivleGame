using UnityEngine;
using System.IO;

public class WorldDataManager 
{
    private string path; 

    public void Init()
    {
        path = Path.Combine(Application.persistentDataPath, "saves", "worldSave.json");
        Debug.Log(path);
    }
    public void Save(WorldData data)
    {
        string json = JsonUtility.ToJson(data);
        File.WriteAllText(path, json);
    }

    public WorldData Load()
    {
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            WorldData data = JsonUtility.FromJson<WorldData>(json);

            return data;
        }
        return null;
    }
}
