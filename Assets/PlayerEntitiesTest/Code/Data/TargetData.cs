using Unity.Entities;

namespace SoftLiu.PlayerEntitiesTest
{
    [GenerateAuthoringComponent]
    public struct TargetData : IComponentData
    {
        public Entity targetEntity;
    }
}
