/// <summary>
/// マウスカーソル管理サービスのインターフェース
/// </summary>
public interface IMouseCursorService
{
    /// <summary>
    /// カーソルを指定されたタイプに設定する
    /// </summary>
    /// <param name="iconType">設定するカーソルタイプ</param>
    void SetCursor(CursorIconType iconType);
    
    /// <summary>
    /// カーソルをデフォルトに戻す
    /// </summary>
    void ResetCursor();
}