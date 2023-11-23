using Unity.Mathematics;
using Unity.Entities;
using UnityEngine;

public struct Unit : IComponentData
{
    [field: SerializeField]
    public int PlayerId { get; set; }
    [field: SerializeField]
    public bool IsSelected { get; set; }
    [field: SerializeField]
    public float3 Target { get; set; }
    [field: SerializeField]
    public float Speed { get; set; }
}
