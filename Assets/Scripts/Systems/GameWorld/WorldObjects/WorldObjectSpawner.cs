using System.Collections.Generic;
using UnityEngine;

public class WorldObjectSpawner : MonoBehaviour
{
    private const int SpawnSeedSalt = 0x5F3759DF;

    [SerializeField] private WorldSaver _world;
    [SerializeField] private ObjectsSaver _saver;

    private readonly List<GameObject> _candidates = new List<GameObject>();
    private readonly Dictionary<int, GameObject> _idToPrefab = new Dictionary<int, GameObject>();
    private Transform _root;

    /// <summary>Единый способ получить ID префаба (совместим со старыми сохранениями).</summary>
    public static int PrefabId(GameObject prefab) => prefab.name.GetHashCode();

    public void Spawn(GeneratedWorld world)
    {
        if (!world.IsValid)
            return;

        BuildPrefabMap(world.Biomes);
        RecreateRoot(world.Terrain.transform);
        _saver.Bind(_root);

        if (_saver.HasSave)
            SpawnFromSave(world.Terrain, _saver.Loaded);
        else
            SpawnGenerated(world, _world.Settings);
    }

    private void SpawnFromSave(Terrain terrain, ObjectData[] objects)
    {
        foreach (ObjectData od in objects)
        {
            if (!_idToPrefab.TryGetValue(od.ID, out GameObject prefab))
                continue;

            Place(terrain, prefab, od.PosX, od.PosZ, od.RotationY);
        }
    }

    private void SpawnGenerated(GeneratedWorld world, WorldSettings settings)
    {
        Terrain terrain = world.Terrain;
        float[,,] alphamap = world.Alphamap;
        Biome[] biomes = world.Biomes;

        int alphaRes = world.TerrainData.alphamapResolution;
        float terrainSize = world.TerrainData.size.x;
        Vector3 terrainPos = terrain.transform.position;

        float step = settings.PerlinNoiseStep;
        float scale = settings.PerlinNoiseScale;

        var rng = new System.Random(settings.Seed ^ SpawnSeedSalt);
        float offsetX = (float)rng.NextDouble();
        float offsetZ = (float)rng.NextDouble();

        for (float x = 0; x < terrainSize; x += step)
        {
            for (float z = 0; z < terrainSize; z += step)
            {
                float normX = x / terrainSize;
                float normZ = z / terrainSize;

                float noise = Mathf.PerlinNoise(
                    (normX + offsetX) * scale,
                    (normZ + offsetZ) * scale);

                int alphaX = Mathf.Clamp(Mathf.FloorToInt(normX * alphaRes), 0, alphaRes - 1);
                int alphaZ = Mathf.Clamp(Mathf.FloorToInt(normZ * alphaRes), 0, alphaRes - 1);

                int dominant = GetDominantBiome(alphamap, alphaZ, alphaX, biomes.Length);
                if (dominant < 0) continue;

                WorldObject[] objs = biomes[dominant].worldObjects;
                if (objs == null) continue;

                _candidates.Clear();
                foreach (WorldObject wo in objs)
                {
                    if (wo.prefab != null && noise > 1f - wo.spawnChance)
                        _candidates.Add(wo.prefab);
                }
                if (_candidates.Count == 0) continue;

                GameObject chosen = _candidates[rng.Next(_candidates.Count)];

                float posX = Mathf.Clamp(x + Range(rng, -0.5f, 0.5f), 0f, terrainSize);
                float posZ = Mathf.Clamp(z + Range(rng, -0.5f, 0.5f), 0f, terrainSize);
                float yaw = Range(rng, 0f, 360f);

                Place(terrain, chosen, terrainPos.x + posX, terrainPos.z + posZ, yaw);
            }
        }
    }
    private void Place(Terrain terrain, GameObject prefab, float worldX, float worldZ, float yaw)
    {
        var pos = new Vector3(worldX, 0f, worldZ);
        pos.y = terrain.SampleHeight(pos) + terrain.transform.position.y;

        GameObject go = Instantiate(prefab, pos, Quaternion.Euler(0f, yaw, 0f), _root);

        SpawnedObject so = go.GetComponent<SpawnedObject>();
        if (so == null) so = go.AddComponent<SpawnedObject>();
        so.PrefabId = PrefabId(prefab);
    }

    private void BuildPrefabMap(Biome[] biomes)
    {
        _idToPrefab.Clear();
        foreach (Biome biome in biomes)
        {
            if (biome.worldObjects == null) continue;
            foreach (WorldObject wo in biome.worldObjects)
            {
                if (wo.prefab == null) continue;
                _idToPrefab.TryAdd(PrefabId(wo.prefab), wo.prefab);
            }
        }
    }

    private void RecreateRoot(Transform parent)
    {
        if (_root != null)
            Destroy(_root.gameObject);

        _root = new GameObject("WorldObjects").transform;
        _root.SetParent(parent, false);
    }

    private static int GetDominantBiome(float[,,] alphamap, int z, int x, int biomeCount)
    {
        float maxWeight = 0f;
        int dominant = -1;
        for (int b = 0; b < biomeCount; b++)
        {
            float w = alphamap[z, x, b];
            if (w > maxWeight)
            {
                maxWeight = w;
                dominant = b;
            }
        }
        return dominant;
    }

    private static float Range(System.Random rng, float min, float max)
        => min + (float)rng.NextDouble() * (max - min);
}