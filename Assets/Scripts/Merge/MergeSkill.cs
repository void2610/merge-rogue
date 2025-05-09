using R3;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;

public class MergeSkill : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private int skillCoolDownTurn = 3;
    
    public int SkillCoolDownTurn => skillCoolDownTurn;
    public bool IsAiming => _isAiming;
    public ReadOnlyReactiveProperty<int> CurrentCoolDownTurn => _currentCoolDownTurn;
    
    private bool _isAiming = false;
    private ReactiveProperty<int> _currentCoolDownTurn = new(0);
    
    public void SubCoolDown() => _currentCoolDownTurn.Value--;

    private void OnPressRightClick(InputAction.CallbackContext ctx)
    {
        if (GameManager.Instance.State.CurrentValue != GameManager.GameState.Merge) return;
        if (_currentCoolDownTurn.Value > 0)
        {
            EndAim();
            return;
        }
        
        if (!_isAiming) StartAim();
        else EndAim();
    }

    private void OnPressLeftClick(InputAction.CallbackContext ctx)
    {
        if (GameManager.Instance.State.CurrentValue != GameManager.GameState.Merge) return;
        if (!_isAiming) return;
        if (_currentCoolDownTurn.Value > 0)
        {
            EndAim();
            return;
        }
        
        var ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        var hit = Physics2D.Raycast(ray.origin, ray.direction);
        if (!hit) return;
        if (hit.collider.TryGetComponent(out BallBase ball))
        {
            if (!ball.isDestroyed && !ball.IsFrozen)
            {
                ball.EffectAndDestroy(null);
                _currentCoolDownTurn.Value = skillCoolDownTurn;
            }
        }
    }
    
    private void StartAim()
    {
        if (_isAiming) return;
        
        _isAiming = true;
        Time.timeScale = GameManager.Instance.TimeScale * 0.1f;
        UIManager.Instance.SetCA(1);
        audioMixer.SetFloat("LowpassCutoffFreq", 300f);
        audioMixer.SetFloat("LowpassResonance", 3f);
    }
    
    private void EndAim()
    {
        if (!_isAiming) return;
        
        _isAiming = false;
        Time.timeScale = GameManager.Instance.TimeScale;
        UIManager.Instance.SetCA(0.07f);
        audioMixer.SetFloat("LowpassCutoffFreq", 5000f);
        audioMixer.SetFloat("LowpassResonance", 1f);
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
