using Unity.Collections;
using Unity.NetCode;

public struct ChatMessage : IRpcCommand
{
    public FixedString128Bytes Message;
}

public struct ChatUser : IRpcCommand
{
    public int UserId;
}

public struct SpawnUnitsCommand : IRpcCommand
{
    public int PlayerId;
    public int Count;
}