using UnityEngine;

[CreateAssetMenu(menuName = "World/Generation Config")]
public class WorldGenerationConfig : ScriptableObject
{
    public int TerrainSize;          
    public int AlphamapResolution;
    public int NumCells;
    public float PerlinNoiseStep;
    public float PerlinNoiseScale;
    public BiomeDatabase Biomes;

    public const int BaseTerrainSize = 200;
    public const int BaseAlphamapResolution = 512;
    public const int BaseNumCells = 20;
    public const float BasePerlinNoiseStep = 2f;
    public const float BasePerlinNoiseScale = 35f;
}
