using UnityEngine;

public class GameWorldBootstrap : MonoBehaviour
{
    [SerializeField] private WorldGenerationConfig _config;
    [SerializeField] private TerrainGenerator _terrainGenerator;
    [SerializeField] private WorldObjectSpawner _objectSpawner;
    [SerializeField] private CharacterSpawner _characterSpawner;
    [SerializeField] private SaveManager _saveManager;

    private WorldSettings _settings;
    private SaveInjector _injector;

    private void Awake()
    {
        _injector = new SaveInjector(_saveManager);

        WorldData save = _saveManager.TryRead();
        bool isLoad = save?.World != null;

        _settings = isLoad
            ? save.World
            : WorldSettings.Create(_config, Random.Range(int.MinValue, int.MaxValue));

        GeneratedWorld world = _terrainGenerator.Generate(_settings, _config);

        if (world.Terrain == null)
        {
            Debug.LogError("Terrain не сгенерирован");
            return;
        }

        _objectSpawner.SpawnWorldObjects(world, _settings);
        GameObject character = _characterSpawner.SpawnCharacter(world.Terrain);

        // Регистрация всех ISaveRegistryClient
        _injector.Inject(gameObject);
        _injector.Inject(world.Terrain.gameObject);
        _injector.Inject(character);

        if (isLoad)
        {
            _saveManager.RestoreAll(save);

            // Если Load создал новые объекты с SaveableBehaviour — зарегистрировать их тоже
            _injector.Inject(gameObject);
            _injector.Inject(world.Terrain.gameObject);
        }
        else
        {
            SaveGame();
        }
    }

    public void SaveGame() => _saveManager.Save(_settings);

    private void OnApplicationQuit() => SaveGame();
}