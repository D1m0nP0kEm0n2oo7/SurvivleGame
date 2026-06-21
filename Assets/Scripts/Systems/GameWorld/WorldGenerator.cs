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

    [Header("Спавн объектов")]
    [SerializeField] private bool _spawnWorldObjects = true;
    [SerializeField] private float _perlinNoiseStep = 2.0f;
    [SerializeField] private float _perlinNoiseScale = 35f;

    private Random.State _defaultState;

    private Terrain _terrain;
    private TerrainData _terrainData;
    private Transform _terrainTransform;

    private float[,,] _alphamap;

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

    private void SpawnWorldObjects()
    {
        if (_terrain == null || _terrainData == null) return;

        int alphaRes = _terrainData.alphamapResolution;
        float[,,] alphamap = _alphamap;
        float terrainSize = _terrainSize;
        Transform terrainTransform = _terrainTransform;

        float offsetX = Random.Range(0f, 1f);
        float offsetZ = Random.Range(0f, 1f);
        Debug.Log($"offsetX = {offsetX}, offsetZ = {offsetZ}");

        // Для временного списка кандидатов (чтобы не аллоцировать каждый раз, можно вынести, но для простоты оставим так)
        List<GameObject> candidates = new List<GameObject>();

        for (float x = 0; x < terrainSize; x += _perlinNoiseStep)
        {
            for (float z = 0; z < terrainSize; z += _perlinNoiseStep)
            {
                // 1. Шум Перлина
                float normX = x / terrainSize;
                float normZ = z / terrainSize;

                float noiseX = (normX + offsetX) * _perlinNoiseScale;
                float noiseZ = (normZ + offsetZ) * _perlinNoiseScale;
                float noiseValue = Mathf.PerlinNoise(noiseX, noiseZ);

                // 2. Биом по Вороному
                int alphaZ = Mathf.FloorToInt(normZ * alphaRes);
                int alphaX = Mathf.FloorToInt(normX * alphaRes);
                alphaZ = Mathf.Clamp(alphaZ, 0, alphaRes - 1);
                alphaX = Mathf.Clamp(alphaX, 0, alphaRes - 1);

                float maxWeight = 0f;
                int dominantBiome = -1;
                for (int b = 0; b < _biomes.Length; b++)
                {
                    float weight = alphamap[alphaZ, alphaX, b];
                    if (weight > maxWeight)
                    {
                        maxWeight = weight;
                        dominantBiome = b;
                    }
                }
                if (dominantBiome < 0) continue;

                Biome biome = _biomes[dominantBiome];
                if (biome.worldObjects == null) continue;

                // 3. Собираем объекты, прошедшие персональный порог
                candidates.Clear();
                foreach (WorldObject worldObj in biome.worldObjects)
                {
                    if (worldObj.prefab == null) continue;

                    float threshold = 1f - worldObj.spawnChance;
                    if (noiseValue > threshold)
                    {
                        candidates.Add(worldObj.prefab);
                    }
                }

                // 4. Если есть кандидаты — спавним одного случайного
                if (candidates.Count > 0)
                {
                    GameObject chosen = candidates[Random.Range(0, candidates.Count)];
                    Vector3 spawnPos = new Vector3(
                        terrainTransform.position.x + x + Random.Range(-0.5f, 0.5f),
                        0,
                        terrainTransform.position.z + z + Random.Range(-0.5f, 0.5f));
                    Instantiate(chosen, spawnPos, Quaternion.Euler(0, Random.Range(-0, 180),0), terrainTransform);
                }
            }
        }
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