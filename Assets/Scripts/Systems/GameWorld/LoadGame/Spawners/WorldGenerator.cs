using UnityEngine;

public class TerrainGenerator : MonoBehaviour, ISaveable
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

    public Terrain Terrain => _terrain;
    public TerrainData TerrainData => _terrainData;
    public float[,,] Alphamap => _alphamap;
    public Biome[] Biomes => _biomes.Biomes;
    public float PerlinNoiseStep => _perlinNoiseStep;
    public float PerlinNoiseScale => _perlinNoiseScale;
    public int Seed => _seed;

    public void Init()
    {
        GenerateTerrain();
    }

    /// <summary>
    /// For world scaling, if generation parameters are not specified, the system calculates a multiplier based on a base value and scales the generation parameters linearly. 
    /// </summary>
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

    /// <summary>
    /// Generation world
    /// </summary>
    private void GenerateTerrain()
    {
        WorldScaling();

        if (_seed == 0)
        {
            _seed = Random.Range(int.MinValue, int.MaxValue);
            Debug.Log($"seed = {_seed}");
        }

        if (_terrain == null)
            _terrain = GetComponentInChildren<Terrain>();

        if (_terrain == null)
        {
            GameObject terrainGO = Terrain.CreateTerrainGameObject(new TerrainData());
            _terrain = terrainGO.GetComponent<Terrain>();
            _terrain.transform.parent = transform;
            _terrain.name = "GeneratedTerrain";
        }
        _terrainTransform = _terrain.transform;

        for (int i = _terrainTransform.childCount - 1; i >= 0; i--)
            if (Application.isPlaying)
                Destroy(_terrainTransform.GetChild(i).gameObject);
            else
                DestroyImmediate(_terrainTransform.GetChild(i).gameObject);

        _terrainData = new TerrainData();
        _terrainData.alphamapResolution = _alphamapResolution;
        _terrainData.size = new Vector3(_terrainSize, 0, _terrainSize);
        _terrain.terrainData = _terrainData;

        var terrainCollider = _terrain.GetComponent<TerrainCollider>();
        if (terrainCollider != null)
            terrainCollider.terrainData = _terrainData;

        _defaultState = Random.state;
        Random.InitState(_seed);

        _terrainData.terrainLayers = GetTerrainLayersFromBiomes();

        _alphamap = GetAlphaMap();
        ApplyAlphamap();

        Random.state = _defaultState;

    }

    private void ApplyAlphamap()
    {
        _terrainData.SetAlphamaps(0, 0, _alphamap);
        _terrainData.SetBaseMapDirty();
        _terrainData.SyncTexture(TerrainData.AlphamapTextureName);
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
            else
            {
                layers[i] = _biomes.Biomes[i].terrainLayer;
            }
        }
        return layers;
    }

    private float[,,] GetAlphaMap()
    {
        int alphaRes = _terrainData.alphamapResolution;
        float[,,] alphamap = new float[alphaRes, alphaRes, _biomes.Biomes.Length];

        Vector2[] cellCenters = new Vector2[_numCells];
        int[] cellBiomes = new int[_numCells];
        for (int i = 0; i < _numCells; i++)
        {
            cellCenters[i] = new Vector2(Random.value, Random.value);
            cellBiomes[i] = Random.Range(0, _biomes.Biomes.Length);
        }

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
                    float dist = dx * dx + dy * dy;

                    if (dist < minDist)
                    {
                        minDist = dist;
                        closestBiome = cellBiomes[i];
                    }
                }

                for (int i = 0; i < _biomes.Biomes.Length; i++)
                    alphamap[z, x, i] = (i == closestBiome) ? 1f : 0f;
            }
        }

        return alphamap;
    }

    public WorldData SaveState(WorldData data)
    {
        data.TerrainSize = _terrainSize;
        data.AlphamapResolution = _alphamapResolution;
        data.NumCells = _numCells;
        data.PerlinNoiseStep = _perlinNoiseStep;
        data.PerlinNoiseScale = _perlinNoiseScale;
        data.Seed = _seed;

        return data;
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
}