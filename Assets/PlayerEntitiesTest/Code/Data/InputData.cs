using Unity.Entities;
using UnityEngine;

namespace SoftLiu.PlayerEntitiesTest
{
    [GenerateAuthoringComponent]
    public struct InputData : IComponentData
    {
        public KeyCode leftKey;
        public KeyCode rightKey;
        public KeyCode upKey;
        public KeyCode downKey;

        //public string horizontalAxis;
        //public string verticalAxis;
    }
}
