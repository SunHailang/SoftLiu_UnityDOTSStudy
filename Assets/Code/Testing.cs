using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using Unity.Rendering;
using Unity.Mathematics;

public class Testing : MonoBehaviour
{

    [SerializeField]
    private Mesh m_mesh;
    [SerializeField]
    private Material m_material;

    private void Start()
    {
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        NativeArray<Entity> entityArray = new NativeArray<Entity>(1, Allocator.Temp);
        //var entityGameObject = GameObjectConversionUtility.ConvertGameObjectHierarchy(m_prefab,
        //    new GameObjectConversionSettings(World.DefaultGameObjectInjectionWorld, GameObjectConversionUtility.ConversionFlags.AssignName,
        //    new BlobAssetStore()));
        //Entity entity = entityManager.Instantiate(entityGameObject);
        //entityManager.SetComponentData(entityGameObject, new Translation() { Value = float3.zero });


        EntityArchetype entityArchetype = entityManager.CreateArchetype(
            typeof(LevelComponent),
            typeof(Translation),
            typeof(RenderMesh),
            typeof(LocalToWorld)
            );


        entityManager.CreateEntity(entityArchetype, entityArray);
        for (int i = 0; i < entityArray.Length; i++)
        {
            Entity entity = entityArray[i];
            entityManager.SetComponentData(entity, new LevelComponent { level = UnityEngine.Random.Range(10, 20) });

            entityManager.SetSharedComponentData(entity, new RenderMesh
            {
                mesh = m_mesh,
                material = m_material,
            });
        }
        entityArray.Dispose();
    }

    private void Update()
    {

    }

    private void ReallyToughTask()
    {

    }

}
