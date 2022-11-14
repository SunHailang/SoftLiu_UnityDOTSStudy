using SoftLiu.PlayerEntitiesTest;
using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;


public class TargetToDirectionSystem : SystemBase
{
    protected override void OnUpdate()
    {
        Entities.
            WithNone<PlayerTag>().
            WithAll<EnemyTag>().
            ForEach((ref MoveData moveData, ref Rotation rotation, in Translation translation, in TargetData targetData) =>
            {
                ComponentDataFromEntity<Translation> allTranslation = GetComponentDataFromEntity<Translation>(true);
                if (!allTranslation.HasComponent(targetData.targetEntity))
                {
                    return;
                }
                Translation targetPos = allTranslation[targetData.targetEntity];
                float3 dirToTarget = targetPos.Value - translation.Value;
                moveData.direction = dirToTarget;
            }).Run();
    }
}