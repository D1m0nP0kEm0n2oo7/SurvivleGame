using UnityEngine;

public class PlayerSaver : SaveableBehaviour
{
    [Header("Max stats")]
    [SerializeField] private int _maxHealth = 100;
    [SerializeField] private int _maxHunger = 100;
    [SerializeField] private int _maxThirst = 100;
    [SerializeField] private int _maxMind = 100;

    private int _health;
    private int _hunger;
    private int _thirst;
    private int _mind;

    public override int LoadPriority => 30;

    private void Awake()
    {
        _health = _maxHealth;
        _hunger = _maxHunger;
        _thirst = _maxThirst;
        _mind = _maxMind;
    }

    public override void Save(WorldData worldData)
    {
        Vector3 pos = transform.position;

        worldData.Character = new CharacterData
        {
            PosX = pos.x,
            PosY = pos.y,
            PosZ = pos.z,
            RotationY = transform.eulerAngles.y,

            MaxHealth = _maxHealth,
            MaxHunger = _maxHunger,
            MaxThirst = _maxThirst,
            MaxMind = _maxMind,

            Health = _health,
            Hunger = _hunger,
            Thirst = _thirst,
            Mind = _mind,
        };
    }

    public override void Load(WorldData worldData)
    {
        CharacterData c = worldData.Character;
        if (c == null) return;

        transform.position = new Vector3(c.PosX, c.PosY, c.PosZ);
        transform.rotation = Quaternion.Euler(0f, c.RotationY, 0f);

        _maxHealth = c.MaxHealth;
        _maxHunger = c.MaxHunger;
        _maxThirst = c.MaxThirst;
        _maxMind = c.MaxMind;

        _health = c.Health;
        _hunger = c.Hunger;
        _thirst = c.Thirst;
        _mind = c.Mind;
    }
}