using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;

[UpdateAfter(typeof(EndFramePhysicsSystem))]
public class PickupOnCollisionSystem : JobComponentSystem
{
    private BuildPhysicsWorld buildPhysicsWorld;
    private StepPhysicsWorld stepPhysicsWorld;

    protected override void OnCreate()
    {
        base.OnCreate();
        buildPhysicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
        stepPhysicsWorld = World.GetOrCreateSystem<StepPhysicsWorld>();
    }

    [BurstCompile]
    struct PickupOnCollisionSystemJob : ICollisionEventsJob
    {
        [ReadOnly]
        public ComponentDataFromEntity<PhysicsVelocity> PhysicsVelocityGroup;

        public void Execute(CollisionEvent collisionEvent)
        {
            Entity entityA = collisionEvent.EntityA;
            Entity entityB = collisionEvent.EntityB;

            bool isBodyADynamic = PhysicsVelocityGroup.HasComponent(entityA);
            bool isBodyBDynamic = PhysicsVelocityGroup.HasComponent(entityB);


            if (isBodyADynamic && isBodyBDynamic)
            {

            }
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        PickupOnCollisionSystemJob job = new PickupOnCollisionSystemJob();
        job.PhysicsVelocityGroup = GetComponentDataFromEntity<PhysicsVelocity>(true);

        JobHandle jobHandle = job.Schedule(stepPhysicsWorld.Simulation,
                                        ref buildPhysicsWorld.PhysicsWorld,
                                        inputDeps);


        jobHandle.Complete();


        return jobHandle;
    }
}
