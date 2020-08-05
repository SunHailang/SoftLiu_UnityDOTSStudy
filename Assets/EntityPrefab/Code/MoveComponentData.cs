using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public struct MoveComponentData : IComponentData
{
    public float moveSpeed;
    public float3 moveDirect;
}

