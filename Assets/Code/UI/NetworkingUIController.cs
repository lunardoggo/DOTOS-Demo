using UnityEngine.SceneManagement;
using System.Collections.Generic;
using Unity.Networking.Transport;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;
using System;
using TMPro;

// NOTE: DOTS requires an eventsystem, this scene will have an eventsystem, as Unity UI is present, but if
// there's no eventsystem, you would have to create one in the start method
[RequireComponent(typeof(ChatUIController))]
public class NetworkingUIController : MonoBehaviour
{
    [SerializeField]
    private TMP_Text server;
    [SerializeField]
    private TMP_Text status;
    [SerializeField]
    private TMP_Text ping;
    [SerializeField]
    private TMP_Text fps;
    [SerializeField]
    private TMP_Text units;
    [SerializeField]
    private string localSceneName;

    private EntityQuery unitsQuery;
    private World clientWorld;

    private void Start()
    {
        foreach (World world in World.All)
        {
            if (world.IsClient() && !world.IsThinClient())
            {
                // create systems that should be present in client worlds, for example the system interfacing
                // the UI with DOTS networking
                NetworkingUISystem uiSystem = world.GetOrCreateSystemManaged<NetworkingUISystem>();
                uiSystem.UIController = this;
                SimulationSystemGroup simulationGroup = world.GetExistingSystemManaged<SimulationSystemGroup>();
                simulationGroup.AddSystemToUpdateList(uiSystem);
                this.clientWorld = world;
                this.unitsQuery = this.clientWorld.EntityManager.CreateEntityQuery(typeof(Unit));
            }
        }
    }

    private void Update()
    {
        if (this.clientWorld != null)
        {
            this.units.text = $"Units: {this.unitsQuery.CalculateEntityCount()}";
        }
        else
        {
            this.units.text = $"Units: 0";
        }

        this.fps.text = $"FPS: {1 / Time.deltaTime:0}";
    }

    public void OnDisconnectClicked()
    {
        this.DestroyClientServerWorlds();

        string localWorldName = "DefaultWorld"; //fallback, if, for some reason, there isn't any cached old local world name
        if (!String.IsNullOrEmpty(LocalUIController.OldLocalSimulationWorldName))
        {
            localWorldName = LocalUIController.OldLocalSimulationWorldName;
        }

        ClientServerBootstrap.CreateLocalWorld(localWorldName);
        SceneManager.LoadScene(this.localSceneName);
        this.clientWorld = null;
    }

    public void SetServerAddress(NetworkEndpoint? endpoint)
    {
        if (endpoint.HasValue)
        {
            this.server.text = $"Server: {endpoint.Value.Address}:{endpoint.Value.Port}";
        }
        else
        {
            this.server.text = "Server: -";
        }
    }

    public void SetConnectionState(ConnectionState state)
    {
        switch (state)
        {
            case ConnectionState.NotConnected:
                this.status.text = "Status: not connected";
                break;
            default:
                this.status.text = "Status: " + state.ToString().ToLower();
                break;
        }
    }

    public void SetPing(int milliseconds)
    {
        if (milliseconds < 0)
        {
            this.ping.text = "Ping: -";
        }
        else
        {
            this.ping.text = $"Ping: {milliseconds}ms";
        }
    }

    private void DestroyClientServerWorlds()
    {
        //To prevent changes to World.All during the loop is running (and the corresponding exception),
        //first all client and server worlds are cached and then disposed in a separate loop
        List<World> worlds = new List<World>();
        foreach (World world in World.All)
        {
            if (world.IsClient() || world.IsServer())
            {
                worlds.Add(world);
            }
        }

        foreach (World world in worlds)
        {
            world.Dispose();
        }
    }
}
