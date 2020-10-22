using SoftLiu.PlayerEntitiesTest;
using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;


[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateBefore(typeof(TargetToDirectionSystem))]
public class AssignPlayerToTargetSystem : SystemBase
{
    protected override void OnStartRunning()
    {
        base.OnStartRunning();
        AssignPlayer();
    }

    private void AssignPlayer()
    {
        EntityQuery playerQuery = GetEntityQuery(ComponentType.ReadOnly<PlayerTag>());
        Entity player = playerQuery.GetSingletonEntity();
        Entities.
            WithAll<EnemyTag>().
            ForEach((ref TargetData targetData) =>
            {
                if (player != Entity.Null)
                {
                    targetData.targetEntity = player;
                }
            }).Schedule();
    }

    protected override void OnUpdate()
    {
        
    }
}