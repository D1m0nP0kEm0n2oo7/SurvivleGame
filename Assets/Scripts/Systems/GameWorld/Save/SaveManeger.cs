using UnityEngine;

public class SaveManager : MonoBehaviour
{
    [SerializeField] private ISaveable[] _savebles;

    private WorldData _worldData;
    private WorldDataManager _dataManager;
    public void Init()
    {
        _dataManager = new WorldDataManager();
        _worldData = new WorldData();
    }

    public void AllSave()
    {
        foreach (var saveble in _savebles)
        {
            _worldData = saveble.SaveState(_worldData);
        }
        _dataManager.Save(_worldData);
    }

    public void AllLoad()
    {
        _worldData = _dataManager.Load();
        if (_worldData == null)
        {
            Debug.Log("WorldData don`t load");
            return;
        }
        foreach (var saveble in _savebles)
        {
            saveble.LoadState(_worldData);
        }
    }
}
