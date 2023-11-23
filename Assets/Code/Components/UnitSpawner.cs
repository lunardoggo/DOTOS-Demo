using Unity.Entities;
using UnityEngine;

public struct UnitSpawner : IComponentData
{
    [field: SerializeField]
    public Entity Prefab { get; set; }
}
