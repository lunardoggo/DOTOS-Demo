using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

[UpdateInGroup(typeof(DemoChatSystemGroup))]
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
public partial class ChatClientSystem : SystemBase
{
    private BeginSimulationEntityCommandBufferSystem commandBufferSystem;

    protected override void OnCreate()
    {
        this.RequireForUpdate<EnableRPC>();
        this.RequireForUpdate<NetworkId>(); // clients can only chat if they are connected
        this.commandBufferSystem = World.GetOrCreateSystemManaged<BeginSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        ComponentLookup<NetworkId> connections = this.GetComponentLookup<NetworkId>(true); //true: readonly
        EntityCommandBuffer commands = this.commandBufferSystem.CreateCommandBuffer();
        FixedString32Bytes worldName = World.Name;

        // Enable chatting for existing users
        this.Entities.WithName("ReceiveChatMessage").ForEach(
            (Entity entity, ref ReceiveRpcCommandRequest rpcCommand, ref ChatMessage chatMessage) =>
            {
                commands.DestroyEntity(entity); //destroy the message
                ChatUIData.Messages.Data.Enqueue(chatMessage.Message);
            }).Run();

        this.Entities.WithName("RegisterUser").WithReadOnly(connections).ForEach(
            (Entity entity, ref ReceiveRpcCommandRequest rpcCommand, ref ChatUser user) =>
            {
                int connectionId = connections[rpcCommand.SourceConnection].Value;
                Debug.Log($"[{worldName}] Received {user.UserId} from connection with id {connectionId}");
                commands.DestroyEntity(entity); //destroy user chat registration
                ChatUIData.Users.Data.Enqueue(user.UserId);
            }).Run();
    }
}
