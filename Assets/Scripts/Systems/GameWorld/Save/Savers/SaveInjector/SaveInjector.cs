using UnityEngine;

public sealed class SaveInjector
{
    private readonly ISaveRegistry _registry;

    public SaveInjector(ISaveRegistry registry) => _registry = registry;

    public void Inject(GameObject root)
    {
        foreach (var client in root.GetComponentsInChildren<ISaveRegistryClient>(true))
            client.Construct(_registry);
    }
}