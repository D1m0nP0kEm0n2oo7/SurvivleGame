using UnityEngine;

public abstract class SaveableBehaviour : MonoBehaviour, ISaveable
{
    public abstract void Save(WorldData data);
    public abstract void Load(WorldData data);
}
