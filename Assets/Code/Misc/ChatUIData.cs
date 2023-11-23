using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections;
using Unity.Burst;

public abstract class ChatUIData
{
    // Message queue
    public static readonly SharedStatic<UnsafeRingQueue<FixedString128Bytes>> Messages = SharedStatic<UnsafeRingQueue<FixedString128Bytes>>.GetOrCreate<MessagesKey>();
    public static readonly SharedStatic<UnsafeRingQueue<int>> Users = SharedStatic<UnsafeRingQueue<int>>.GetOrCreate<UsersKey>();
    
    // These two classes are used as identifiers for the two shared static fields above
    private class MessagesKey {}
    private class UsersKey {}
}
