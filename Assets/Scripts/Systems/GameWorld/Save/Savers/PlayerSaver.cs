using UnityEngine;

/// <summary>Хранит данные секции Character. Живёт в сцене, а не на префабе игрока.</summary>
public class PlayerSaver : SaveableBehaviour
{
    private Transform _character;
    private PlayerStats _stats;

    public CharacterData Loaded { get; private set; }
    public bool HasSave => Loaded != null && Loaded.MaxHealth > 0;

    /// <summary>Спавнер вызывает после создания персонажа.</summary>
    public void Bind(Transform character, PlayerStats stats)
    {
        _character = character;
        _stats = stats;
    }

    public override void Load(WorldData data) => Loaded = data.Character;

    public override void Save(WorldData data)
    {
        if (_character == null)
        {
            data.Character = Loaded;
            return;
        }

        Vector3 pos = _character.position;
        var c = new CharacterData
        {
            PosX = pos.x,
            PosY = pos.y,
            PosZ = pos.z,
            RotationY = _character.eulerAngles.y,
        };

        if (_stats != null)
            _stats.WriteTo(c);

        data.Character = c;
    }
}