using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Jobs;

namespace SoftLiu.EntitiesTest
{
    public class WaveSystem : ComponentSystem
    {

        protected override void OnUpdate()
        {
            float elapsedTime = (float)Time.ElapsedTime;

            Entities.WithAll<MoveData>().ForEach((ref Translation tanslation, ref MoveData moveSpeed, ref WaveData waveData) =>
            {
                float zPosition = waveData.amplitude * math.sin(elapsedTime * moveSpeed.Value +
                    tanslation.Value.x * waveData.xOffset + tanslation.Value.y * waveData.yOffset);
                tanslation.Value = new float3(tanslation.Value.x, tanslation.Value.y, zPosition);
            });
        }
    }
}
