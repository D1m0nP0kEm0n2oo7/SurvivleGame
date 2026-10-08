[System.Serializable]
public class WorldData
{
    public int Version = 1;
    public WorldSettings World;
    public int WorldTimeSec;
    public CharacterData Character;
    public InventoryData Inventory;
    public ObjectData[] Objects;
}