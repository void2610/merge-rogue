using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

/// <summary>
/// 仮想マウス管理サービス
/// MyVirtualMouseInputとの連携を管理します
/// </summary>
public class VirtualMouseService : IVirtualMouseService
{
    private readonly CursorConfiguration _cursorConfiguration;
    private readonly MyVirtualMouseInput _virtualMouseInput;
    private readonly Image _virtualMouseImage;
    
    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="cursorConfiguration">カーソル設定</param>
    public VirtualMouseService(CursorConfiguration cursorConfiguration)
    {
        _cursorConfiguration = cursorConfiguration ?? throw new System.ArgumentNullException(nameof(cursorConfiguration));
        _cursorConfiguration.Initialize();
        _virtualMouseInput = Object.FindFirstObjectByType<MyVirtualMouseInput>();
        _virtualMouseImage = _virtualMouseInput.GetComponent<Image>();
    }
    
    /// <summary>
    /// 仮想マウスのスプライトを設定します
    /// </summary>
    /// <param name="iconType">設定するカーソルタイプ</param>
    public void SetVirtualMouseSprite(CursorIconType iconType)
    {
        var cursorData = _cursorConfiguration.GetCursorData(iconType);
        _virtualMouseImage.sprite = cursorData.sprite;
    }
    
    /// <summary>
    /// 仮想マウスが有効かどうかを取得します
    /// </summary>
    /// <returns>有効な場合true</returns>
    public bool IsVirtualMouseActive()
    {
        return _virtualMouseInput && _virtualMouseInput.isActive;
    }
    
    /// <summary>
    /// 仮想マウスの位置を設定します
    /// </summary>
    /// <param name="position">設定する位置</param>
    public void SetVirtualMousePosition(Vector2 position)
    {
        if (_virtualMouseInput?.virtualMouse != null)
        {
            InputState.Change(_virtualMouseInput.virtualMouse.position, position);
            _virtualMouseInput.transform.position = position;
        }
    }
    
    /// <summary>
    /// 仮想マウスを中央に移動します
    /// </summary>
    public void MoveVirtualMouseToCenter()
    {
        var centerPos = new Vector2(Screen.width / 2f, Screen.height / 2f);
        SetVirtualMousePosition(centerPos);
        
        // UI上の表示位置もリセット
        _virtualMouseInput.transform.position = Vector2.zero;
    }
    
    /// <summary>
    /// 仮想マウスの有効/無効を切り替えます
    /// </summary>
    public void ToggleVirtualMouse()
    {
        SetVirtualMouseActive(!IsVirtualMouseActive());
    }
    
    /// <summary>
    /// 仮想マウスを有効/無効に設定します
    /// </summary>
    /// <param name="active">有効にする場合true</param>
    public void SetVirtualMouseActive(bool active)
    {
        if (active)
        {
            _virtualMouseInput.isActive = true;
            EventSystem.current.sendNavigationEvents = false;
            MoveVirtualMouseToCenter();
        }
        else
        {
            _virtualMouseInput.isActive = false;
            EventSystem.current.sendNavigationEvents = true;
            // 画面外に移動
            _virtualMouseInput.transform.position = new Vector2(-1000, -1000);
        }
    }
}