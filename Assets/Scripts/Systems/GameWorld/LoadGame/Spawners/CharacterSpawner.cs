using UnityEngine;

public class CharacterSpawner : MonoBehaviour
{
    [SerializeField] private GameObject _characterPrefab;
    [SerializeField] private PlayerSaver _saver;

    public GameObject Spawn(GeneratedWorld world)
    {
        if (!world.IsValid)
            return null;

        Vector3 pos;
        Quaternion rot;

        if (_saver.HasSave)
        {
            CharacterData c = _saver.Loaded;
            pos = new Vector3(c.PosX, c.PosY, c.PosZ);
            rot = Quaternion.Euler(0f, c.RotationY, 0f);
        }
        else
        {
            pos = GetDefaultPosition(world.Terrain);
            rot = Quaternion.identity;
        }

        // Позиция задаётся при создании, чтобы CharacterController/Rigidbody не "перепрыгивали" после Instantiate
        GameObject go = Instantiate(_characterPrefab, pos, rot);

        PlayerStats stats = go.GetComponent<PlayerStats>();
        if (stats != null && _saver.HasSave)
            stats.Apply(_saver.Loaded);

        _saver.Bind(go.transform, stats);
        return go;
    }

    private static Vector3 GetDefaultPosition(Terrain terrain)
    {
        Vector3 origin = terrain.transform.position;
        float half = terrain.terrainData.size.x * 0.5f;

        var pos = new Vector3(origin.x + half, 0f, origin.z + half);
        pos.y = terrain.SampleHeight(pos) + origin.y + 1f;
        return pos;
    }
}