public interface ISaveable
{
    int LoadPriority { get; } 
    void Save(WorldData data);
    void Load(WorldData data);
}
