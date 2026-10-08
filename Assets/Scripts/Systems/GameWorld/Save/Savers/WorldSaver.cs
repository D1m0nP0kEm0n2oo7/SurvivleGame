using UnityEngine;

public class WorldSaver : SaveableBehaviour
{
    [SerializeField] private WorldTime _worldTime = new WorldTime();

    /// <summary>Время мира. Доступно другим системам (тик времени и т.п.).</summary>
    public WorldTime Time => _worldTime;

    public override int LoadPriority => 10;

    public override void Save(WorldData worldData)
    {
        worldData.WorldTimeSec = _worldTime.seconds;
    }

    public override void Load(WorldData worldData)
    {
        _worldTime.seconds = worldData.WorldTimeSec;
    }
}