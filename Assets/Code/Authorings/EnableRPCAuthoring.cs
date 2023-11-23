using Unity.Entities;
using UnityEngine;

[DisallowMultipleComponent]
public class EnableRPCAuthoring : MonoBehaviour
{
    class Baker : Baker<EnableRPCAuthoring>
    {
        public override void Bake(EnableRPCAuthoring authoring)
        {
            Entity entity = this.GetEntity(TransformUsageFlags.Dynamic);
            this.AddComponent(entity, new EnableRPC());
        }
    }
}
