using UnityEngine;

public class InputProvider : MonoBehaviour
{
    public static InputProvider Instance { get; private set; }
    public InputSystem_Actions.GameplayActions Gameplay => _inputActions.Gameplay;
    public InputSystem_Actions.UIActions UI => _inputActions.UI;
    private InputSystem_Actions _inputActions;
    
    public Vector2 GetMousePosition () => _inputActions.UI.MousePosition.ReadValue<Vector2>();
    public bool IsSkipButtonPressed() => _inputActions.UI.Skip.triggered;
    
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);
        
        _inputActions = new InputSystem_Actions();
        Gameplay.Enable();
        UI.Enable();
    }
    
    private void OnDestroy()
    {
        Gameplay.Disable();
        UI.Disable();
    }
}
