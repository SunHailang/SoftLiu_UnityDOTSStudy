using UnityEditor;
using UnityEditor.UI;
using UnityEngine;
[CustomEditor(typeof(TestEmojiText), true)]
[CanEditMultipleObjects]
public class TestEmojiTextEditor : GraphicEditor
{
    #region 属性
    private TestEmojiText _emojiText;
    private string _lastText;
    SerializedProperty _text;
    SerializedProperty m_Text;
    SerializedProperty m_FontData;
    GUIContent _inputGUIContent;
    GUIContent _outputGUIContent;
    private SerializedProperty m_emojiSpriteList;
    #endregion
    protected override void OnEnable()
    {
        base.OnEnable();
        _lastText = "";
        _inputGUIContent = new GUIContent("Input Text");
        _outputGUIContent = new GUIContent("Output Text");

        _text = serializedObject.FindProperty("_text");
        m_Text = serializedObject.FindProperty("m_Text");
        m_FontData = serializedObject.FindProperty("m_FontData");
        m_emojiSpriteList = serializedObject.FindProperty("m_emojiSpriteList");

        _emojiText = target as TestEmojiText;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(_text, _inputGUIContent);
        EditorGUILayout.PropertyField(m_Text, _outputGUIContent);
        EditorGUILayout.PropertyField(m_FontData);
        EditorGUILayout.PropertyField(m_emojiSpriteList);
        AppearanceControlsGUI();
        RaycastControlsGUI();
        serializedObject.ApplyModifiedProperties();
        //更新字符
        if (_emojiText != null && _lastText != _text.stringValue)
        {
            _emojiText.text = _text.stringValue;
            _lastText = _text.stringValue;
        }
    }
}
