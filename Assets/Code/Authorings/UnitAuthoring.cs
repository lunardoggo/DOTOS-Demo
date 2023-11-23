using Unity.Entities;
using UnityEngine;

public class UnitAuthoring : MonoBehaviour
{
    class Baker : Baker<UnitAuthoring>
    {
        public override void Bake(UnitAuthoring authoring)
        {
            this.AddComponent<Unit>(this.GetEntity(TransformUsageFlags.Dynamic));
        }
    }
}
