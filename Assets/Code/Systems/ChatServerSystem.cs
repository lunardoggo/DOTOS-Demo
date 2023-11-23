using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

[UpdateInGroup(typeof(DemoChatSystemGroup))]
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
public partial class ChatServerSystem : SystemBase
{
    private BeginSimulationEntityCommandBufferSystem commandBufferSystem;
    private NativeList<int> users; //a user is identified by their connection id

    protected override void OnCreate()
    {
        this.RequireForUpdate<EnableRPC>();
        this.commandBufferSystem = World.GetOrCreateSystemManaged<BeginSimulationEntityCommandBufferSystem>();
        this.users = new NativeList<int>(Allocator.Persistent);
    }

    protected override void OnUpdate()
    {
        ComponentLookup<NetworkId> connections = this.GetComponentLookup<NetworkId>(true); //true: readonly
        EntityCommandBuffer commands = this.commandBufferSystem.CreateCommandBuffer();
        FixedString32Bytes worldName = World.Name;

        //WithName: name for the processing job
        //WithReadOnly: store variable as readonly to be used in the job
        this.Entities.WithName("ReceiveChatMessage").WithReadOnly(connections).ForEach(
            (Entity entity, ref ReceiveRpcCommandRequest rpcCommand, ref ChatMessage chatMessage) => {
                int connectionId = connections[rpcCommand.SourceConnection].Value;
                Debug.Log($"[{worldName}] Received \"{chatMessage.Message}\" from connection User {connectionId}");
                commands.DestroyEntity(entity); //destroy original message sent to the server
                
                //Private messages aren't implemented for now, but it would be possible, for example by adding and evaluating a recipients bit mask in the ChatMessage component
                Entity broadcast = commands.CreateEntity();
                commands.AddComponent(broadcast, new ChatMessage() { Message = FixedString.Format("User {0}: {1}", connectionId, chatMessage.Message)});
                commands.AddComponent<SendRpcCommandRequest>(broadcast);
            }).Run();
        
        //TODO: look up what this is supposed to do
        this.commandBufferSystem.AddJobHandleForProducer(this.Dependency);

        NativeList<int> users = this.users; //Has to be cached, as instance variables aren't accessible in .ForEach blocks
        //WithNone: only include entities that don't have a specific component attached to them
        this.Entities.WithName("AddNewUser").WithNone<ChatUserInitialized>().ForEach(
            (Entity entity, ref NetworkId id) =>
            {
                int connectionId = id.Value;

                //Notify all chat users about the new connection (including the sender itself)
                Entity connectionBroadcast = commands.CreateEntity();
                commands.AddComponent(connectionBroadcast, new ChatUser() { UserId = connectionId });
                commands.AddComponent<SendRpcCommandRequest>(connectionBroadcast);
                Debug.Log($"[{worldName}] New chat user \"User {connectionId}\" joined the chat, sending broadcast to everyone");

                for(int i = 0; i < users.Length; i++)
                {
                    //Notify new user about all other currently connected users
                    Entity notification = commands.CreateEntity();
                    int userId = users[i];
                    commands.AddComponent(notification, new ChatUser() { UserId = userId });
                    commands.AddComponent(notification, new SendRpcCommandRequest() { TargetConnection = entity });
                }
                Debug.Log($"[{worldName}] Notified \"User {connectionId}\" about all currently connected chatters");

                users.Add(connectionId);
                commands.AddComponent<ChatUserInitialized>(entity);
            }).Run();

        //TODO: research how to deal with users disconnecting
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        //native lists are unmanaged and have to be disposed of manually as a result
        this.users.Dispose();
    }
}
