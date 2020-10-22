using SoftLiu.PhysicsEntitiesTest;
using SoftLiu.PlayerEntitiesTest;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public class RemoveOnDeathSystem : SystemBase
{
    private EndSimulationEntityCommandBufferSystem commandBufferSystem;

    protected override void OnCreate()
    {
        base.OnCreate();
        commandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        EntityCommandBuffer entityCommandBuffer = commandBufferSystem.CreateCommandBuffer();

        Entities.
            WithAny<PlayerTag>().
            ForEach((Entity entity, in HeathData heathData) =>
            {
                if (heathData.isDead)
                {
                    entityCommandBuffer.DestroyEntity(entity);
                }

            }).Schedule();
        commandBufferSystem.AddJobHandleForProducer(this.Dependency);
    }
}
