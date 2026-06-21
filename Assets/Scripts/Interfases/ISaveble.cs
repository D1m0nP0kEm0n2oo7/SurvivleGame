public interface ISaveble
{
    void SaveState(WorldData data);
    void LoadState(WorldData data);
    int LoadPriority { get; }
}
