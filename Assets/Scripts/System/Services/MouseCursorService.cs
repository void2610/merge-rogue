using UnityEngine;

/// <summary>
/// マウスカーソル管理サービス
/// 純粋なC#クラスとしてカーソルの状態管理を行います
/// </summary>
public class MouseCursorService : IMouseCursorService
{
    private readonly CursorConfiguration _cursorConfiguration;
    private readonly IVirtualMouseService _virtualMouseService;
    private CursorIconType _currentCursorType;
    
    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="cursorConfiguration">カーソル設定</param>
    /// <param name="virtualMouseService">仮想マウスサービス</param>
    public MouseCursorService(CursorConfiguration cursorConfiguration, IVirtualMouseService virtualMouseService)
    {
        _cursorConfiguration = cursorConfiguration ?? throw new System.ArgumentNullException(nameof(cursorConfiguration));
        _virtualMouseService = virtualMouseService ?? throw new System.ArgumentNullException(nameof(virtualMouseService));
        
        _cursorConfiguration.Initialize();
        _currentCursorType = CursorIconType.Default;
        
        // 初期カーソルを設定
        SetCursor(CursorIconType.Default);
    }
    
    /// <summary>
    /// カーソルを指定されたタイプに設定します
    /// </summary>
    /// <param name="iconType">設定するカーソルタイプ</param>
    public void SetCursor(CursorIconType iconType)
    {
        var cursorData = _cursorConfiguration.GetCursorData(iconType);
        
        // システムカーソルを設定
        Cursor.SetCursor(cursorData.texture, cursorData.hotspot, CursorMode.Auto);
        
        // 仮想マウスのスプライトも更新
        _virtualMouseService?.SetVirtualMouseSprite(iconType);
        
        _currentCursorType = iconType;
    }
    
    /// <summary>
    /// カーソルをデフォルトに戻します
    /// </summary>
    public void ResetCursor()
    {
        SetCursor(CursorIconType.Default);
    }
}