using UnityEngine;

public class WorldSaver : SaveableBehaviour
{
    [SerializeField] private WorldGenerationConfig _config;

    public WorldSettings Settings { get; private set; }
    public WorldTime Time { get; } = new WorldTime();

    public override void Load(WorldData d)
    {
        bool hasWorld = d.World != null && d.World.TerrainSize > 0;
        Settings = hasWorld
            ? d.World
            : WorldSettings.Create(_config, Random.Range(int.MinValue, int.MaxValue));
        Time.seconds = d.WorldTimeSec;
    }

    public override void Save(WorldData d)
    {
        d.World = Settings;
        d.WorldTimeSec = Time.seconds;
    }
}