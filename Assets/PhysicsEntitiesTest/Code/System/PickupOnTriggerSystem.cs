using System;
using Unity.Entities;
using Unity.Jobs;
using Unity.Burst;
using Unity.Transforms;
using Unity.Collections;
using SoftLiu.PhysicsEntitiesTest;
using SoftLiu.PlayerEntitiesTest;
using Unity.Physics;
using Unity.Physics.Systems;


[UpdateAfter(typeof(EndFramePhysicsSystem))]
public class PickupOnTriggerSystem : JobComponentSystem
{
    private BuildPhysicsWorld buildPhysicsWorld;
    private StepPhysicsWorld stepPhysicsWorld;

    private EndSimulationEntityCommandBufferSystem commandBufferSystem;

    protected override void OnCreate()
    {
        base.OnCreate();
        buildPhysicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
        stepPhysicsWorld = World.GetOrCreateSystem<StepPhysicsWorld>();
        commandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    [BurstCompile]
    struct PickupOnTriggerSystemJob : ITriggerEventsJob
    {
        [ReadOnly]
        public ComponentDataFromEntity<PickupData> allPickups;
        [ReadOnly]
        public ComponentDataFromEntity<PlayerTag> allPlayers;

        public EntityCommandBuffer entityCommandBuffer;

        public void Execute(TriggerEvent triggerEvent)
        {
            Entity entityA = triggerEvent.EntityA;
            Entity entityB = triggerEvent.EntityB;

            if (allPickups.HasComponent(entityA) && allPickups.HasComponent(entityB))
            {
                return;
            }
            if (allPickups.HasComponent(entityA) && allPlayers.HasComponent(entityB))
            {
                UnityEngine.Debug.Log($"Pickup Entity A: {entityA} collided with Player Entity B: {entityB}");
                entityCommandBuffer.DestroyEntity(entityA);
            }
            else if (allPlayers.HasComponent(entityA) && allPickups.HasComponent(entityB))
            {
                UnityEngine.Debug.Log($"Player Entity A: {entityA} collided with Pickup Entity B: {entityB}");
                entityCommandBuffer.DestroyEntity(entityB);
            }
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var job = new PickupOnTriggerSystemJob();
        job.allPickups = GetComponentDataFromEntity<PickupData>(true);
        job.allPlayers = GetComponentDataFromEntity<PlayerTag>(true);

        job.entityCommandBuffer = commandBufferSystem.CreateCommandBuffer();

        JobHandle jobHandle = job.Schedule(stepPhysicsWorld.Simulation, ref buildPhysicsWorld.PhysicsWorld, inputDeps);

        jobHandle.Complete();
        commandBufferSystem.AddJobHandleForProducer(jobHandle);

        return jobHandle;
    }
}
