using System;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Unity.Burst;
using UnityEngine.Jobs;

public class JobSysetmTest : MonoBehaviour
{
    public GameObject prefab = null;

    private GameObject[] games;
    private Transform[] gameTransforms;

    [Range(30, 100)]
    public float m_cycle = 50;

    private float m_deltaTime = 0;

    private float m_scale = 0;

    private void Start()
    {
        m_scale = 0.2f;
        prefab.transform.localScale = new Vector3(m_scale, m_scale, m_scale);
        int len = Mathf.FloorToInt(20 / m_scale);
        games = new GameObject[len];
        gameTransforms = new Transform[len];
        for (int i = 0; i < games.Length; i++)
        {
            GameObject obj = Instantiate(prefab);
            games[i] = obj;

            gameTransforms[i] = games[i].transform;
        }
    }

    private void Update()
    {
        m_deltaTime += Time.deltaTime;
        //NativeArray<float3> tmpPosition = new NativeArray<float3>(games.Length, Allocator.TempJob);
        //NativeArray<float3> tmpRotation = new NativeArray<float3>(games.Length, Allocator.TempJob);
        //for (int i = 0; i < games.Length; i++)
        //{
        //    tmpPosition[i] = new float3(games[i].transform.position.x, games[i].transform.position.y, games[i].transform.position.z);
        //    tmpRotation[i] = new float3(games[i].transform.position.x, games[i].transform.position.y, games[i].transform.position.z);
        //}
        //JobTest jobTest = new JobTest()
        //{
        //    position = tmpPosition,
        //    rotation = tmpRotation,
        //    deltaTime = m_deltaTime,
        //    cycle = m_cycle,
        //    scale = m_scale
        //};
        //JobHandle jobTestHandle = jobTest.Schedule();
        //jobTestHandle.Complete();

        //JobParallelForTest jobParallelForTest = new JobParallelForTest()
        //{
        //    position = tmpPosition,
        //    rotation = tmpRotation,
        //    deltaTime = m_deltaTime,
        //    cycle = m_cycle,
        //    scale = m_scale
        //};
        //JobHandle jobParallelForTestHandle = jobParallelForTest.Schedule(tmpPosition.Length, 1);
        //jobParallelForTestHandle.Complete();
        //for (int i = 0; i < games.Length; i++)
        //{
        //    games[i].transform.position = new Vector3(tmpPosition[i].x, tmpPosition[i].y, tmpPosition[i].z);
        //    games[i].transform.rotation = Quaternion.Euler(tmpRotation[i].x, tmpRotation[i].y, tmpRotation[i].z);
        //}
        //tmpPosition.Dispose();
        //tmpRotation.Dispose();
        TransformAccessArray transformAccessArray = new TransformAccessArray(gameTransforms);
        JobParallelForTransformTest jobParallelForTransformTest = new JobParallelForTransformTest()
        {
            deltaTime = m_deltaTime,
            cycle = m_cycle,
            scale = m_scale
        };
        JobHandle jobParallelForTransformTestHandle = jobParallelForTransformTest.Schedule(transformAccessArray);
        jobParallelForTransformTestHandle.Complete();
        transformAccessArray.Dispose();
    }
}

/// <summary>
/// 多线程控制
/// </summary>
[BurstCompile]
public struct JobTest : IJob
{
    public NativeArray<float3> position;
    public NativeArray<float3> rotation;
    public float deltaTime;
    public float cycle;
    public float scale;

    public void Execute()
    {
        for (int i = 0; i < position.Length; i++)
        {
            float pointX = (i * scale - 10);
            float x = (math.PI / 180) * ((pointX + deltaTime) * cycle);
            float y = math.sin(x);
            // 求导 是 某一点的斜率
            float slope = math.cos(x);
            // tan(倾斜角) = 斜率  => 获得倾斜角(弧度值)
            slope = math.atan(slope);
            // 弧度转角度
            slope *= 180 / math.PI;
            position[i] = new float3(pointX, y, 0);
            rotation[i] = new float3(0, 0, slope);
        }
    }
}

/// <summary>
/// 多线程并行化控制
/// </summary>
[BurstCompile]
public struct JobParallelForTest : IJobParallelFor
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

        //float x = position[index].x;
        //float y = Unity.Mathematics.math.sin((math.PI / 180) * ((x - deltaTime) * cycle));
        //position[index] = new float3(x, y, position[index].z);
    }
}

/// <summary>
/// 
/// </summary>
[BurstCompile]
public struct JobParallelForTransformTest : IJobParallelForTransform
{
    public float deltaTime;
    public float cycle;
    public float scale;

    public void Execute(int index, TransformAccess transform)
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
        transform.position = new float3(pointX, y, 0);
        transform.rotation = Quaternion.Euler(0, 0, slope);
    }
}
