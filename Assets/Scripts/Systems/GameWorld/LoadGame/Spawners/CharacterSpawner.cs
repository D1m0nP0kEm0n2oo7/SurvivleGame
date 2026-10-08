using UnityEngine;

public class CharacterSpawner : MonoBehaviour
{
    [SerializeField] private GameObject _character;

    public GameObject SpawnCharacter(Terrain terrain)
    {
        Vector3 origin = terrain.transform.position;
        float half = terrain.terrainData.size.x * 0.5f;

        var pos = new Vector3(origin.x + half, 0f, origin.z + half);
        pos.y = terrain.SampleHeight(pos) + origin.y + 1f;

        return Instantiate(_character, pos, Quaternion.identity);
    }
}