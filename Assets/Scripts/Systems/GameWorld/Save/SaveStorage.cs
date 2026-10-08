using System.IO;
using UnityEngine;

public class SaveStorage
{
    private readonly string _fileName;
    private string _path;

    public SaveStorage(string fileName = "worldSave.json")
    {
        _fileName = fileName;  
    }

    private string Path
    {
        get
        {
            if (_path == null)
            {
                _path = System.IO.Path.Combine(Application.persistentDataPath, "saves", _fileName);
                Debug.Log(_path);
            }

            return _path;
        }
    }

    public bool Exists => File.Exists(Path);

    public void Write(WorldData data)
    {
        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(Path));
        string tmp = Path + ".tmp";
        File.WriteAllText(tmp, JsonUtility.ToJson(data));
        if (File.Exists(Path)) File.Replace(tmp, Path, Path + ".bak");
        else File.Move(tmp, Path);
    }

    public WorldData Read()
    {
        if (!File.Exists(Path)) return null;
        try { return JsonUtility.FromJson<WorldData>(File.ReadAllText(Path)); }
        catch (System.Exception e)
        {
            Debug.LogError($"Save corrupted: {e}");
            return null;
        }
    }
}