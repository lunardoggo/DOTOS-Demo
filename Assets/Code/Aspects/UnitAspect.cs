using Unity.Mathematics;
using Unity.Transforms;
using Unity.Entities;

public readonly partial struct UnitAspect : IAspect
{
    private readonly RefRW<LocalTransform> transform;
    private readonly RefRW<Unit> unit;

    public void MoveTowardsTarget(float deltaTime)
    {
        float3 offset = this.unit.ValueRO.Target - this.transform.ValueRO.Position;
        float3 movement = math.normalizesafe(offset) * unit.ValueRO.Speed * deltaTime;

        if (math.length(offset) <= math.length(movement))
        {
            this.transform.ValueRW.Position = this.unit.ValueRO.Target;
        }
        else
        {
            this.transform.ValueRW.Position += movement;
        }
    }

    public void SetSpeed(float speed)
    {
        this.unit.ValueRW.Speed = speed;
    }

    public void SetTarget(float3 target)
    {
        this.unit.ValueRW.Target = target;
    }

    public bool IsTargetReached
    {
        get => math.all(this.unit.ValueRO.Target == this.transform.ValueRO.Position);
    }
}
