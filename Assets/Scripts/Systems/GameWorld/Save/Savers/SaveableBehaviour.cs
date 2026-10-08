using UnityEngine;

public abstract class SaveableBehaviour : MonoBehaviour, ISaveable, ISaveRegistryClient
{
    private ISaveRegistry _registry;

    public abstract void Save(WorldData data);
    public abstract void Load(WorldData data);

    public void Construct(ISaveRegistry registry)
    {
        if (_registry == registry) return;           // повторный инжект безопасен
        _registry?.Unregister(this);
        _registry = registry;
        _registry.Register(this);
    }

    protected virtual void OnDestroy() => _registry?.Unregister(this);
}
