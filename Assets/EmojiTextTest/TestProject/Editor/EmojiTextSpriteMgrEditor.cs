using System;
using UnityEditor;

[CustomEditor(typeof(EmojiText), true)]
[CanEditMultipleObjects]
public class EmojiTextSpriteMgrEditor : Editor
{
    #region 属性
    SerializedProperty _spriteAssets;
    #endregion

    private void OnEnable()
    {
        _spriteAssets = serializedObject.FindProperty("spriteAssets");
    }
    
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(_spriteAssets);
        serializedObject.ApplyModifiedProperties();
    }
}
