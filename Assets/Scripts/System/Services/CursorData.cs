using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// カーソルアイコンの設定データ
/// </summary>
[Serializable]
public struct CursorIconData
{
    public CursorIconType iconType;
    public Texture2D texture;
    public Sprite sprite;
    public Vector2 hotspot;
}

/// <summary>
/// カーソル設定の管理クラス
/// </summary>
[Serializable]
public class CursorConfiguration
{
    [SerializeField] private List<CursorIconData> cursorIcons = new List<CursorIconData>();
    
    private Dictionary<CursorIconType, CursorIconData> _cursorDataCache;
    
    /// <summary>
    /// カーソルデータのキャッシュを初期化します
    /// </summary>
    public void Initialize()
    {
        _cursorDataCache = new Dictionary<CursorIconType, CursorIconData>();
        foreach (var iconData in cursorIcons)
        {
            _cursorDataCache[iconData.iconType] = iconData;
        }
    }
    
    /// <summary>
    /// 指定されたタイプのカーソルデータを取得します
    /// </summary>
    /// <param name="iconType">カーソルアイコンタイプ</param>
    /// <returns>カーソルデータ</returns>
    public CursorIconData GetCursorData(CursorIconType iconType)
    {
        if (_cursorDataCache == null) Initialize();
        
        return _cursorDataCache.TryGetValue(iconType, out var data) 
            ? data 
            : _cursorDataCache.GetValueOrDefault(CursorIconType.Default);
    }
    
    /// <summary>
    /// カーソルデータが存在するかチェックします
    /// </summary>
    /// <param name="iconType">カーソルアイコンタイプ</param>
    /// <returns>存在する場合true</returns>
    public bool HasCursorData(CursorIconType iconType)
    {
        if (_cursorDataCache == null) Initialize();
        return _cursorDataCache.ContainsKey(iconType);
    }
}