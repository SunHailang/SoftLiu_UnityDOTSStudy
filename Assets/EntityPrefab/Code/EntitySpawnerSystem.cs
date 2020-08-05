using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

public class EntitySpawnerSystem : ComponentSystem
{
    private float m_spawnTimer = 0;
    private Unity.Mathematics.Random random;

    protected override void OnCreate()
    {
        random = new Unity.Mathematics.Random(56);
    }

    protected override void OnUpdate()
    {
        m_spawnTimer -= UnityEngine.Time.deltaTime;
        if (m_spawnTimer <= 0)
        {
            m_spawnTimer = 0.5f;
            // Spawn

            Entity spawnedEntity = PrefabEntities.managerEntity.Instantiate(PrefabEntities.prefabEntity);
            EntityManager.SetComponentData(spawnedEntity, new Translation() { Value = new float3(random.NextFloat(-5, 5), random.NextFloat(-5, 5), 0) });

            /*
            Entities.ForEach((ref PrefabEntityComponent prefabEntityComponent) =>
            {
                Entity spawnedEntity = EntityManager.Instantiate(prefabEntityComponent.prefabEntuity);

                EntityManager.SetComponentData(spawnedEntity,
                    new Translation { Value = new float3(random.NextFloat(-5, 5), random.NextFloat(-5,5), 0)}
                    );
            });
            */
        }
    }
}
