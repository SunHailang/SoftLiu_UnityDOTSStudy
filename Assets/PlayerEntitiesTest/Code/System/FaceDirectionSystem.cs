using System;
using Unity.Entities;
using Unity.Transforms;
using SoftLiu.PlayerEntitiesTest;
using Unity.Mathematics;


[UpdateAfter(typeof(TransformSystemGroup))]
public class FaceDirectionSystem : SystemBase
{
    protected override void OnUpdate()
    {
        Entities.
            WithAny<PlayerTag, EnemyTag>().
            ForEach((ref Rotation rotation, in Translation pos, in MoveData moveDta) =>
          {
              if (!moveDta.direction.Equals(float3.zero))
              {
                  quaternion targetRotation = quaternion.LookRotationSafe(moveDta.direction, math.up());
                  rotation.Value = math.slerp(rotation.Value, targetRotation, moveDta.turnSpeed);
              }
          }).Schedule();
    }
}
