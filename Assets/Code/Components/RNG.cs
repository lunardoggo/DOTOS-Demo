using Unity.Mathematics;
using Unity.Entities;

public struct RNG : IComponentData
{
    public Random Random { get; set; }
}
