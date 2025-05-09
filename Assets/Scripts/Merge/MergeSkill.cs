using UnityEngine;
using UnityEngine.InputSystem;

public class MergeSkill : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    public bool IsAiming => _isAiming;
    private bool _isAiming = false;

    private void OnPressRightClick(InputAction.CallbackContext ctx)
    {
        if (GameManager.Instance.state != GameManager.GameState.Merge) return;
        
        _isAiming = !_isAiming;

        if (_isAiming)
        {
            Time.timeScale = GameManager.Instance.TimeScale * 0.1f;
        }
        else
        {
            Time.timeScale = GameManager.Instance.TimeScale;
        }
    }

    private void OnPressLeftClick(InputAction.CallbackContext ctx)
    {
        if (GameManager.Instance.state != GameManager.GameState.Merge) return;
        if (!_isAiming) return;
        
        var ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        var hit = Physics2D.Raycast(ray.origin, ray.direction);
        if (hit)
        {
            Debug.Log("Hit: " + hit.collider.name);
            if (hit.collider.TryGetComponent(out BallBase ball))
            {
                ball.EffectAndDestroy(null);
            }
        }
    }

    private void Start()
    {
        InputProvider.Instance.Gameplay.RightClick.performed += OnPressRightClick;
        InputProvider.Instance.Gameplay.LeftClick.performed += OnPressLeftClick;
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
