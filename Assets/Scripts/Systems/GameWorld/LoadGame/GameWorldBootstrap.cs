using UnityEngine;

public class GameWorldBootstrap : MonoBehaviour
{
    [SerializeField] private TerrainGenerator _terrainGenerator;
    [SerializeField] private WorldObjectSpawner _worldObjectSpawner;
    [SerializeField] private CharacterSpawner _characterSpawner;
    private void Awake()
    {
        _terrainGenerator.Init();
        _worldObjectSpawner.SpawnWorldObjects(
            _terrainGenerator.Terrain,
            _terrainGenerator.TerrainData,
            _terrainGenerator.Alphamap,
            _terrainGenerator.Biomes,
            _terrainGenerator.PerlinNoiseStep,
            _terrainGenerator.PerlinNoiseScale,
            _terrainGenerator.Seed
            );
        _characterSpawner.SpawnCharacter(_terrainGenerator.Terrain);
    }
}
