using UnityEngine;
using UnityEngine.InputSystem;

public class MergeSkill : MonoBehaviour
{
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

    private void Start()
    {
        InputProvider.Instance.Gameplay.RightClick.performed += OnPressRightClick;
    }

    private void Update()
    {
        
    }
    
    private void OnDestroy()
    {
        if (InputProvider.Instance != null)
            InputProvider.Instance.Gameplay.RightClick.performed -= OnPressRightClick;
    }
}
