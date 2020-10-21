using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using System;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Rendering;

public class EntitiesTest : MonoBehaviour
{
    [SerializeField]
    private Mesh unitMesh;
    [SerializeField]
    private Material unitMaterial;
    [SerializeField]
    private GameObject gameObjectPrefab;

    private EntityManager m_entityManager;
    private Entity m_entityPrefab;
    private World m_entityWorld;

    private void Awake()
    {
        m_entityWorld = World.DefaultGameObjectInjectionWorld;
        m_entityManager = m_entityWorld.EntityManager;
    }

    private void Start()
    {
        GameObjectConversionSettings settings = GameObjectConversionSettings.FromWorld(m_entityWorld, new BlobAssetStore());
        m_entityPrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(gameObjectPrefab, settings);


        //InstantiateEntity(new float3(2f, 0f, 4f));
        InstantiateEntityGrid(10, 10, 0.1f);
    }

    private void InstantiateEntity(float3 position)
    {
        Entity myEntity = m_entityManager.Instantiate(m_entityPrefab);
        m_entityManager.SetComponentData(myEntity, new Translation()
        {
            Value = position
        });
    }

    private void InstantiateEntityGrid(int dimX, int dimY, float spacing = 0.1f)
    {
        for (int i = 0; i < dimX; i++)
        {
            for (int j = 0; j < dimY; j++)
            {
                InstantiateEntity(new float3(i + spacing, j + spacing, 0));
            }
        }
    }

    private void MakeEntity()
    {
        EntityArchetype archetype = m_entityManager.CreateArchetype(
            typeof(Translation),
            typeof(Rotation),
            typeof(RenderMesh),
            typeof(RenderBounds),
            typeof(LocalToWorld));

        Entity entity = m_entityManager.CreateEntity(archetype);
        m_entityManager.AddComponentData(entity, new Translation()
        {
            Value = new float3(2f, 0f, 4f)
        });

        m_entityManager.AddSharedComponentData(entity, new RenderMesh()
        {
            mesh = unitMesh,
            material = unitMaterial
        });
    }

}

public struct EntitiesJobParallelForTest : IJobParallelFor
{
    public NativeArray<float3> position;
    public NativeArray<float3> rotation;
    public float deltaTime;
    public float cycle;
    public float scale;

    public void Execute(int index)
    {
        float pointX = (index * scale - 10);
        float x = (math.PI / 180) * ((pointX + deltaTime) * cycle);
        float y = math.sin(x);
        // 求导 是 某一点的斜率
        float slope = math.cos(x);
        // tan(倾斜角) = 斜率  => 获得倾斜角(弧度值)
        slope = math.atan(slope);
        // 弧度转角度
        slope *= 180 / math.PI;
        position[index] = new float3(pointX, y, 0);
        rotation[index] = new float3(0, 0, slope);
    }
}
