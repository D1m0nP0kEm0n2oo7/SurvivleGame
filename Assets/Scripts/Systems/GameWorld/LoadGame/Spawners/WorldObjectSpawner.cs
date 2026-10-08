using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class WorldObjectSpawner : MonoBehaviour
{   
    private const int SpawnSeedSalt = 0x5F3759DF;

    private readonly List<GameObject> _candidates = new List<GameObject>();
    private Transform _root;

    public void SpawnWorldObjects(GeneratedWorld world, WorldSettings settings)
    {
        if (settings == null || world.Terrain == null)
            return;

        var rng = new System.Random(settings.Seed ^ SpawnSeedSalt);

        Terrain terrain = world.Terrain;
        float[,,] alphamap = world.Alphamap;
        Biome[] biomes = world.Biomes;

        int alphaRes = world.TerrainData.alphamapResolution;
        float terrainSize = world.TerrainData.size.x;
        Vector3 terrainPos = terrain.transform.position;

        float perlinNoiseStep = settings.PerlinNoiseStep;
        float perlinNoiseScale = settings.PerlinNoiseScale;

        float offsetX = (float)rng.NextDouble();
        float offsetZ = (float)rng.NextDouble();

        GameObject root = new GameObject("Root");

        RecreateRoot(terrain.transform);

        for (float x = 0; x < terrainSize; x += perlinNoiseStep)
        {
            for (float z = 0; z < terrainSize; z += perlinNoiseStep)
            {
                float normX = x / terrainSize;
                float normZ = z / terrainSize;

                // 1. Шум Перлина
                float noiseValue = Mathf.PerlinNoise(
                    (normX + offsetX) * perlinNoiseScale,
                    (normZ + offsetZ) * perlinNoiseScale);

                // 2. Доминирующий биом
                int alphaX = Mathf.Clamp(Mathf.FloorToInt(normX * alphaRes), 0, alphaRes - 1);
                int alphaZ = Mathf.Clamp(Mathf.FloorToInt(normZ * alphaRes), 0, alphaRes - 1);

                int dominantBiome = GetDominantBiome(alphamap, alphaZ, alphaX, biomes.Length);
                if (dominantBiome < 0) continue;

                Biome biome = biomes[dominantBiome];
                if (biome.worldObjects == null) continue;

                // 3. Кандидаты, прошедшие порог
                _candidates.Clear();
                foreach (WorldObject worldObj in biome.worldObjects)
                {
                    if (worldObj.prefab == null) continue;
                    if (noiseValue > 1f - worldObj.spawnChance)
                        _candidates.Add(worldObj.prefab);
                }
                if (_candidates.Count == 0) continue;

                // 4. Спавн одного случайного
                GameObject chosen = _candidates[rng.Next(_candidates.Count)];

                float posX = Mathf.Clamp(x + Range(rng, -0.5f, 0.5f), 0f, terrainSize);
                float posZ = Mathf.Clamp(z + Range(rng, -0.5f, 0.5f), 0f, terrainSize);
                float yaw = Range(rng, 0f, 360f);

                var worldPos = new Vector3(terrainPos.x + posX, 0f, terrainPos.z + posZ);
                worldPos.y = terrain.SampleHeight(worldPos) + terrainPos.y;

                Instantiate(chosen, worldPos, Quaternion.Euler(0f, yaw, 0f), _root);
            }
        }
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

    private void RecreateRoot(Transform parent)
    {
        if (_root != null)
            Destroy(_root.gameObject);

        _root = new GameObject("WorldObjects").transform;
        _root.SetParent(parent, false);
    }

    private static float Range(System.Random rng, float min, float max)
        => min + (float)rng.NextDouble() * (max - min);
}
