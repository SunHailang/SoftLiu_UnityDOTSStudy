﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using System;

public class MoveSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        //Entities.ForEach((ref Translation translation) =>
        //{
        //    translation.Value.y += 1f * UnityEngine.Time.deltaTime;
        //});
    }
}
