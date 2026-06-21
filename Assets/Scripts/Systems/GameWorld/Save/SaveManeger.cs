using UnityEngine;

public class SaveManeger : MonoBehaviour
{
    [SerializeField] private ISaveble[] _sevebles;

    private WorldData _worldData;

    public void Save()
    {
        _worldData = new WorldData();

        foreach (var se in _sevebles)
        {
            se.SeveState(_worldData);
        }
        WorldManager.Instance.Save(_worldData);
    }

    public void Load()
    {
        _worldData = WorldManager.Instance.Load();

        foreach (var se in _sevebles)
        {
            se.LoadState(_worldData);
        }
    }
}
