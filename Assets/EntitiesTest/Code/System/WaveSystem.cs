using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Jobs;
using System;

namespace SoftLiu.EntitiesTest
{
    public class WaveSystem : SystemBase
    {
        public class JobParalledForTest : IJobParallelFor
        {
            public void Execute(int index)
            {
                
            }
        }

        protected override void OnCreate()
        {
            
        }

        protected override void OnUpdate()
        {
            float elapsedTime = (float)Time.ElapsedTime;

            Entities.WithAll<WaveMoveTag>().ForEach((ref Translation tanslation, ref MoveData moveSpeed, ref WaveData waveData) =>
             {
                 float zPosition = waveData.amplitude * math.sin(elapsedTime * moveSpeed.Value +
                     tanslation.Value.x * waveData.xOffset + tanslation.Value.y * waveData.yOffset);
                 tanslation.Value = new float3(tanslation.Value.x, tanslation.Value.y, zPosition);
             }).Schedule();
        }
    }
}
