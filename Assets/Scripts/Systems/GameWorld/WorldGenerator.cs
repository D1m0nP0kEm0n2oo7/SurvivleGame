using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour, ISaveble
{
    [Header("Настройки Terrain")]
    [SerializeField] private int _terrainSize = 200;
    [SerializeField] private int _alphamapResolution = 512;

    [Header("Настройки генерации биомов")]
    [SerializeField] private Biome[] _biomes;
    [SerializeField] private int _numCells = 20;
    [SerializeField] private int _seed = 0;



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

    private void GenerateTerrain()
    {
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

        if (_spawnWorldObjects)
            SpawnWorldObjects();

        Random.state = _defaultState;
    }

    private TerrainLayer[] GetTerrainLayersFromBiomes()
    {
        if (_biomes == null || _biomes.Length == 0)
            return new TerrainLayer[0];

        TerrainLayer[] layers = new TerrainLayer[_biomes.Length];
        for (int i = 0; i < _biomes.Length; i++)
        {
            if (_biomes[i].terrainLayer == null)
            {
                Debug.LogWarning($"Biome '{_biomes[i].biomeName}' не имеет TerrainLayer!");
                layers[i] = new TerrainLayer();
            }
            layers[i] = _biomes[i].terrainLayer;
        }
        return layers;
    }

    private float[,,] GetAlphaMap()
    {
        int alphaRes = _terrainData.alphamapResolution;
        float[,,] alphamap = new float[alphaRes, alphaRes, _biomes.Length];

        // Генерация случайных центров и биомов для них
        Vector2[] cellCenters = new Vector2[_numCells];
        int[] cellBiomes = new int[_numCells];
        for (int i = 0; i < _numCells; i++)
        {
            cellCenters[i] = new Vector2(Random.value, Random.value);
            cellBiomes[i] = Random.Range(0, _biomes.Length);
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
                for (int i = 0; i < _biomes.Length; i++)
                    alphamap[z, x, i] = (i == closestBiome) ? 1f : 0f;
            }
        }

        return alphamap;
    }
    public void SaveState(WorldData data)
    {
        data.TerrainSize = _terrainSize;
        data.AlphamapResolution = _alphamapResolution;
        data.PerlinNoiseStep = _perlinNoiseStep;
        data.PerlinNoiseScale = _perlinNoiseScale;
        data.Seed = _seed;
    }

    public void LoadState(WorldData data)
    {
        _terrainSize = data.TerrainSize;
        _alphamapResolution = data.AlphamapResolution;
        _perlinNoiseStep = data.PerlinNoiseStep;
        _perlinNoiseScale = data.PerlinNoiseScale;
        _seed = data.Seed;
    }


}