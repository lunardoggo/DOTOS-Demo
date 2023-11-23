using System.Collections.Generic;
using Unity.Collections;
using UnityEngine.UI;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;
using System;
using TMPro;

public class ChatUIController : MonoBehaviour
{
    [SerializeField]
    private TMP_Text chatLinePrefab;
    [SerializeField]
    private RectTransform chatParent;
    [SerializeField]
    private ScrollRect scrollPane;
    [SerializeField]
    private TMP_InputField input;
    [SerializeField]
    private int maxMessages;
    [SerializeField]
    private int unitSpawnCount = 100;

    private Queue<TMP_Text> lines = new Queue<TMP_Text>();
    private List<World> clientWorlds = new List<World>();
    private int currentUserId = -1;
    private int lineCounter = 1;

    private void Start()
    {
        foreach (World world in World.All)
        {
            if (world.IsClient() && !world.IsThinClient())
            {
                this.clientWorlds.Add(world);
            }
        }
    }

    private void Update()
    {
        if (this.currentUserId == -1)
        {
            this.QueryCurrentUserId();
        }
        else
        {
            if (ChatUIData.Messages.Data.IsCreated && ChatUIData.Messages.Data.TryDequeue(out FixedString128Bytes message))
            {
                Debug.Log("Received message: " + message);
                this.DisplayNewMessage(message);
            }

            if (ChatUIData.Users.Data.IsCreated && ChatUIData.Users.Data.TryDequeue(out int userId))
            {
                Debug.Log("New user with id " + userId + " joined the chat");
                this.DisplayNewUser(userId);
            }
        }
    }

    public void OnChatInputEnter(string _)
    {
        if (this.currentUserId != -1 && !String.IsNullOrWhiteSpace(this.input.text) && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            string message = this.input.text;
            Debug.Log("Sending message: " + message + "...");
            this.SendChatMessage(message);

            this.input.text = "";
            this.input.Select();
            this.input.ActivateInputField();
        }
    }

    public void OnSpawnUnitsClicked()
    {
        if (this.clientWorlds.Count > 0 && this.clientWorlds[0].IsCreated)
        {
            World world = this.clientWorlds[0];

            Entity cmd = world.EntityManager.CreateEntity();
            world.EntityManager.AddComponentData(cmd, new SpawnUnitsCommand()
            {
                PlayerId = this.currentUserId,
                Count = this.unitSpawnCount
            });
            world.EntityManager.AddComponent<SendRpcCommandRequest>(cmd);
        }
    }

    private void DisplayNewMessage(FixedString128Bytes message)
    {
        TMP_Text line = GameObject.Instantiate(this.chatLinePrefab, this.chatParent.transform);
        line.text = this.lineCounter + ": " + message;
        this.lineCounter++;

        this.lines.Enqueue(line);
        if (this.lines.Count > this.maxMessages)
        {
#if UNITY_EDITOR
            GameObject.DestroyImmediate(this.lines.Dequeue().gameObject);
#else
            GameObject.Destroy(this.lines.Dequeue().gameObject);
#endif
        }

        this.scrollPane.verticalNormalizedPosition = 0.0f;
    }

    private void DisplayNewUser(int userId)
    {
        this.DisplayNewMessage($"User {userId} joined the chat");
    }

    private void SendChatMessage(string message)
    {
        if (this.clientWorlds.Count > 0 && this.clientWorlds[0].IsCreated)
        {
            World world = this.clientWorlds[0];

            Entity msg = world.EntityManager.CreateEntity();
            world.EntityManager.AddComponentData(msg, new ChatMessage() { Message = message });
            world.EntityManager.AddComponent<SendRpcCommandRequest>(msg);
            //if private messages were implemented, one would also add a target entity to the SendRpcCommandRequest for specifying the recipient of the private message
        }
    }

    private void QueryCurrentUserId()
    {
        //This demo only allows for one chat client id at once, its questionable if more than one would even have one use case for our game
        EntityQuery query = this.clientWorlds[0].EntityManager.CreateEntityQuery(ComponentType.ReadOnly<NetworkId>());
        NativeArray<NetworkId> connectionIds = query.ToComponentDataArray<NetworkId>(Allocator.Temp);

        if (connectionIds.Length > 0)
        {
            this.currentUserId = connectionIds[0].Value;
        }
    }
}
