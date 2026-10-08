using UnityEngine;

[System.Serializable]
public class WorldSettings
{
    public int Seed;
    public int TerrainSize;
    public int AlphamapResolution;
    public int NumCells;
    public float PerlinNoiseStep;
    public float PerlinNoiseScale;

    public static WorldSettings Create(WorldGenerationConfig c, int seed)
    {
        int size = c.TerrainSize > 0 ? c.TerrainSize : WorldGenerationConfig.BaseTerrainSize;
        float k = (float)size / WorldGenerationConfig.BaseTerrainSize;

        return new WorldSettings
        {
            Seed = seed,
            TerrainSize = size,
            AlphamapResolution = c.AlphamapResolution > 0 ? c.AlphamapResolution
                : Mathf.RoundToInt(WorldGenerationConfig.BaseAlphamapResolution * k),
            NumCells = c.NumCells > 0 ? c.NumCells
                : Mathf.RoundToInt(WorldGenerationConfig.BaseNumCells * k),
            PerlinNoiseStep = c.PerlinNoiseStep > 0 ? c.PerlinNoiseStep
                : WorldGenerationConfig.BasePerlinNoiseStep,
            PerlinNoiseScale = c.PerlinNoiseScale > 0 ? c.PerlinNoiseScale
                : WorldGenerationConfig.BasePerlinNoiseScale * k,
        };
    }
}