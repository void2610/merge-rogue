using UnityEngine;

/// <summary>
/// 仮想マウス管理サービスのインターフェース
/// </summary>
public interface IVirtualMouseService
{
    /// <summary>
    /// 仮想マウスのスプライトを設定します
    /// </summary>
    /// <param name="iconType">設定するカーソルタイプ</param>
    void SetVirtualMouseSprite(CursorIconType iconType);
    
    /// <summary>
    /// 仮想マウスが有効かどうかを取得します
    /// </summary>
    /// <returns>有効な場合true</returns>
    bool IsVirtualMouseActive();
    
    /// <summary>
    /// 仮想マウスの位置を設定します
    /// </summary>
    /// <param name="position">設定する位置</param>
    void SetVirtualMousePosition(Vector2 position);
    
    /// <summary>
    /// 仮想マウスを中央に移動します
    /// </summary>
    void MoveVirtualMouseToCenter();
    
    /// <summary>
    /// 仮想マウスの有効/無効を切り替えます
    /// </summary>
    void ToggleVirtualMouse();
    
    /// <summary>
    /// 仮想マウスを有効/無効に設定します
    /// </summary>
    /// <param name="active">有効にする場合true</param>
    void SetVirtualMouseActive(bool active);
}