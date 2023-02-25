using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using act.ui.EmojiText;

[ExecuteInEditMode]
public class EmojiTextSpriteMgr : MonoBehaviour
{
    //图集
    [SerializeField]
    private List<SpriteAsset> _spriteAssets = new List<SpriteAsset>();

    public static List<SpriteAsset> spriteAssets;
    public static Dictionary<int, Dictionary<string, SpriteInforGroup>> spriteInfoDict = new Dictionary<int, Dictionary<string, SpriteInforGroup>>();

    private void OnEnable()
    {
        spriteAssets = _spriteAssets;
        foreach (var assets in spriteAssets)
        {
            if (spriteInfoDict.TryGetValue(assets.Id, out var spriteDict))
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
        }
    }
}
