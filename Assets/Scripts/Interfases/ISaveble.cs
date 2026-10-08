public interface ISaveable
{
    WorldData SaveState(WorldData data);
    void LoadState(WorldData data);
}
