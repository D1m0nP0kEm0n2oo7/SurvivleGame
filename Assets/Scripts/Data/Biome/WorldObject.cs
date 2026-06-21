using UnityEngine;
[System.Serializable]
public class WorldObject 
{
    public GameObject prefab;
    [Range(0, 1)] public float spawnChance;
}
