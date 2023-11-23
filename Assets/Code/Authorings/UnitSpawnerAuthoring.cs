using Unity.Entities;
using UnityEngine;

public class UnitSpawnerAuthoring : MonoBehaviour
{
    [SerializeField]
    private GameObject unitPrefab;

    class Baker : Baker<UnitSpawnerAuthoring>
    {
        public override void Bake(UnitSpawnerAuthoring authoring)
        {
            this.AddComponent(this.GetEntity(TransformUsageFlags.Dynamic), new UnitSpawner()
            {
                Prefab = this.GetEntity(authoring.unitPrefab, TransformUsageFlags.Dynamic)
            });
        }
    }
}
