using UnityEngine;

/// <summary>
/// 入力を提供するインターフェース
/// VContainerでDI管理するための抽象化
/// </summary>
public interface IInputProvider
{
    InputSystem_Actions.GameplayActions Gameplay { get; }
    InputSystem_Actions.UIActions UI { get; }
    
    Vector2 GetMousePosition();
    bool IsSkipButtonPressed();
    Vector2 GetScrollSpeed();
}