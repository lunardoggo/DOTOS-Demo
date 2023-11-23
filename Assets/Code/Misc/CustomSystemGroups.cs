using Unity.Entities;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
public partial class DemoChatSystemGroup : ComponentSystemGroup
{}