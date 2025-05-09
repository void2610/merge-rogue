using R3;
using UnityEngine;
using UnityEngine.InputSystem;

public class MergeSkill : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    public bool IsAiming => _isAiming;
    private bool _isAiming = false;

    private void OnPressRightClick(InputAction.CallbackContext ctx)
    {
        if (GameManager.Instance.State.CurrentValue != GameManager.GameState.Merge) return;
        
        if (!_isAiming) StartAim();
        else EndAim();
    }

    private void OnPressLeftClick(InputAction.CallbackContext ctx)
    {
        if (GameManager.Instance.State.CurrentValue != GameManager.GameState.Merge) return;
        if (!_isAiming) return;
        
        var ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        var hit = Physics2D.Raycast(ray.origin, ray.direction);
        if (!hit) return;
        if (hit.collider.TryGetComponent(out BallBase ball)) 
        { 
            if (!ball.isDestroyed && !ball.IsFrozen)
                ball.EffectAndDestroy(null);
        }
    }
    
    private void StartAim()
    {
        if (_isAiming) return;
        
        _isAiming = true;
        Time.timeScale = GameManager.Instance.TimeScale * 0.1f;
        UIManager.Instance.SetCA(1);
    }
    
    private void EndAim()
    {
        if (!_isAiming) return;
        
        _isAiming = false;
        Time.timeScale = GameManager.Instance.TimeScale;
        UIManager.Instance.SetCA(0.07f);
    }

    private void Start()
    {
        InputProvider.Instance.Gameplay.RightClick.performed += OnPressRightClick;
        InputProvider.Instance.Gameplay.LeftClick.performed += OnPressLeftClick;

        GameManager.Instance.State.Subscribe(_ => EndAim());
    }

    private void Update()
    {
        
    }
    
    private void OnDestroy()
    {
        if (InputProvider.Instance != null)
        {
            InputProvider.Instance.Gameplay.RightClick.performed -= OnPressRightClick;
            InputProvider.Instance.Gameplay.LeftClick.performed -= OnPressLeftClick;
        }
    }
}
