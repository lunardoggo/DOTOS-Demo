using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

[UpdateInGroup(typeof(GhostSimulationSystemGroup))]
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
public partial struct ClientUnitMovementSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<RNG>();
    }

    public void OnUpdate(ref SystemState state)
    {
        NetworkTick serverTick = SystemAPI.GetSingleton<NetworkTime>().ServerTick;
        float deltaTime = SystemAPI.Time.DeltaTime;

        foreach (Entity entity in state.GetEntityQuery(typeof(Unit), typeof(PredictedGhost)).ToEntityArray(Allocator.Temp))
        {
            if (state.EntityManager.GetComponentData<PredictedGhost>(entity).ShouldPredict(serverTick))
            {
                UnitAspect unit = state.EntityManager.GetAspect<UnitAspect>(entity);
                if (!unit.IsTargetReached)
                {
                    unit.MoveTowardsTarget(deltaTime);
                }
            }
        }
    }
}
