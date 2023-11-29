using Unity.Mathematics;
using Unity.Collections;
using Unity.Entities;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
public partial struct UnitSpawningSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<UnitSpawner>();
        state.RequireForUpdate<RNG>();
    }

    public void OnUpdate(ref SystemState state)
    {
        NativeArray<Entity> commands = state.GetEntityQuery(typeof(SpawnUnitsCommand)).ToEntityArray(Allocator.Temp);
        UnitSpawner spawner = state.GetEntityQuery(typeof(UnitSpawner)).GetSingleton<UnitSpawner>();
        EntityCommandBuffer buffer = new EntityCommandBuffer(Allocator.Temp);
        RefRW<RNG> rng = SystemAPI.GetSingletonRW<RNG>();
        Random random = rng.ValueRW.Random;

        foreach(Entity command in commands)
        {
            buffer.DestroyEntity(command);

            SpawnUnitsCommand cmd = state.EntityManager.GetComponentData<SpawnUnitsCommand>(command);

            //Here one could check if the user is allowed to spawn more units
            for(int i = 0; i < cmd.Count; i++)
            {
                //TODO: use maths random
                float3 target = new float3(UnityEngine.Random.Range(-50.0f, 50.0f), 0, UnityEngine.Random.Range(-50.0f, 50.0f));
                Entity unit = buffer.Instantiate(spawner.Prefab);
                buffer.SetComponent(unit, new Unit() {
                    Speed = UnityEngine.Random.Range(1.0f, 5.0f),
                    PlayerId = cmd.PlayerId,
                    IsSelected = false,
                    Target = target
                });
            }
        }

        buffer.Playback(state.EntityManager);
    }
}
