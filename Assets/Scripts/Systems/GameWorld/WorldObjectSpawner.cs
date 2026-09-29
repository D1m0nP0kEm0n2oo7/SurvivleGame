using System.Collections.Generic;
using UnityEngine;

public class WorldObjectSpawner : MonoBehaviour
{
    private List<GameObject> _candidates = new List<GameObject>();

    public void SpawnWorldObjects(Terrain terrain, 
        TerrainData terrainData, 
        float[,,] alphamap,
        Biome[] biomes, 
        float perlinNoiseStep, 
        float perlinNoiseScale)
    {
        if (terrain == null || terrainData == null || alphamap == null) return;

        
        int alphaRes = terrainData.alphamapResolution;
        float terrainSize = terrainData.size.x;
        Transform terrainTransform = terrain.transform;

        float offsetX = Random.Range(0f, 1f);
        float offsetZ = Random.Range(0f, 1f);
        Debug.Log($"offsetX = {offsetX}, offsetZ = {offsetZ}");

        GameObject root = new GameObject("Root");
        root.transform.SetParent(terrainTransform);

        for (float x = 0; x < terrainSize; x += perlinNoiseStep)
        {
            for (float z = 0; z < terrainSize; z += perlinNoiseStep)
            {
                // 1. Шум Перлина
                float normX = x / terrainSize;
                float normZ = z / terrainSize;

                float noiseX = (normX + offsetX) * perlinNoiseScale;
                float noiseZ = (normZ + offsetZ) * perlinNoiseScale;
                float noiseValue = Mathf.PerlinNoise(noiseX, noiseZ);

                // 2. Биом по Вороному
                int alphaZ = Mathf.FloorToInt(normZ * alphaRes);
                int alphaX = Mathf.FloorToInt(normX * alphaRes);
                alphaZ = Mathf.Clamp(alphaZ, 0, alphaRes - 1);
                alphaX = Mathf.Clamp(alphaX, 0, alphaRes - 1);

                float maxWeight = 0f;
                int dominantBiome = -1;
                for (int b = 0; b < biomes.Length; b++)
                {
                    float weight = alphamap[alphaZ, alphaX, b];
                    if (weight > maxWeight)
                    {
                        maxWeight = weight;
                        dominantBiome = b;
                    }
                }
                if (dominantBiome < 0) continue;

                Biome biome = biomes[dominantBiome];
                if (biome.worldObjects == null) continue;

                // 3. Собираем объекты, прошедшие персональный порог
                _candidates.Clear();
                foreach (WorldObject worldObj in biome.worldObjects)
                {
                    if (worldObj.prefab == null) continue;

                    float threshold = 1f - worldObj.spawnChance;
                    if (noiseValue > threshold)
                    {
                        _candidates.Add(worldObj.prefab);
                    }
                }

                // 4. Если есть кандидаты — спавним одного случайного
                if (_candidates.Count > 0)
                {
                    GameObject chosen = _candidates[Random.Range(0, _candidates.Count)];
                    Vector3 spawnPos = new Vector3(
                        terrainTransform.position.x + x + Random.Range(-0.5f, 0.5f),
                        0,
                        terrainTransform.position.z + z + Random.Range(-0.5f, 0.5f));
                    Instantiate(chosen, spawnPos, Quaternion.Euler(0, Random.Range(-0, 180), 0), root.transform);
                }
            }
        }
    } 
}
