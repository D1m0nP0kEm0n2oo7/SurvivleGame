using UnityEngine;

public class SaveManeger : MonoBehaviour
{
    [SerializeField] private ISaveble[] _savebles;

    private WorldData _worldData;

    public void Save()
    {
        _worldData = new WorldData();

        foreach (var se in _savebles)
        {
            se.SaveState(_worldData);
        }
        WorldManager.Instance.Save(_worldData);
    }

    public void Load()
    {
        _worldData = WorldManager.Instance.Load();

        foreach (var se in _savebles)
        {
            se.LoadState(_worldData);
        }
    }
}
