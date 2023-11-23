using Unity.Entities;

public struct EnableRPC : IComponentData
{ }
// No data inside JoinGame needed, as it works more like a tag instead of
// a component holding game state (Unity also displays it as a tag in the inspector)
