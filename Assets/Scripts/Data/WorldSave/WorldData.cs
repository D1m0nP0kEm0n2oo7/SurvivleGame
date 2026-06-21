[System.Serializable]

public class WorldData
{
    public int Seed;
    public float[,,] AlphaMap;
    public int WorldTimeSec;
    public CharacterData CharacterData;
    public InventoryData InventoryData;
    public Object[] Objects;
}
