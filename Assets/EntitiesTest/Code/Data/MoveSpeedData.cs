using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

namespace SoftLiu.EntitiesTest
{
    [GenerateAuthoringComponent]
    public struct MoveData : IComponentData
    {
        public float Value;
    }
}
