using UnityEditor;
using UnityEngine;



public class TerrainGenerator : MonoBehaviour, ISaveble
{
    [Header("Настройки Terrain")]
    [SerializeField] private int _terrainSize;
    [SerializeField] private int _alphamapResolution;

    [Header("Настройки генерации биомов")]

    [SerializeField] private float _perlinNoiseStep;
    [SerializeField] private float _perlinNoiseScale;
    [SerializeField] private BiomeDatabase _biomes;
    [SerializeField] private int _numCells;
    [SerializeField] private int _seed = 0;

    [Header("Спавнер обектов")]
    [SerializeField] private WorldObjectSpawner _objectSpawner;

    private const int _baseTerrainSize = 200;
    private const int _baseAlphamapResolution = 512;
    private const int _baseNumCells = 20;
    private const float _basePerlinNoiseStep = 2.0f;
    private const float _basePerlinNoiseScale = 35f;

    private Random.State _defaultState;
    private Terrain _terrain;
    private TerrainData _terrainData;
    private Transform _terrainTransform;

    private float[,,] _alphamap;

    public int LoadPriority => 0;

    private void Start()
    {
        GenerateTerrain();
    }

    public void ReGeneration()
    {
        _seed = 0;
        GenerateTerrain();
    }

    private void WorldScaling()
    {
        if (_terrainSize <= 0)
            _terrainSize = _baseTerrainSize;

        float worldScale = (float)_terrainSize / _baseTerrainSize;

        if (_alphamapResolution <= 0)
            _alphamapResolution = Mathf.RoundToInt(_baseAlphamapResolution * worldScale);

        if (_numCells <= 0)
            _numCells = Mathf.RoundToInt(_baseNumCells * worldScale);

        if (_perlinNoiseStep <= 0f)
            _perlinNoiseStep = _basePerlinNoiseStep;

        if (_perlinNoiseScale <= 0f)
            _perlinNoiseScale = _basePerlinNoiseScale * worldScale;
    }

    private void GenerateTerrain()
    {
        WorldScaling();
        if (_terrain == null)
        {

            GameObject terrainGO = Terrain.CreateTerrainGameObject(new TerrainData());
            _terrain = terrainGO.GetComponent<Terrain>();
            _terrain.transform.parent = transform;
            _terrain.name = "GeneratedTerrain";
            _terrainTransform = terrainGO.transform;
        }
        else
        {
            for (int i = _terrainTransform.childCount - 1; i >= 0; i--)
            {
                Transform child = _terrainTransform.GetChild(i);
                DestroyImmediate(child.gameObject);
            }
        }

        // Настроить TerrainData
        _terrainData = _terrain.terrainData;
        _terrainData.alphamapResolution = _alphamapResolution;
        _terrainData.size = new Vector3(_terrainSize, 100f, _terrainSize);

        if (_seed == 0)
        {
            _seed = Random.Range(int.MinValue, int.MaxValue);
            Debug.Log($"seed = {_seed}");
        }
        _defaultState = Random.state;
        Random.InitState(_seed);

        _terrainData.terrainLayers = GetTerrainLayersFromBiomes();

        _alphamap = GetAlphaMap();
        _terrainData.SetAlphamaps(0, 0, _alphamap);

        if (_objectSpawner)
            _objectSpawner.SpawnWorldObjects(_terrain,
                _terrainData,
                _alphamap,
                _biomes.Biomes,
                _perlinNoiseStep,
                _perlinNoiseScale
                );

        Random.state = _defaultState;
    }

    private TerrainLayer[] GetTerrainLayersFromBiomes()
    {
        if (_biomes.Biomes == null || _biomes.Biomes.Length == 0)
            return new TerrainLayer[0];

        TerrainLayer[] layers = new TerrainLayer[_biomes.Biomes.Length];
        for (int i = 0; i < _biomes.Biomes.Length; i++)
        {
            if (_biomes.Biomes[i].terrainLayer == null)
            {
                Debug.LogWarning($"Biome '{_biomes.Biomes[i].biomeName}' не имеет TerrainLayer!");
                layers[i] = new TerrainLayer();
            }
            layers[i] = _biomes.Biomes[i].terrainLayer;
        }
        return layers;
    }

    private float[,,] GetAlphaMap()
    {
        int alphaRes = _terrainData.alphamapResolution;
        float[,,] alphamap = new float[alphaRes, alphaRes, _biomes.Biomes.Length];

        // Генерация случайных центров и биомов для них
        Vector2[] cellCenters = new Vector2[_numCells];
        int[] cellBiomes = new int[_numCells];
        for (int i = 0; i < _numCells; i++)
        {
            cellCenters[i] = new Vector2(Random.value, Random.value);
            cellBiomes[i] = Random.Range(0, _biomes.Biomes.Length);
        }

        // Для каждой точки альфа-карты находим ближайшее ядро
        for (int z = 0; z < alphaRes; z++)
        {
            for (int x = 0; x < alphaRes; x++)
            {
                float normZ = (float)z / alphaRes;
                float normX = (float)x / alphaRes;

                float minDist = float.MaxValue;
                int closestBiome = 0;

                for (int i = 0; i < _numCells; i++)
                {
                    float dx = normX - cellCenters[i].x;
                    float dy = normZ - cellCenters[i].y;
                    float dist = dx * dx + dy * dy; // квадрат расстояния (быстрее sqrt)

                    if (dist < minDist)
                    {
                        minDist = dist;
                        closestBiome = cellBiomes[i];
                    }
                }

                // Устанавливаем вес 1 для выбранного биома, 0 для остальных
                for (int i = 0; i < _biomes.Biomes.Length; i++)
                    alphamap[z, x, i] = (i == closestBiome) ? 1f : 0f;
            }
        }

        return alphamap;
    }


    public void SaveState(WorldData data)
    {
        data.TerrainSize = _terrainSize;
        data.AlphamapResolution = _alphamapResolution;
        data.NumCells = _numCells;
        data.PerlinNoiseStep = _perlinNoiseStep;
        data.PerlinNoiseScale = _perlinNoiseScale;
        data.Seed = _seed;
    }

    public void LoadState(WorldData data)
    {
        _terrainSize = data.TerrainSize;
        _alphamapResolution = data.AlphamapResolution;
        _numCells = data.NumCells;
        _perlinNoiseStep = data.PerlinNoiseStep;
        _perlinNoiseScale = data.PerlinNoiseScale;
        _seed = data.Seed;
    }

    [ContextMenu("Generate Terrain")]
    private void GenerateInEditor()
    {
        GenerateTerrain();
        #if UNITY_EDITOR
                SaveTerrainDataAsAsset();
        #endif
    }
    #if UNITY_EDITOR
    private void SaveTerrainDataAsAsset()
    {
    string path = $"Assets/GeneratedTerrains/Terrain_{_seed}.asset";
    System.IO.Directory.CreateDirectory("Assets/GeneratedTerrains");

    AssetDatabase.CreateAsset(_terrainData, path);
    AssetDatabase.SaveAssets();
    AssetDatabase.Refresh();

    Debug.Log($"TerrainData saved to {path}");
    }
    #endif
}