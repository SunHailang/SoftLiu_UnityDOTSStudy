using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace SoftLiu.EntitiesTest
{
    [GenerateAuthoringComponent]
    public struct WaveData : IComponentData
    {
        public float amplitude;
        public float xOffset;
        public float yOffset;
    }
}
