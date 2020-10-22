using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace SoftLiu.PhysicsEntitiesTest
{
    [GenerateAuthoringComponent]
    public struct HeathData : IComponentData
    {
        public bool isDead;

    }
}
