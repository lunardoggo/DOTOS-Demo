using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation | WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
public partial class JoinGameSystem : SystemBase
{
    private EntityQuery newConnectionsQuery;

    protected override void OnCreate()
    {
        this.RequireForUpdate<JoinGame>();
        this.RequireForUpdate(this.newConnectionsQuery);
    }

    protected override void OnUpdate()
    {
        EntityCommandBuffer commands = new EntityCommandBuffer(Allocator.Temp);
        //world name has to be cached, as the world cannot be directly accessed inside the foreach block
        FixedString32Bytes world = World.Name;

        // Join the game as soon, as the connection is setup
        Entities.WithName("NewConnectionsGoInGame").WithStoreEntityQueryInField(ref newConnectionsQuery).WithNone<NetworkStreamInGame>().ForEach(
            (Entity entity, in NetworkId id) => 
            {
                Debug.Log($"[{world}] player with id {id.Value} joining the game...");
                commands.AddComponent<NetworkStreamInGame>(entity);
            }).Run(); //run the temporarily allocated for each job 

        // There's a sync point before and after each simulation group. The command buffer playback ensures that all commands are executed in
        // a single sync point
        commands.Playback(this.EntityManager);
    }
}
