using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Jobs;
using UnityEngine.Jobs;
using Unity.Collections;
using Unity.Mathematics;

namespace job.tween
{
    public class JobSystemTweenManager : MonoBehaviour
    {
        private static JobSystemTweenManager m_instance = null;
        public static JobSystemTweenManager GetInstance()
        {
            return m_instance;
        }

        private void Awake()
        {
            if(m_instance != null)
            {
                DestroyImmediate(m_instance);
            }
            m_instance = this;
        }
    }

    public class TweenData
    {
        TransformAccessArray transArray = new TransformAccessArray();
        
        public void OnStart()
        {
            
        }

        public void OnUpdate()
        {

        }

        public void OnRelease()
        {

        }
    }

    public enum TransType
    {
        PositionType,
        RotationType,
        ScaleType,
    }

    public struct TransformJob : IJobParallelForTransform
    {
        [ReadOnly]
        public NativeArray<float3> float3s;
        [ReadOnly]
        public float deltaTime;
        [ReadOnly]
        public float speed;
        [ReadOnly]
        public TransType type;
        public void Execute(int index, TransformAccess transform)
        {
            
        }
    }
}

