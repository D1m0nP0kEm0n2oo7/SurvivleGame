using UnityEngine;

public class GameWorldBootstrap : MonoBehaviour
{
    [SerializeField] private SaveManager _save;
    [SerializeField] private TerrainGenerator _terrain;
    [SerializeField] private WorldObjectSpawner _objects;
    [SerializeField] private CharacterSpawner _character;

    private void Awake()
    {
        _save.Load();

        GeneratedWorld world = _terrain.Generate();
        if (!world.IsValid) return;

        _objects.Spawn(world);
        _character.Spawn(world);
        _save.Save();
    }

    private void OnApplicationQuit() => _save.Save();
}