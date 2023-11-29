using UnityEngine.SceneManagement;
using Unity.Networking.Transport;
using Unity.Entities;
using Unity.NetCode;
using UnityEditor;
using UnityEngine;
using System;
using TMPro;

public class LocalUIController : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField ipAddress;
    [SerializeField]
    private TMP_InputField port;
    [SerializeField]
    private string networkingSceneName;

    private ushort lastPort;
    private string lastIp;

    public static string OldLocalSimulationWorldName { get; private set; }


    private void Update()
    {
        this.ValidateIPAddress();
        this.ValidatePort();
    }

    private void ValidatePort()
    {
        if (String.IsNullOrEmpty(this.port.text))
        {
            this.lastPort = 0;
        }
        else if (!UInt16.TryParse(this.port.text, out this.lastPort))
        {
            port.text = this.lastPort.ToString();
        }
    }

    private void ValidateIPAddress()
    {
        //This example project will only utilize IPv4 addresses, it would theoretically also be possible to use IPv6 addresses though
        if (!String.IsNullOrEmpty(this.ipAddress.text) && this.IPContainsInvalidCharacters())
        {
            this.ipAddress.text = this.lastIp;
        }
        else
        {
            this.lastIp = this.ipAddress.text;
        }
    }

    private bool IPContainsInvalidCharacters()
    {
        foreach (char c in this.ipAddress.text)
        {
            if (!Char.IsNumber(c) && c != '.')
            {
                return true;
            };
        }
        return false;
    }

    public void OnQuitClicked()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

    public void OnConnectClicked()
    {
        //No need to create a server world, as the server is running remotely
        World client = ClientServerBootstrap.CreateClientWorld("ClientWorld");

        SceneManager.LoadScene(this.networkingSceneName);

        this.DestroyLocalSimulationWorld();
        this.ConnectClient(client, this.GetServerEndpoint());
    }

    public void OnHostClicked()
    {
        //Create worlds for the server and client, both are running locally on this machine
        World server = ClientServerBootstrap.CreateServerWorld("ServerWorld");
        World client = ClientServerBootstrap.CreateClientWorld("ClientWorld");

        SceneManager.LoadScene(this.networkingSceneName);

        this.DestroyLocalSimulationWorld();
        World.DefaultGameObjectInjectionWorld ??= server; //set the default injection world to server if it's null

        //Start the server; do this in two steps in order to be able to dispose the query afterwards and not leave any
        //unmanaged stuff in memory
        using EntityQuery serverDriverQuery = server.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<NetworkStreamDriver>());
        serverDriverQuery.GetSingletonRW<NetworkStreamDriver>().ValueRW.Listen(NetworkEndpoint.AnyIpv4.WithPort(this.lastPort));

        // Control the tick rate of the server; in this case it should run at 140Hz
        ClientServerTickRate tickRate = new ClientServerTickRate();
        tickRate.SimulationTickRate = 140;
        tickRate.ResolveDefaults(); //automatic calculation for other properties
        server.EntityManager.CreateSingleton(tickRate);

        //Start the client connection
        //As the server is running locally, the client should connect to the local loopback regardless of the ip address provided to the server to listen to
        this.ConnectClient(client, NetworkEndpoint.LoopbackIpv4.WithPort(this.lastPort));
    }

    private void ConnectClient(World clientWorld, NetworkEndpoint serverEndpoint)
    {
        using EntityQuery clientDriverQuery = clientWorld.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<NetworkStreamDriver>());
        clientDriverQuery.GetSingletonRW<NetworkStreamDriver>().ValueRW.Connect(clientWorld.EntityManager, serverEndpoint);
    }

    private NetworkEndpoint GetServerEndpoint()
    {
        return NetworkEndpoint.Parse(this.lastIp, this.lastPort);
    }

    private void DestroyLocalSimulationWorld()
    {
        foreach (World world in World.All)
        {
            //Local worlds have the WorldFlags.Game flag
            if (world.Flags == WorldFlags.Game)
            {
                LocalUIController.OldLocalSimulationWorldName = world.Name;
                world.Dispose(); //Destroy and cleanup the world
                break; //there should only be one local simulation => break out of the loop
            }
        }
    }
}
