using System.IO;
using UnityEngine;

public class SaveStorage
{
    private readonly string _fileName;

    public SaveStorage(string fileName = "worldSave.json") => _fileName = fileName;

    // —читаетс€ при каждом обращении: Application.persistentDataPath нельз€ читать в конструкторе MonoBehaviour
    private string FolderPath => Path.Combine(Application.persistentDataPath, "saves");
    private string FilePath => Path.Combine(FolderPath, _fileName);

    public void Write(WorldData data)
    {
        Directory.CreateDirectory(FolderPath);
        File.WriteAllText(FilePath, JsonUtility.ToJson(data));
    }

    public WorldData Read()
    {
        if (!File.Exists(FilePath)) return null;

        try { return JsonUtility.FromJson<WorldData>(File.ReadAllText(FilePath)); }
        catch { return null; }   // битый файл = нова€ игра
    }
}