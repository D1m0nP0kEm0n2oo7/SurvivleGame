using UnityEngine;

[CreateAssetMenu(fileName = "NewBiome", menuName = "World/Biome")]
public class Biome : ScriptableObject 
{
    public string biomeName;
    public TerrainLayer terrainLayer;
    public WorldObject[] worldObjects;
}
