using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using act.ui.EmojiText;
using UnityEditor;

[ExecuteInEditMode]
public class EmojiTextSpriteMgr : MonoBehaviour
{
    //图集
    [SerializeField] private List<act.ui.EmojiText.SpriteAsset> _spriteAssets = new List<act.ui.EmojiText.SpriteAsset>();

    public static Dictionary<int, act.ui.EmojiText.SpriteAsset> spriteAssets;
    public static Dictionary<int, Dictionary<string, SpriteInforGroup>> spriteInfoDict = new Dictionary<int, Dictionary<string, SpriteInforGroup>>();
    
    public static List<SpriteGraphic> _spriteGraphics = new List<SpriteGraphic>();//绘制的模型数据信息
    private static readonly Dictionary<int, Dictionary<TestEmojiText, MeshInfo>> _graphicMeshInfo = new Dictionary<int, Dictionary<TestEmojiText, MeshInfo>>();
    
    private void OnEnable()
    {
        foreach (SpriteAsset assets in _spriteAssets)
        {
            if (spriteInfoDict.TryGetValue(assets.Id, out Dictionary<string, SpriteInforGroup> spriteDict))
            {
                spriteDict.Clear();
            }
            else
            {
                spriteDict = new Dictionary<string, SpriteInforGroup>();
            }

            foreach (var item in assets.ListSpriteGroup)
            {
                if (!spriteDict.ContainsKey(item.Tag) && item.ListSpriteInfor != null && item.ListSpriteInfor.Count > 0)
                {
                    spriteDict.Add(item.Tag, item);
                }
            }

            spriteInfoDict[assets.Id] = spriteDict;
            spriteAssets[assets.Id] = assets;
        }
    }


    //渲染列表
    static List<int> _renderIndexs = new List<int>();

    private void Update()
    {
        if (_renderIndexs != null && _renderIndexs.Count > 0)
        {
            for (int i = 0; i < _renderIndexs.Count; i++)
            {
                int id = _renderIndexs[i];
                SpriteGraphic spriteGraphic = _spriteGraphics.Find(x => x.m_spriteAsset != null && x.m_spriteAsset.Id == id);
                if (spriteGraphic != null)
                {
                    if (!_graphicMeshInfo.TryGetValue(id, out Dictionary<TestEmojiText, MeshInfo> textMeshInfo))
                    {
                        spriteGraphic.MeshInfo = null;
                        continue;
                    }

                    //Dictionary<InlineText, MeshInfo> textMeshInfo = _graphicMeshInfo[id];
                    if (textMeshInfo == null || textMeshInfo.Count == 0)
                        spriteGraphic.MeshInfo = null;
                    else
                    {
                        MeshInfo meshInfo = Pool<MeshInfo>.Get();
                        meshInfo.Reset();
                        foreach (var item in textMeshInfo)
                        {
                            if (item.Value.visable)
                            {
                                meshInfo.Vertices.AddRange(item.Value.Vertices);
                                meshInfo.UVs.AddRange(item.Value.UVs);
                                meshInfo.Colors.AddRange(item.Value.Colors);
                            }
                        }

                        if (spriteGraphic.MeshInfo != null)
                            Pool<MeshInfo>.Release(spriteGraphic.MeshInfo);

                        spriteGraphic.MeshInfo = meshInfo;
                    }
                }
            }

            //清掉渲染索引
            _renderIndexs.Clear();
        }
    }

    public static void SetSpriteGraphic(int id, SpriteGraphic graphic)
    {
        _spriteGraphics.Add(graphic);
    }
    
    //更新Text文本信息
    public static void UpdateTextInfo(TestEmojiText key, int id, List<SpriteTagInfo> value, bool visable)
    {
        Dictionary<TestEmojiText, MeshInfo> textMeshInfo;
        if (value == null)
        {
            if (_graphicMeshInfo.TryGetValue(id, out textMeshInfo) && textMeshInfo.TryGetValue(key, out MeshInfo meshInfo))
            {
                meshInfo.Release();
                textMeshInfo.Remove(key);
            }
        }
        else
        {
            SpriteGraphic spriteGraphic = _spriteGraphics.Find(x => x.m_spriteAsset != null && x.m_spriteAsset.Id == id);
            if (spriteGraphic != null)
            {
                if (!_graphicMeshInfo.TryGetValue(id, out textMeshInfo))
                {
                    textMeshInfo = new Dictionary<TestEmojiText, MeshInfo>();
                    _graphicMeshInfo.Add(id, textMeshInfo);
                }

                if (!textMeshInfo.TryGetValue(key, out MeshInfo meshInfo))
                {
                    meshInfo = Pool<MeshInfo>.Get();
                    textMeshInfo.Add(key, meshInfo);
                }

                meshInfo.Reset();
                meshInfo.visable = visable;
                for (int i = 0; i < value.Count; i++)
                {
                    for (int j = 0; j < value[i].Pos.Length; j++)
                    {
                        //世界转本地坐标->避免位置变换的错位
                        meshInfo.Vertices.Add(Utility.TransformWorld2Point(spriteGraphic.transform, value[i].Pos[j]));
                    }

                    meshInfo.UVs.AddRange(value[i].UVs);
                    meshInfo.Colors.Add(value[i].ColorData);
                }
            }
        }

        //添加到渲染列表里面  --  等待下一帧渲染
        if (!_renderIndexs.Contains(id))
        {
            _renderIndexs.Add(id);
        }
    }
}