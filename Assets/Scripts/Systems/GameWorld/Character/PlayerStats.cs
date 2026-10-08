using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Max stats")]
    [SerializeField] private int _maxHealth = 100;
    [SerializeField] private int _maxHunger = 100;
    [SerializeField] private int _maxThirst = 100;
    [SerializeField] private int _maxMind = 100;

    public int Health { get; private set; }
    public int Hunger { get; private set; }
    public int Thirst { get; private set; }
    public int Mind { get; private set; }

    public int MaxHealth => _maxHealth;
    public int MaxHunger => _maxHunger;
    public int MaxThirst => _maxThirst;
    public int MaxMind => _maxMind;

    private void Awake()
    {
        Health = _maxHealth;
        Hunger = _maxHunger;
        Thirst = _maxThirst;
        Mind = _maxMind;
    }

    public void Apply(CharacterData c)
    {
        _maxHealth = c.MaxHealth;
        _maxHunger = c.MaxHunger;
        _maxThirst = c.MaxThirst;
        _maxMind = c.MaxMind;

        Health = c.Health;
        Hunger = c.Hunger;
        Thirst = c.Thirst;
        Mind = c.Mind;
    }

    public void WriteTo(CharacterData c)
    {
        c.MaxHealth = _maxHealth;
        c.MaxHunger = _maxHunger;
        c.MaxThirst = _maxThirst;
        c.MaxMind = _maxMind;

        c.Health = Health;
        c.Hunger = Hunger;
        c.Thirst = Thirst;
        c.Mind = Mind;
    }
}