using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EmojiText), true)]
[CanEditMultipleObjects]
public class EmojiTextEditor : UnityEditor.UI.TextEditor
{
    #region 属性
    SerializedProperty _emojiAsset;
    SerializedProperty m_Text;
    SerializedProperty _text;
    GUIContent _inputGUIContent;
    GUIContent _outputGUIContent;
    #endregion

    protected override void OnEnable()
    {
        base.OnEnable();
        _emojiAsset = serializedObject.FindProperty("emojiAsset");
        _inputGUIContent = new GUIContent("Input Text");
        _outputGUIContent = new GUIContent("Output Text");

        _text = serializedObject.FindProperty("_text");
        m_Text = serializedObject.FindProperty("m_Text");
    }
    
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(_emojiAsset);
        
        EditorGUILayout.PropertyField(_text, _inputGUIContent);
        EditorGUILayout.PropertyField(m_Text, _outputGUIContent);
        AppearanceControlsGUI();
        RaycastControlsGUI();
        serializedObject.ApplyModifiedProperties();
    }
}
