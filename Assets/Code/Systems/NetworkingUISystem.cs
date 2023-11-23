using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

[DisableAutoCreation] //This system should only be created manually, as local scenes for example shouldn't use it
// both of the following attributes are set in the demo, but are not necessary, as this system will always be created manually
// they are preserved in order to make it easier to know the parameters this system should run in
[UpdateInGroup(typeof(LateSimulationSystemGroup))]
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
public partial class NetworkingUISystem : SystemBase //SystemBase is incompatible with Burst and generally single threaded
{
    public NetworkingUIController UIController { get; set; }

    protected override void OnUpdate()
    {
        //Finish scheduled jobs to get results immediately
        base.CompleteDependency();

        //frameCount: frame number since game start; refresh the UI regurarily but not too often to avoid high CPU load
        if (UnityEngine.Time.frameCount % 30 == 0)
        {
            if (SystemAPI.TryGetSingletonEntity<NetworkStreamConnection>(out Entity connectionEntity))
            {
                NetworkStreamConnection connection = this.EntityManager.GetComponentData<NetworkStreamConnection>(connectionEntity);
                NetworkStreamDriver driver = SystemAPI.GetSingleton<NetworkStreamDriver>();

                this.UIController.SetServerAddress(driver.GetRemoteEndPoint(connection));
                if (this.EntityManager.HasComponent<NetworkId>(connectionEntity))
                {
                    //true if the connection was successfully established
                    NetworkSnapshotAck snapshot = this.EntityManager.GetComponentData<NetworkSnapshotAck>(connectionEntity);
                    this.UIController.SetPing(Mathf.RoundToInt(snapshot.EstimatedRTT));
                    this.UIController.SetConnectionState(ConnectionState.Connected);
                }
                else
                {
                    //Manually add new network id component for this new player
                    this.EntityManager.AddComponent<NetworkId>(connectionEntity);

                    //false if the connection is being established
                    this.UIController.SetConnectionState(ConnectionState.Connecting);
                    this.UIController.SetPing(-1);
                }

            }
            else
            {
                this.UIController.SetConnectionState(ConnectionState.NotConnected);
                this.UIController.SetServerAddress(null);
                this.UIController.SetPing(-1);
            }
        }
    }
}