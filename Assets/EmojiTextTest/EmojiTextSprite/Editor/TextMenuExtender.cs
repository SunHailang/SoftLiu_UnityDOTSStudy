using UnityEngine;
using System.Collections;
using act.ui.EmojiText;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace act.ui.EmojiText.Editor
{
    public class TextMenuExtender
    {
        [MenuItem("GameObject/UI/uiEmojiText", false, 10)]
        static void CreateCustomGameObject(MenuCommand menuCommand)
        {
            GameObject go = null;
            EmojiManager inline = AssetDatabase.LoadAssetAtPath<EmojiManager>("Assets/ResourceRex/UI/Emoji/Prefabs/EmojiText/UiEmojiText.prefab");
            if (inline)
            {
                go = GameObject.Instantiate(inline).gameObject;
            }
            else
            {
                go = new GameObject();
                go.AddComponent<UiEmojiText>();
            }
            go.name = "UiEmojiText";
            GameObject parent = menuCommand.context as GameObject;
            if (parent == null)
            {
                parent = new GameObject("Canvas");
                parent.layer = LayerMask.NameToLayer("UI");
                parent.AddComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
                parent.AddComponent<CanvasScaler>();
                parent.AddComponent<GraphicRaycaster>();

                EventSystem _es = GameObject.FindObjectOfType<EventSystem>();
                if (!_es)
                {
                    _es = new GameObject("EventSystem").AddComponent<EventSystem>();
                    _es.gameObject.AddComponent<StandaloneInputModule>();
                }
            }
            GameObjectUtility.SetParentAndAlign(go, parent);
            //注册返回事件
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            Selection.activeObject = go;
        }
    }

}