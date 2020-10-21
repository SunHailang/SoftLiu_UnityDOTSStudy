using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

public class WaveSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((ref Translation tanslation, ref MoveData moveSpeed, ref WaveData waveData) =>
        {
            float zPosition = waveData.amplitude * math.sin((float)Time.ElapsedTime * moveSpeed.Value +
                tanslation.Value.x * waveData.xOffset + tanslation.Value.y * waveData.yOffset);
            tanslation.Value = new float3(tanslation.Value.x, tanslation.Value.y, zPosition);
        });
    }
}
