using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using System;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;

public class EntitiesTest : MonoBehaviour
{

    public GameObject prefab;

    private EntityManager m_entityManager;

    private NativeArray<Entity> entityArray;

    [Range(30, 100)]
    public float m_cycle = 50;

    private float m_deltaTime = 0;

    private float m_scale = 0.2f;

    private void Awake()
    {
        m_entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
    }

    private void Start()
    {
        prefab.transform.localScale = Vector3.one * m_scale;

        int len = Mathf.FloorToInt(20 / m_scale);
        entityArray = new NativeArray<Entity>(len, Allocator.Persistent);

        Entity entity = GameObjectConversionUtility.ConvertGameObjectHierarchy(prefab, GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, new BlobAssetStore()));
        m_entityManager.Instantiate(entity, entityArray);
        for (int i = 0; i < entityArray.Length; i++)
        {
            m_entityManager.SetComponentData<Translation>(entityArray[i], new Translation() { Value = float3.zero });
            m_entityManager.SetComponentData<Rotation>(entityArray[i], new Rotation() { Value = Quaternion.identity });
        }

    }

    private void Update()
    {
        m_deltaTime += Time.deltaTime;

        NativeArray<float3> tmpPosition = new NativeArray<float3>(entityArray.Length, Allocator.TempJob);
        NativeArray<float3> tmpRotation = new NativeArray<float3>(entityArray.Length, Allocator.TempJob);
        for (int i = 0; i < entityArray.Length; i++)
        {
            Translation ts = m_entityManager.GetComponentData<Translation>(entityArray[i]);
            Rotation rt = m_entityManager.GetComponentData<Rotation>(entityArray[i]);
            tmpPosition[i] = ts.Value.xyz;
            tmpRotation[i] = rt.Value.value.xyz;
        }

        EntitiesJobParallelForTest jobParallelForTest = new EntitiesJobParallelForTest()
        {
            position = tmpPosition,
            rotation = tmpRotation,
            deltaTime = m_deltaTime,
            cycle = m_cycle,
            scale = m_scale
        };
        JobHandle jobTestHandle = jobParallelForTest.Schedule(entityArray.Length, 10);
        jobTestHandle.Complete();
        for (int i = 0; i < entityArray.Length; i++)
        {
            Translation ts = m_entityManager.GetComponentData<Translation>(entityArray[i]);
            Rotation rt = m_entityManager.GetComponentData<Rotation>(entityArray[i]);
            ts.Value.xyz = tmpPosition[i];
            rt.Value.value.xyz = tmpRotation[i];
        }
        tmpPosition.Dispose();
        tmpRotation.Dispose();
    }

    private void OnDestroy()
    {
        entityArray.Dispose();
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
