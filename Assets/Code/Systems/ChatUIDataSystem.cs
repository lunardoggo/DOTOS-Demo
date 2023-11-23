using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

// This system facilitates passing data between DOTS and GameObjects in a decoupled, clean way by creating and cleaning up the users and messages queues
[UpdateInGroup(typeof(DemoChatSystemGroup))]
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
public partial class ChatUIDataSystem : SystemBase
{
    private bool ownsData;

    protected override void OnCreate()
    {
        this.ownsData = !ChatUIData.Users.Data.IsCreated;
        if(this.ownsData)
        {
            ChatUIData.Users.Data = new UnsafeRingQueue<int>(32, Allocator.Persistent); // There's space for at max 32 non-dequeued users for now
            ChatUIData.Messages.Data = new UnsafeRingQueue<FixedString128Bytes>(32, Allocator.Persistent); // same with not already dequeued messages
            // The chat ui controller will dequeue users and messages, so that an overflow of these two queues is unlikely
        }
        this.Enabled = false;
        Debug.Log("Created users and messages queue: " + this.ownsData);
    }

    protected override void OnUpdate()
    { } // do nothing, this system only creates the queues if they don't exist and cleans them up later if it created the queues

    protected override void OnDestroy()
    {
        if(this.ownsData)
        {
            ChatUIData.Messages.Data.Dispose();
            ChatUIData.Users.Data.Dispose();
        }
    }
}
