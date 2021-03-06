﻿using SoftLiu.PlayerEntitiesTest;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public class PlayerMovementSystem : SystemBase
{
    protected override void OnUpdate()
    {
        float deltaTime = (float)Time.DeltaTime;
        Entities.
            WithAny<PlayerTag, EnemyTag>().
            ForEach((ref Translation translation, in MoveData moveData) =>
        {
            float3 normalizedDir = math.normalizesafe(moveData.direction);
            translation.Value += normalizedDir * moveData.speed * deltaTime;
        }).Run();
    }
}
