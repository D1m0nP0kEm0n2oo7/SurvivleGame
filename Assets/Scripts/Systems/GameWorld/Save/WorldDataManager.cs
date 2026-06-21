using UnityEngine;
using System.IO;

public class WorldManager : MonoBehaviour
{
    private string path; 

    public static WorldManager Instance;
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);

        path = Path.Combine(Application.persistentDataPath, "/seves/", "worldSave.json");
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
