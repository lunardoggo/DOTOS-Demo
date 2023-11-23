using Unity.Entities;
using UnityEngine;

[DisallowMultipleComponent]
public class JoinGameAuthoring : MonoBehaviour
{
    class Baker : Baker<JoinGameAuthoring>
    {
        public override void Bake(JoinGameAuthoring authoring)
        {
            Entity entity = this.GetEntity(TransformUsageFlags.Dynamic);
            this.AddComponent(entity, new JoinGame());
        }
    }
}
