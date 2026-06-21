using System.Collections.Generic;
using UnityEngine;

public class WorldObjectSpawner : MonoBehaviour
{

    [SerializeField] private float _perlinNoiseStep = 2.0f;
    [SerializeField] private float _perlinNoiseScale = 35f;

    public void Spawn(Terrain terrain, float[,,] alphamap, Biome[] biomes)
    {
        SpawnWorldObjects(terrain, terrain.terrainData)
    }

    private void SpawnWorldObjects(Terrain terrain, TerrainData terrainData, )
    {
        if (terrain == null || terrainData == null) return;

        int alphaRes = terrainData.alphamapResolution;
        float[,,] alphamap = _alphamap;
        float terrainSize = terrainSize;
        Transform terrainTransform = terrainTransform;

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
                    Instantiate(chosen, spawnPos, Quaternion.Euler(0, Random.Range(-0, 180), 0), terrainTransform);
                }
            }
        }
    }
