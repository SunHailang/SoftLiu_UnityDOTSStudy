using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Example : MonoBehaviour
{
    public EmojiText emojiText;
    public Button m_btnChange = null;

    // Start is called before the first frame update
    void Start()
    {
        emojiText.OnHrefClick.AddListener(OnHrefClick);
    }
    
    
    
    void OnHrefClick(string msg)
    {
        Debug.Log(msg);

        Debug.Log($"{emojiText.preferredHeight},{emojiText.preferredWidth}");
    }
}
