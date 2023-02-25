using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using System.Text;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System;
using act.ui.EmojiText;

[ExecuteInEditMode]
public class TestEmojiText : Text
{
    // 用正则取  [图集ID#表情Tag] ID值==-1 ,表示为超链接
    private static readonly Regex _inputTagRegex = new Regex(@"\[(\-{0,1}\d{0,})#(.+?)\]", RegexOptions.Singleline);

    //表情位置索引信息
    private List<SpriteTagInfo> _spriteInfo = new List<SpriteTagInfo>();

    //计算定点信息的缓存数组
    private readonly UIVertex[] m_TempVerts = new UIVertex[4];

    private StringBuilder _textBuilder = new StringBuilder();

    UIVertex _tempVertex = UIVertex.simpleVert;
    private List<int> _lastRenderIndexs = new List<int>();

    #region 超链接

    [System.Serializable]
    public class HrefClickEvent : UnityEvent<string, int>
    {
    }

    //点击事件监听
    public HrefClickEvent OnHrefClick = new HrefClickEvent();

    // 超链接信息列表  
    private readonly List<HrefInfo> _listHrefInfos = new List<HrefInfo>();

    public bool HasHref => _listHrefInfos.Count > 0;

    #endregion


    #region 重写函数

    [TextArea(3, 10)] [SerializeField] protected string _text = string.Empty;

    public override string text
    {
        get { return m_Text; }
        set
        {
            if (String.IsNullOrEmpty(value))
            {
                if (String.IsNullOrEmpty(m_Text))
                    return;
                m_Text = GetOutputText(value);
                SetVerticesDirty();
            }
            else if (_text != value)
            {
                m_Text = GetOutputText(value);
                //m_Text = value;
                SetVerticesDirty();
                SetLayoutDirty();
            }
#if UNITY_EDITOR
            //编辑器赋值 如果是一样的 也可以刷新一下
            else
            {
                m_Text = GetOutputText(value);
                SetVerticesDirty();
                SetLayoutDirty();
            }
#endif
            //输入字符备份
            _text = value;
        }
    }

    private const string _defaultShader = "Hidden/UI/Emoji";
    private Material _defaultMater = null;
    public override Material material
    {
        get
        {
            if (_defaultMater == null && EmojiTextSpriteMgr.spriteAssets != null)
            {
                _defaultMater = new Material(Shader.Find(_defaultShader));
                //是否开启动画
                // if (m_spriteAsset.IsStatic)
                    _defaultMater.DisableKeyword("EMOJI_ANIMATION");
                // else
                // {
                //     _defaultMater.EnableKeyword("EMOJI_ANIMATION");
                //     _defaultMater.SetFloat("_CellAmount", m_spriteAsset.Column);
                //     _defaultMater.SetFloat("_Speed", m_spriteAsset.Speed);
                // }
            }
            return _defaultMater;
        }
    }
    
    private string GetOutputText(string inputText)
    {
        //回收各种对象
        ReleaseSpriteTageInfo();
        ReleaseHrefInfos();

        if (string.IsNullOrEmpty(inputText))
            return "";

        _textBuilder.Remove(0, _textBuilder.Length);
        int textIndex = 0;
        int newIndex = 0;
        string part = "";

        foreach (Match match in _inputTagRegex.Matches(inputText))
        {
            int tempId = 0;
            if (!string.IsNullOrEmpty(match.Groups[1].Value) && !match.Groups[1].Value.Equals("-"))
                tempId = int.Parse(match.Groups[1].Value);
            string tempTag = match.Groups[2].Value;
            //更新超链接
            if (tempId < 0)
            {
                part = inputText.Substring(textIndex, match.Index - textIndex);
                _textBuilder.Append(part);
                _textBuilder.Append($"<color=#0000FF{String.Format("{0:X2}", (byte) (color.a * byte.MaxValue))}>");
                int startIndex = _textBuilder.Length * 4;
                _textBuilder.Append("[" + match.Groups[2].Value + "]");
                int endIndex = _textBuilder.Length * 4 - 1;
                _textBuilder.Append("</color>");
                newIndex += ReplaceRichText(part).Length * 4;
                int newStartIndex = newIndex;
                newIndex += match.Groups[2].Value.Length * 4 + 8;

                var hrefInfo = act.ui.EmojiText.Pool<HrefInfo>.Get();
                hrefInfo.Id = Mathf.Abs(tempId);
                hrefInfo.NewStartIndex = newStartIndex;
                hrefInfo.NewEndIndex = newIndex - 1;
                hrefInfo.Name = match.Groups[2].Value;
                hrefInfo.HrefValue = match.Groups[3].Value;
                _listHrefInfos.Add(hrefInfo);
            }
            //更新表情
            else
            {
                if (EmojiTextSpriteMgr.spriteInfoDict == null
                    || !EmojiTextSpriteMgr.spriteInfoDict.TryGetValue(tempId, out Dictionary<string, SpriteInforGroup> _indexSpriteInfo)
                    || !_indexSpriteInfo.TryGetValue(tempTag, out SpriteInforGroup tempGroup))
                {
                    continue;
                }

                part = inputText.Substring(textIndex, match.Index - textIndex);
                _textBuilder.Append(part);
                int tempIndex = _textBuilder.Length * 4;
                newIndex += ReplaceRichText(part).Length * 4;
                _textBuilder.Append(@"<quad size=" + tempGroup.Size + " width=" + tempGroup.Width + " />");

                //清理标签
                SpriteTagInfo tempSpriteTag = Pool<SpriteTagInfo>.Get();
                tempSpriteTag.NewIndex = newIndex;
                tempSpriteTag.Id = tempId;
                tempSpriteTag.Tag = tempTag;
                tempSpriteTag.Size = new Vector2(tempGroup.Size * tempGroup.Width, tempGroup.Size);
                tempSpriteTag.UVs = tempGroup.ListSpriteInfor[0].Uv;
                tempSpriteTag.ColorData = new Color(1, 1, 1, color.a);

                //添加正则表达式的信息
                _spriteInfo.Add(tempSpriteTag);
                newIndex += 4;
            }

            textIndex = match.Index + match.Length;
        }

        _textBuilder.Append(inputText.Substring(textIndex, inputText.Length - textIndex));
        return _textBuilder.ToString();
    }

    //回收SpriteTagInfo
    private void ReleaseSpriteTageInfo()
    {
        //记录之前的信息
        for (int i = 0; i < _spriteInfo.Count; i++)
        {
            //回收信息到对象池
            Pool<SpriteTagInfo>.Release(_spriteInfo[i]);
        }

        _spriteInfo.Clear();
    }

    //回收超链接的信息
    private void ReleaseHrefInfos()
    {
        for (int i = 0; i < _listHrefInfos.Count; i++)
        {
            Pool<HrefInfo>.Release(_listHrefInfos[i]);
        }

        _listHrefInfos.Clear();
    }

    //换掉富文本
    private string ReplaceRichText(string str)
    {
        str = Regex.Replace(str, @"<color=(.+?)>", "");
        str = str.Replace("</color>", "");
        str = str.Replace("<b>", "");
        str = str.Replace("</b>", "");
        str = str.Replace("<i>", "");
        str = str.Replace("</i>", "");
        str = str.Replace("\n", "");
        str = str.Replace("\t", "");
        str = str.Replace("\r", "");
        str = str.Replace(" ", "");

        return str;
    }

    #endregion
}