using System;
using UnityEditor;

[CustomEditor(typeof(EmojiTextSpriteMgrEditor), true)]
[CanEditMultipleObjects]
public class EmojiTextSpriteMgrEditor : Editor
{
    #region 属性
    SerializedProperty _spriteAssets;
    #endregion

    private void OnEnable()
    {
        _spriteAssets = serializedObject.FindProperty("_spriteAssets");
    }
    
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(_spriteAssets);
        serializedObject.ApplyModifiedProperties();
    }
}
