using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using act.ui.EmojiText;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class EmojiText : Text, IPointerClickHandler
{
    [Header("Emoji")] [SerializeField] private EmojiAsset emojiAsset;

    static Dictionary<string, SpriteInfo> emojiData;
    readonly StringBuilder builder = new StringBuilder();
    readonly Dictionary<int, EmojiInfo> emojis = new Dictionary<int, EmojiInfo>();
    readonly UIVertex[] tempVerts = new UIVertex[4];
    //readonly MatchResult matchResult = new MatchResult();
    //static readonly string regexTag = "\\[([0-9A-Za-z]+)((\\|[0-9]+){0,2})(#[0-9a-f]{6})?(#[^=\\]]+)?(=[^\\]]+)?\\]";
    string outputText = "";

    #region 超链接

    [Serializable]
    public class HrefClickEvent : UnityEvent<string>
    {
    }
    private readonly List<HrefInfo> _listHrefInfos = new List<HrefInfo>();
    public HrefClickEvent OnHrefClick { get; } = new HrefClickEvent();

    #endregion

    public override float preferredWidth
    {
        get
        {
            var settings = GetGenerationSettings(Vector2.zero);
            return cachedTextGeneratorForLayout.GetPreferredWidth(outputText, settings) / pixelsPerUnit;
        }
    }

    public override float preferredHeight
    {
        get
        {
            //var settings = GetGenerationSettings(new Vector2(rectTransform.rect.size.x, 0.0f));
            var settings = GetGenerationSettings(new Vector2(preferredWidth, 0.0f));
            return cachedTextGeneratorForLayout.GetPreferredHeight(outputText, settings) / pixelsPerUnit;
        }
    }

    public float GetPreferredHeight(float width)
    {
        if (width <= 0)
        {
            width = preferredWidth;
        }

        var settings = GetGenerationSettings(new Vector2(width, 0.0f));
        return cachedTextGeneratorForLayout.GetPreferredHeight(outputText, settings) / pixelsPerUnit;
    }
    
    [TextArea(3, 10)]
    [SerializeField]
    protected string _text = string.Empty;

    public override string text
    {
        get { return m_Text; }

        set
        {
            if (String.IsNullOrEmpty(value))
            {
                if (String.IsNullOrEmpty(m_Text))
                    return;
                //m_Text = "";
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

    protected override void Awake()
    {
        base.Awake();

        // only run in playing mode
        if (!Application.isPlaying)
            return;

        if (null == emojiData && null != emojiAsset)
        {
            emojiData = new Dictionary<string, SpriteInfo>();
            foreach (var data in emojiAsset.spriteInfoList)
            {
                if (emojiData.TryGetValue(data.name, out SpriteInfo info))
                {
                    Debug.LogWarning($"key {data.name} has exist!");
                    continue;
                }

                emojiData.Add(data.name, data);
            }
        }
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        if (null != emojiAsset)
        {
            material = emojiAsset.material;
        }
        else
        {
            material = null;
        }
    }
#endif

    private StringBuilder _textBuilder = new StringBuilder();

    // 用正则取  [图集ID#表情Tag] ID值==-1 ,表示为超链接
    private static readonly Regex _inputTagRegex = new Regex(@"\[(\-{0,1}\d{0,})#(.+?)\]", RegexOptions.Singleline);
    
    //表情位置索引信息
    private List<SpriteTagInfo> _spriteInfo = new List<SpriteTagInfo>();

    //根据正则规则更新文本
    private string GetOutputText(string inputText)
    {
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
                // if (_inlineManager == null || !_inlineManager.IndexSpriteInfo.TryGetValue(tempId, out Dictionary<string, SpriteInforGroup> _indexSpriteInfo)
                //                            || !_indexSpriteInfo.TryGetValue(tempTag, out SpriteInforGroup tempGroup))
                //     continue;

                SpriteInforGroup tempGroup = new SpriteInforGroup();// _inlineManager.IndexSpriteInfo[tempId][tempTag];

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

    UIVertex _tempVertex = UIVertex.simpleVert;
    
    //处理表情信息
    private void DealSpriteTagInfo(VertexHelper toFill)
    {
        int index = -1;
        //bool autoLF = AutoLF();
        //emoji 
        for (int i = 0; i < _spriteInfo.Count; i++)
        {
            index = _spriteInfo[i].NewIndex;
            if ((index + 4) <= toFill.currentVertCount)
            {
                for (int j = index; j < index + 4; j++)
                {
                    toFill.PopulateUIVertex(ref _tempVertex, j);
                    //清理多余的乱码uv
                    _tempVertex.uv0 = Vector2.zero;
                    _tempVertex.color = color;
                    //获取quad的位置 --> 转为世界坐标
                    _spriteInfo[i].Pos[j - index] = Utility.TransformPoint2World(transform, _tempVertex.position);
                    toFill.SetUIVertex(_tempVertex, j);
                }
            }
        }
    }

    //处理超链接的信息
    private void DealHrefInfo(VertexHelper toFill)
    {
        if (_listHrefInfos.Count > 0)
        {
            // 处理超链接包围框  
            for (int i = 0; i < _listHrefInfos.Count; i++)
            {
                _listHrefInfos[i].Boxes.Clear();

                int startIndex = _listHrefInfos[i].NewStartIndex;
                int endIndex = _listHrefInfos[i].NewEndIndex;

                if (startIndex >= toFill.currentVertCount)
                    continue;

                toFill.PopulateUIVertex(ref _tempVertex, startIndex);
                // 将超链接里面的文本顶点索引坐标加入到包围框  
                var pos = _tempVertex.position;
                var bounds = new Bounds(pos, Vector3.zero);
                for (int j = startIndex + 1; j < endIndex; j++)
                {
                    if (j >= toFill.currentVertCount)
                    {
                        break;
                    }

                    toFill.PopulateUIVertex(ref _tempVertex, j);
                    pos = _tempVertex.position;
                    if (pos.x < bounds.min.x)
                    {
                        // 换行重新添加包围框  
                        _listHrefInfos[i].Boxes.Add(new Rect(bounds.min, bounds.size));
                        bounds = new Bounds(pos, Vector3.zero);
                    }
                    else
                    {
                        bounds.Encapsulate(pos); // 扩展包围框  
                    }
                }

                //添加包围盒
                _listHrefInfos[i].Boxes.Add(new Rect(bounds.min, bounds.size));
            }
        }
    }
    private List<int> _lastRenderIndexs = new List<int>();
    //表情绘制
    private void UpdateDrawSprite(bool visable)
    {
        //记录之前的信息
        if ((_spriteInfo == null || _spriteInfo.Count == 0) && _lastRenderIndexs.Count > 0)
        {
            for (int i = 0; i < _lastRenderIndexs.Count; i++)
            {
                //_inlineManager.UpdateTextInfo(this, _lastRenderIndexs[i], null, visable);
            }

            _lastRenderIndexs.Clear();
        }
        else
        {
            _lastRenderIndexs.Clear();
            for (int i = 0; i < _spriteInfo.Count; i++)
            {
                //添加渲染id索引
                if (!_lastRenderIndexs.Contains(_spriteInfo[i].Id))
                {
                    //_inlineManager.UpdateTextInfo(this, _spriteInfo[i].Id, _spriteInfo.FindAll(x => x.Id == _spriteInfo[i].Id), visable);
                    _lastRenderIndexs.Add(_spriteInfo[i].Id);
                }
            }
        }
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

    protected override void OnPopulateMesh(VertexHelper toFill)
    {
        if (font == null)
            return;
        base.OnPopulateMesh(toFill);

        m_DisableFontTextureRebuiltCallback = true;
        //更新顶点位置&去掉乱码uv
        DealSpriteTagInfo(toFill);
        //处理超链接的信息
        DealHrefInfo(toFill);
        m_DisableFontTextureRebuiltCallback = false;

        //更新表情绘制
        UpdateDrawSprite(true);
    }
    

    // protected override void OnPopulateMesh(VertexHelper toFill)
    // {
    //     if (font == null)
    //         return;
    //
    //     if (string.IsNullOrEmpty(m_Text))
    //     {
    //         base.OnPopulateMesh(toFill);
    //         return;
    //     }
    //
    //     ParseText(m_Text);
    //
    //     // We don't care if we the font Texture changes while we are doing our Update.
    //     // The end result of cachedTextGenerator will be valid for this instance.
    //     // Otherwise we can get issues like Case 619238.
    //     m_DisableFontTextureRebuiltCallback = true;
    //
    //
    //     Vector2 extents = rectTransform.rect.size;
    //
    //     var settings = GetGenerationSettings(extents);
    //     cachedTextGenerator.Populate(outputText, settings);
    //
    //     // Apply the offset to the vertices
    //     IList<UIVertex> verts = cachedTextGenerator.verts;
    //     float unitsPerPixel = 1 / pixelsPerUnit;
    //     //Last 4 verts are always a new line... (\n)
    //     int vertCount = verts.Count - 4;
    //
    //     // We have no verts to process just return (case 1037923)
    //     if (vertCount <= 0)
    //     {
    //         toFill.Clear();
    //         return;
    //     }
    //
    //     Vector3 repairVec = new Vector3(0, fontSize * 0.1f);
    //     Vector2 roundingOffset = new Vector2(verts[0].position.x, verts[0].position.y) * unitsPerPixel;
    //     roundingOffset = PixelAdjustPoint(roundingOffset) - roundingOffset;
    //     toFill.Clear();
    //     if (roundingOffset != Vector2.zero)
    //     {
    //         for (int i = 0; i < vertCount; ++i)
    //         {
    //             int tempVertsIndex = i & 3;
    //             tempVerts[tempVertsIndex] = verts[i];
    //             tempVerts[tempVertsIndex].position *= unitsPerPixel;
    //             tempVerts[tempVertsIndex].position.x += roundingOffset.x;
    //             tempVerts[tempVertsIndex].position.y += roundingOffset.y;
    //             if (tempVertsIndex == 3)
    //             {
    //                 toFill.AddUIVertexQuad(tempVerts);
    //             }
    //         }
    //     }
    //     else
    //     {
    //         Vector4 uv = Vector4.zero;
    //         for (int i = 0; i < vertCount; ++i)
    //         {
    //             int index = i / 4;
    //             int tempVertIndex = i & 3;
    //
    //             if (emojis.TryGetValue(index, out EmojiInfo info))
    //             {
    //                 tempVerts[tempVertIndex] = verts[i];
    //                 tempVerts[tempVertIndex].position -= repairVec;
    //                 if (info.type == MatchType.Emoji)
    //                 {
    //                     uv.x = info.sprite.index;
    //                     uv.y = info.sprite.frameCount;
    //                     tempVerts[tempVertIndex].uv0 += uv * 10;
    //                 }
    //                 else
    //                 {
    //                     tempVerts[tempVertIndex].position = tempVerts[0].position;
    //                 }
    //
    //                 tempVerts[tempVertIndex].position *= unitsPerPixel;
    //                 if (tempVertIndex == 3)
    //                 {
    //                     toFill.AddUIVertexQuad(tempVerts);
    //                 }
    //             }
    //             else
    //             {
    //                 tempVerts[tempVertIndex] = verts[i];
    //                 tempVerts[tempVertIndex].position *= unitsPerPixel;
    //                 if (tempVertIndex == 3)
    //                 {
    //                     toFill.AddUIVertexQuad(tempVerts);
    //                 }
    //             }
    //         }
    //
    //         CalcBoundsInfo(toFill);
    //         DrawUnderline(toFill);
    //     }
    //
    //     m_DisableFontTextureRebuiltCallback = false;
    // }

    // void ParseText(string mText)
    // {
    //     if (emojiData == null || !Application.isPlaying)
    //     {
    //         outputText = mText;
    //         return;
    //     }
    //
    //     builder.Length = 0;
    //     emojis.Clear();
    //     hrefs.Clear();
    //
    //     MatchCollection matches = Regex.Matches(mText, regexTag);
    //     if (matches.Count > 0)
    //     {
    //         int textIndex = 0;
    //         for (int i = 0; i < matches.Count; i++)
    //         {
    //             var match = matches[i];
    //             matchResult.Parse(match, fontSize);
    //
    //             switch (matchResult.type)
    //             {
    //                 case MatchType.Emoji:
    //                 {
    //                     if (emojiData.TryGetValue(matchResult.title, out SpriteInfo info))
    //                     {
    //                         builder.Append(mText.Substring(textIndex, match.Index - textIndex));
    //                         int temIndex = builder.Length;
    //
    //                         builder.Append("<quad size=");
    //                         builder.Append(matchResult.height);
    //                         builder.Append(" width=");
    //                         builder.Append((matchResult.width * 1.0f / matchResult.height).ToString("f2"));
    //                         builder.Append(" />");
    //
    //                         emojis.Add(temIndex, new EmojiInfo()
    //                         {
    //                             type = MatchType.Emoji,
    //                             sprite = info,
    //                             width = matchResult.width,
    //                             height = matchResult.height
    //                         });
    //
    //                         if (matchResult.HasUrl)
    //                         {
    //                             var hrefInfo = new HrefInfo()
    //                             {
    //                                 show = false,
    //                                 NewStartIndex = temIndex * 4,
    //                                 NewEndIndex = temIndex * 4 + 3,
    //                                 HrefValue = matchResult.url,
    //                                 color = matchResult.GetColor(color)
    //                             };
    //                             hrefs.Add(hrefInfo);
    //                         }
    //
    //                         textIndex = match.Index + match.Length;
    //                     }
    //
    //                     break;
    //                 }
    //                 case MatchType.HyperLink:
    //                 {
    //                     builder.Append(mText.Substring(textIndex, match.Index - textIndex));
    //                     builder.Append("<color=");
    //                     builder.Append(matchResult.GetHexColor(color));
    //                     builder.Append(">");
    //
    //                     var href = new HrefInfo
    //                     {
    //                         show = true,
    //                         NewStartIndex = builder.Length * 4
    //                     };
    //                     builder.Append(matchResult.link);
    //                     href.NewEndIndex = builder.Length * 4 - 1;
    //                     href.HrefValue = matchResult.url;
    //                     href.color = matchResult.GetColor(color);
    //
    //                     hrefs.Add(href);
    //                     builder.Append("</color>");
    //
    //                     textIndex = match.Index + match.Length;
    //                     break;
    //                 }
    //             }
    //         }
    //
    //         builder.Append(mText.Substring(textIndex, mText.Length - textIndex));
    //         outputText = builder.ToString();
    //     }
    //     else
    //     {
    //         outputText = mText;
    //     }
    // }

    /// <summary>
    /// 计算可点击的富文本部分的包围盒
    /// </summary>
    /// <param name="toFill"></param>
    void CalcBoundsInfo(VertexHelper toFill)
    {
        UIVertex vert = new UIVertex();
        for (int u = 0; u < _listHrefInfos.Count; u++)
        {
            var href = _listHrefInfos[u];
            href.Boxes.Clear();
            if (href.NewStartIndex >= toFill.currentVertCount)
                continue;

            // Add hyper text vector index to bounds
            toFill.PopulateUIVertex(ref vert, href.NewStartIndex);
            var pos = vert.position;
            var bounds = new Bounds(pos, Vector3.zero);
            for (int i = href.NewStartIndex, m = href.NewEndIndex; i < m; i++)
            {
                if (i >= toFill.currentVertCount) break;

                toFill.PopulateUIVertex(ref vert, i);
                pos = vert.position;
                if (pos.x < bounds.min.x)
                {
                    //if in different lines
                    href.Boxes.Add(new Rect(bounds.min, bounds.size));
                    bounds = new Bounds(pos, Vector3.zero);
                }
                else
                {
                    bounds.Encapsulate(pos); //expand bounds
                }
            }

            //add bound
            href.Boxes.Add(new Rect(bounds.min, bounds.size));
        }
    }

    void DrawUnderline(VertexHelper toFill)
    {
        if (_listHrefInfos.Count <= 0)
        {
            return;
        }

        Vector2 extents = rectTransform.rect.size;
        var settings = GetGenerationSettings(extents);
        cachedTextGenerator.Populate("_", settings);
        IList<UIVertex> uList = cachedTextGenerator.verts;
        float h = uList[2].position.y - uList[1].position.y;
        Vector3[] temVecs = new Vector3[4];

        for (int i = 0; i < _listHrefInfos.Count; i++)
        {
            var info = _listHrefInfos[i];
            if (!info.show)
            {
                continue;
            }

            for (int j = 0; j < info.Boxes.Count; j++)
            {
                if (info.Boxes[j].width <= 0 || info.Boxes[j].height <= 0)
                {
                    continue;
                }

                temVecs[0] = info.Boxes[j].min;
                temVecs[1] = temVecs[0] + new Vector3(info.Boxes[j].width, 0);
                temVecs[2] = temVecs[0] + new Vector3(info.Boxes[j].width, -h);
                temVecs[3] = temVecs[0] + new Vector3(0, -h);

                for (int k = 0; k < 4; k++)
                {
                    tempVerts[k] = uList[k];
                    tempVerts[k].color = info.color;
                    tempVerts[k].position = temVecs[k];
                    tempVerts[k].uv0 = GetUnderlineCharUV();
                }

                toFill.AddUIVertexQuad(tempVerts);
            }
        }
    }

    private Vector2 GetUnderlineCharUV()
    {
        if (font.GetCharacterInfo('_', out CharacterInfo info, fontSize, fontStyle))
        {
            return (info.uvBottomLeft + info.uvBottomRight + info.uvTopLeft + info.uvTopRight) * 0.25f;
        }

        return Vector2.zero;
    }

    void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform, eventData.position, eventData.pressEventCamera, out Vector2 lp);

        for (int h = 0; h < _listHrefInfos.Count; h++)
        {
            var hrefInfo = _listHrefInfos[h];
            var boxes = hrefInfo.Boxes;
            for (var i = 0; i < boxes.Count; ++i)
            {
                if (boxes[i].Contains(lp))
                {
                    OnHrefClick.Invoke(hrefInfo.HrefValue);
                    return;
                }
            }
        }
    }

    class EmojiInfo
    {
        public MatchType type;
        public int width;
        public int height;
        public SpriteInfo sprite;
    }

    enum MatchType
    {
        None,
        Emoji,
        HyperLink,
    }

    class MatchResult
    {
        public MatchType type;
        public string title;
        public string url;
        public string link;
        public int height;
        public int width;
        private string strColor;
        private Color color;

        public bool HasUrl
        {
            get { return !string.IsNullOrEmpty(url); }
        }

        void Reset()
        {
            type = MatchType.None;
            title = String.Empty;
            width = 0;
            height = 0;
            strColor = string.Empty;
            url = string.Empty;
            link = string.Empty;
        }

        public void Parse(Match match, int fontSize)
        {
            Reset();
            if (!match.Success || match.Groups.Count != 7)
                return;
            title = match.Groups[1].Value;
            if (match.Groups[2].Success)
            {
                string v = match.Groups[2].Value;
                string[] sp = v.Split('|');
                height = sp.Length > 1 ? int.Parse(sp[1]) : fontSize;
                width = sp.Length == 3 ? int.Parse(sp[2]) : height;
            }
            else
            {
                height = fontSize;
                width = fontSize;
            }

            if (match.Groups[4].Success)
            {
                strColor = match.Groups[4].Value.Substring(1);
                strColor = "#" + strColor;
            }

            if (match.Groups[5].Success)
            {
                url = match.Groups[5].Value.Substring(1);
            }

            if (match.Groups[6].Success)
            {
                link = match.Groups[6].Value.Substring(1);
            }

            if (title.Equals("0x01")) //hyper link
            {
                if (!string.IsNullOrEmpty(url) && !string.IsNullOrEmpty(link))
                {
                    type = MatchType.HyperLink;
                }
            }

            if (type == MatchType.None)
            {
                type = MatchType.Emoji;
            }
        }

        public Color GetColor(Color fontColor)
        {
            if (string.IsNullOrEmpty(strColor))
                return fontColor;
            ColorUtility.TryParseHtmlString(strColor, out color);
            return color;
        }

        public string GetHexColor(Color fontColor)
        {
            if (!string.IsNullOrEmpty(strColor))
                return strColor;
            return ColorUtility.ToHtmlStringRGBA(fontColor);
        }
    }

    class HrefInfo
    {
        /// <summary>
        /// 超链接id
        /// </summary>
        public int Id;
        /// <summary>
        /// 是否绘制下划线
        /// </summary>
        public bool show;
        /// <summary>
        /// 名称
        /// </summary>
        public string Name;
        public int NewStartIndex;
        public int NewEndIndex;
        public Color color;
        public readonly List<Rect> Boxes = new List<Rect>();
        public string HrefValue;
    }
    /// <summary>
    /// 图片的信息
    /// </summary>
    public class SpriteTagInfo
    {
        /// <summary>
        /// 为了兼容unity2019 单行的顶点的索引
        /// </summary>
        public int NewIndex;
        /// <summary>
        /// 图集id
        /// </summary>
        public int Id;
        /// <summary>
        /// 标签标签
        /// </summary>
        public string Tag;
        /// <summary>
        /// 标签大小
        /// </summary>
        public Vector2 Size;
        /// <summary>
        /// 表情位置
        /// </summary>
        public Vector3[] Pos = new Vector3[4];
        /// <summary>
        /// uv
        /// </summary>
        public Vector2[] UVs = new Vector2[4];

        public Color ColorData;
    }
}