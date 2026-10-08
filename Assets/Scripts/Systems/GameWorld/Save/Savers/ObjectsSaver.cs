using System.Collections.Generic;
using UnityEngine;

public class ObjectsSaver : SaveableBehaviour
{
    [SerializeField] private WorldObjectSpawner _spawner;
    [SerializeField] private WorldGenerationConfig _config;

    private Dictionary<int, GameObject> _idToPrefab;

    public override int LoadPriority => 20;

    public override void Save(WorldData worldData)
    {
        Transform root = _spawner != null ? _spawner.Root : null;
        if (root == null)
        {
            worldData.Objects = System.Array.Empty<ObjectData>();
            return;
        }

        var list = new List<ObjectData>();
        foreach (SpawnedObject s in root.GetComponentsInChildren<SpawnedObject>(true))
        {
            Transform t = s.transform;
            list.Add(new ObjectData
            {
                ID = s.PrefabId,
                PosX = t.position.x,
                PosZ = t.position.z,
                RotationY = t.eulerAngles.y,
            });
        }

        worldData.Objects = list.ToArray();
    }

    public override void Load(WorldData worldData)
    {
        if (_spawner == null) return;

        _spawner.ClearSpawned();

        ObjectData[] objects = worldData.Objects;
        if (objects == null || objects.Length == 0) return;

        EnsureMap();

        Terrain terrain = _spawner.Terrain;
        Transform root = _spawner.Root;
        if (root == null || terrain == null) return;

        foreach (ObjectData od in objects)
        {
            if (!_idToPrefab.TryGetValue(od.ID, out GameObject prefab) || prefab == null)
                continue;

            var pos = new Vector3(od.PosX, 0f, od.PosZ);
            pos.y = terrain.SampleHeight(pos) + terrain.transform.position.y;

            GameObject go = Instantiate(
                prefab, pos, Quaternion.Euler(0f, od.RotationY, 0f), root);

            SpawnedObject so = go.GetComponent<SpawnedObject>()
                            ?? go.AddComponent<SpawnedObject>();
            so.PrefabId = od.ID;
        }
    }

    public static int PrefabId(GameObject prefab) => prefab.name.GetHashCode();

    private void EnsureMap()
    {
        if (_idToPrefab != null) return;
        _idToPrefab = new Dictionary<int, GameObject>();

        if (_config == null || _config.Biomes == null || _config.Biomes.Biomes == null)
            return;

        foreach (Biome biome in _config.Biomes.Biomes)
        {
            if (biome.worldObjects == null) continue;
            foreach (WorldObject wo in biome.worldObjects)
            {
                if (wo.prefab == null) continue;
                int id = PrefabId(wo.prefab);
                if (!_idToPrefab.ContainsKey(id))
                    _idToPrefab[id] = wo.prefab;
            }
        }
    }
}