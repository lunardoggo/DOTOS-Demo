using UnityEngine.Scripting;
using Unity.NetCode;
using UnityEngine.SceneManagement;

[Preserve] //Prevents Unity from not including this class in the shipped product
public class NetworkingBootstrap : ClientServerBootstrap
{
    //A bootstrap can be used to configure client and server settings
    public override bool Initialize(string defaultWorldName)
    {
        UnityEngine.Debug.Log("Setup Bootstrap!");

        //If the active scene is "LocalScene", there's no network connection yet. The setup of the connection can 
        bool isLocal = SceneManager.GetActiveScene().name.Equals("LocalScene");

        if (isLocal)
        {
            ClientServerBootstrap.AutoConnectPort = 0; //No autoconnect
            ClientServerBootstrap.CreateLocalWorld(defaultWorldName);
        }
        else
        {
            ClientServerBootstrap.AutoConnectPort = 11111;

            base.CreateDefaultClientServerWorlds();
        }

        //return true if the bootstrap created at least one world, false otherwise; if false is returned, a default world
        //will be created by Unity
        return true;
    }
}
