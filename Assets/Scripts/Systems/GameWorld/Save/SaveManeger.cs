using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SaveManager : MonoBehaviour, ISaveRegistry
{
    private readonly List<ISaveable> _saveables = new();
    private readonly SaveStorage _storage = new SaveStorage();

    public WorldData Current { get; private set; }

    public void Register(ISaveable saveable)
    {
        if (saveable != null && !_saveables.Contains(saveable))
            _saveables.Add(saveable);
    }

    public void Unregister(ISaveable saveable) => _saveables.Remove(saveable);

    public WorldData TryRead() => _storage.Read();

    public void Save(WorldSettings world)
    {
        _saveables.RemoveAll(s => s is Object o && o == null);

        var data = new WorldData { World = world };
        foreach (var s in _saveables) s.Save(data);

        Current = data;
        _storage.Write(data);
    }

    public void RestoreAll(WorldData data)
    {
        Current = data;
        foreach (var s in _saveables.OrderBy(saveable => saveable.LoadPriority))
            s.Load(data);
    }
}