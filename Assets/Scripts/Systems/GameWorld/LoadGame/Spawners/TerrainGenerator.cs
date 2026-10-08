using UnityEngine;

public readonly struct GeneratedWorld
{
    public readonly Terrain Terrain;
    public readonly TerrainData TerrainData;
    public readonly float[,,] Alphamap;
    public readonly Biome[] Biomes;

    public GeneratedWorld(Terrain terrain, TerrainData terrainData, float[,,] alphamap, Biome[] biomes)
    {
        Terrain = terrain;
        TerrainData = terrainData;
        Alphamap = alphamap;
        Biomes = biomes;
    }
}

public class TerrainGenerator : MonoBehaviour
{
    private Terrain _terrain;
    private TerrainData _ownedData; 

    public GeneratedWorld Generate(WorldSettings settings, WorldGenerationConfig config)
    {
        Biome[] biomes = config.Biomes.Biomes;
        if (biomes == null || biomes.Length == 0)
        {
            Debug.LogError("BiomeDatabase пуст, генерация невозможна");
            return default;
        }

        var rng = new System.Random(settings.Seed);

        Terrain terrain = GetOrCreateTerrain();
        ClearChildren(terrain.transform);

        TerrainData data = CreateTerrainData(settings, biomes, terrain);
        float[,,] alphamap = BuildAlphamap(data.alphamapResolution, settings.NumCells, biomes.Length, rng);
        ApplyAlphamap(data, alphamap);

        return new GeneratedWorld(terrain, data, alphamap, biomes);
    }

    private Terrain GetOrCreateTerrain()
    {
        if (_terrain != null)
            return _terrain;

        _terrain = GetComponentInChildren<Terrain>();
        if (_terrain != null)
            return _terrain;

        var initial = new TerrainData();
        GameObject go = Terrain.CreateTerrainGameObject(initial);
        _ownedData = initial;

        go.name = "GeneratedTerrain";
        go.transform.SetParent(transform);
        _terrain = go.GetComponent<Terrain>();
        return _terrain;
    }

    private static void ClearChildren(Transform root)
    {
        for (int i = root.childCount - 1; i >= 0; i--)
        {
            GameObject child = root.GetChild(i).gameObject;
            if (Application.isPlaying) Destroy(child);
            else DestroyImmediate(child);
        }
    }

    private TerrainData CreateTerrainData(WorldSettings settings, Biome[] biomes, Terrain terrain)
    {
        var data = new TerrainData();
        data.alphamapResolution = settings.AlphamapResolution;
        data.size = new Vector3(settings.TerrainSize, 0, settings.TerrainSize);
        data.terrainLayers = GetTerrainLayers(biomes);

        terrain.terrainData = data;

        var terrainCollider = terrain.GetComponent<TerrainCollider>();
        if (terrainCollider != null)
            terrainCollider.terrainData = data;

        // старые данные больше не нужны, иначе утечка при повторной генерации
        if (_ownedData != null)
            Destroy(_ownedData);
        _ownedData = data;

        return data;
    }

    private static TerrainLayer[] GetTerrainLayers(Biome[] biomes)
    {
        var layers = new TerrainLayer[biomes.Length];
        for (int i = 0; i < biomes.Length; i++)
        {
            if (biomes[i].terrainLayer == null)
            {
                Debug.LogWarning($"Biome '{biomes[i].biomeName}' не имеет TerrainLayer!");
                layers[i] = new TerrainLayer();
            }
            else
            {
                layers[i] = biomes[i].terrainLayer;
            }
        }
        return layers;
    }

    private static float[,,] BuildAlphamap(int res, int numCells, int biomeCount, System.Random rng)
    {
        var alphamap = new float[res, res, biomeCount];

        var centers = new Vector2[numCells];
        var cellBiomes = new int[numCells];
        for (int i = 0; i < numCells; i++)
        {
            centers[i] = new Vector2((float)rng.NextDouble(), (float)rng.NextDouble());
            cellBiomes[i] = rng.Next(0, biomeCount);
        }

        for (int z = 0; z < res; z++)
        {
            float normZ = (float)z / res;
            for (int x = 0; x < res; x++)
            {
                float normX = (float)x / res;

                float minDist = float.MaxValue;
                int closest = 0;
                for (int i = 0; i < numCells; i++)
                {
                    float dx = normX - centers[i].x;
                    float dy = normZ - centers[i].y;
                    float dist = dx * dx + dy * dy;
                    if (dist < minDist)
                    {
                        minDist = dist;
                        closest = cellBiomes[i];
                    }
                }

                alphamap[z, x, closest] = 1f; // остальные слои уже 0
            }
        }
        return alphamap;
    }

    private static void ApplyAlphamap(TerrainData data, float[,,] alphamap)
    {
        data.SetAlphamaps(0, 0, alphamap);
        data.SetBaseMapDirty();
        data.SyncTexture(TerrainData.AlphamapTextureName);
    }
}