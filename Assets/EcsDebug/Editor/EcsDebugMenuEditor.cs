using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace EcsDebug
{

    public static class EcsDebugMenuEditor
    {
        [MenuItem("SoftLiu/EcsDebug")]
        private static void EcsDebugEditor()
        {
            EcsDebugWindow.OpenWindow();
        }
        
    }
}