using UnityEngine;

public class SaveManager : MonoBehaviour
{
    [SerializeField] private SaveableBehaviour[] _saveables;   // WorldSaver, ObjectsSaver, PlayerSaver
    private readonly SaveStorage _storage = new();

    public void Load()
    {
        WorldData data = _storage.Read() ?? new WorldData();   // пустой = новая игра
        foreach (var saveable in _saveables) saveable.Load(data);
    }

    public void Save()
    {
        var data = new WorldData();
        foreach (var saveable in _saveables) saveable.Save(data);
        _storage.Write(data);
    }
}