using Unity.Mathematics;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

[UpdateInGroup(typeof(GhostSimulationSystemGroup))]
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
public partial struct ServerUnitMovementSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<RNG>();
    }

    public void OnUpdate(ref SystemState state)
    {
        RefRW<RNG> rng = SystemAPI.GetSingletonRW<RNG>();
        float deltaTime = SystemAPI.Time.DeltaTime;
        Random random = rng.ValueRW.Random;

        foreach (Entity entity in state.GetEntityQuery(typeof(Unit)).ToEntityArray(Allocator.Temp))
        {
            UnitAspect unit = state.EntityManager.GetAspect<UnitAspect>(entity);
            if (unit.IsTargetReached)
            {
                //TODO: use maths random
                float3 target = new float3(UnityEngine.Random.Range(-50.0f, 50.0f), 0, UnityEngine.Random.Range(-50.0f, 50.0f));
                // float speed = UnityEngine.Random.Range(1.0f, 10.0f);
                unit.SetNextTarget(target);
                // unit.SetSpeed(speed);
            }
            else
            {
                unit.MoveTowardsTarget(deltaTime);
            }
        }
    }
}
