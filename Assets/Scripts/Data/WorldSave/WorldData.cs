using UnityEngine.UI;

[System.Serializable]

public class WorldData
{
    public int Seed;
    public int TerrainSize;
    public int AlphamapResolution;
    public int NumCells;
    public float PerlinNoiseStep;
    public float PerlinNoiseScale;

    
    public int WorldTimeSec;
    public CharacterData CharacterData;
    public InventoryData InventoryData;
    public Object[] Objects;
}
