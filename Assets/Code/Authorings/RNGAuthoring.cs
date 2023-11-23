using Unity.Entities;
using UnityEngine;
using System;

[DisallowMultipleComponent]
public class RNGAuthoring : MonoBehaviour
{
    class Baker : Baker<RNGAuthoring>
    {
        public override void Bake(RNGAuthoring authoring)
        {
            uint seed = (uint)UnityEngine.Random.Range(0, Int32.MaxValue);
            this.AddComponent(this.GetEntity(TransformUsageFlags.Dynamic), new RNG() {
                Random = new Unity.Mathematics.Random(seed)
            });
        }
    }
}
