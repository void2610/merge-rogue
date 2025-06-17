using UnityEngine;
using System;

/// <summary>
/// 純粋なC#クラスとして実装された入力プロバイダーサービス
/// VContainerで管理され、全シーンで共有される
/// </summary>
public class InputProviderService : IInputProvider, IDisposable
{
    private readonly InputSystem_Actions _inputActions;
    
    public InputSystem_Actions.GameplayActions Gameplay => _inputActions.Gameplay;
    public InputSystem_Actions.UIActions UI => _inputActions.UI;
    
    public Vector2 GetMousePosition() => _inputActions.Gameplay.MousePosition.ReadValue<Vector2>();
    public bool IsSkipButtonPressed() => _inputActions.UI.Skip.triggered;
    public Vector2 GetScrollSpeed() => _inputActions.UI.Scroll.ReadValue<Vector2>();
    
    public InputProviderService()
    {
        _inputActions = new InputSystem_Actions();
        Gameplay.Enable();
        UI.Enable();
    }
    
    public void Dispose()
    {
        Gameplay.Disable();
        UI.Disable();
        _inputActions?.Dispose();
    }
}