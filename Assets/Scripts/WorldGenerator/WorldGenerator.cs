using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    [Header("Настройки Terrain")]
    [SerializeField] private int _terrainSize = 200;
    [SerializeField] private int _alphamapResolution = 512;

    [Header("Настройки генерации биомов")]
    [SerializeField] private Biome[] _biomes;
    [SerializeField] private int _numCells = 10;
    [SerializeField] private int _seed = 0;

    [Header("Спавн объектов")]
    [SerializeField] private bool _spawnWorldObjects = true;
    [SerializeField] private float _perlinNoisestep = 5.0f;
    [SerializeField] private float _perlinNoiseScale = 0.1f;
    [SerializeField] private float _perlinNoiseThreshold = 0.5f;

    private Random.State _defaultState;

    private Terrain _terrain;
    private TerrainData _terrainData;
    private Transform _terrainTransform;

    private float[,,] _alphamap;

    void Start()
    {
        GenerateTerrain();
    }

    public void GenerateTerrain()
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
        _seed = 0;
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

        // Проходим по всей площади террейна
        for (float x = 0; x < terrainSize; x += _perlinNoisestep)
        {
            for (float z = 0; z < terrainSize; z += _perlinNoisestep)
            {
                // 1. Вычисляем единый шум Перлина для этой точки
                float normX = x / terrainSize;
                float normZ = z / terrainSize;

                float noiseX = (normX + offsetX) * _perlinNoiseScale; 
                float noiseZ = (normZ + offsetZ) * _perlinNoiseScale;
                float noiseValue = Mathf.PerlinNoise(noiseX, noiseZ);

                // 2. Если шум НЕ превышает порог — пропускаем
                if (noiseValue <= _perlinNoiseThreshold) continue;

                // 3. Определяем биом в этой точке по карте Вороного
                int alphaZ = Mathf.FloorToInt(normZ * alphaRes);
                int alphaX = Mathf.FloorToInt(normX * alphaRes);
                alphaZ = Mathf.Clamp(alphaZ, 0, alphaRes - 1);
                alphaX = Mathf.Clamp(alphaX, 0, alphaRes - 1);

                // Находим доминирующий биом
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

                WorldObject spawnObject = ChoseWoroldObject(biome);
                if (spawnObject.prefab == null) continue;

                Vector3 spawnPos = new Vector3(terrainTransform.position.x + x, 0, terrainTransform.position.z + z);
                Instantiate(spawnObject.prefab, spawnPos, Quaternion.identity, terrainTransform);
                }
            }  
        }
    
    private WorldObject ChoseWoroldObject(Biome biome)
    {
        WorldObject[] available = biome.worldObjects;
        if (available != null && available.Length > 0)
        {
            float totalWeight = 0f;
            foreach (var obj in available) totalWeight += obj.spawnChance;

            if (totalWeight > 0f) // если есть хоть какой-то вес
            {
                float randomPoint = Random.value * totalWeight;
                float cumulative = 0f;
                WorldObject chosen = null;
                foreach (var obj in available)
                {
                    cumulative += obj.spawnChance;
                    if (randomPoint <= cumulative)
                    {
                        chosen = obj;
                        return chosen;
                    }
                }
            }
        }
        return biome.worldObjects[0];
    }
}