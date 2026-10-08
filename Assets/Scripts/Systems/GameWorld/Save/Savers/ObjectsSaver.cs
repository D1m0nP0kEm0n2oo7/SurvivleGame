using System.Collections.Generic;
using UnityEngine;

public class ObjectsSaver : SaveableBehaviour
{
    private Transform _root;

    public ObjectData[] Loaded { get; private set; }
    public bool HasSave => Loaded != null && Loaded.Length > 0;

    public void Bind(Transform root) => _root = root;

    public override void Load(WorldData data) => Loaded = data.Objects;

    public override void Save(WorldData data)
    {
        var list = new List<ObjectData>();

        if (_root != null)
        {
            foreach (SpawnedObject s in _root.GetComponentsInChildren<SpawnedObject>(true))
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
        }

        data.Objects = list.ToArray();
    }
}