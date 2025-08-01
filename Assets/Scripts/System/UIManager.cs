using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using TMPro;
using DG.Tweening;
using JetBrains.Annotations;
using R3;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    
    [SerializeField] private Canvas uiCanvas;
    [SerializeField] private Camera uiCamera;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider seSlider;
    [SerializeField] private Image fadeImage;
    [SerializeField] private DescriptionWindow descriptionWindow;
    [SerializeField] private InputGuide inputGuide;
    [SerializeField] private Volume volume;
    [SerializeField] private List<CanvasGroup> canvasGroups;
    [SerializeField] private TextMeshProUGUI coinText;
    // [SerializeField] private TextMeshProUGUI expText;
    [SerializeField] private Slider hpSlider;
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private Shop shop;
    [SerializeField] private Treasure treasure;
    [SerializeField] private GameObject mergeArea;
    [SerializeField] private GameObject virtualMouse;
    [SerializeField] private Transform ballUIContainer;
    [SerializeField] private Transform relicContainer;
    [SerializeField] private Transform playerStatusUI;
    [SerializeField] private Transform enemyStatusUIContainer;
    [SerializeField] private GameObject inventoryCanvasBlocker;
    
    private static CursorPositionType _cursorPosition = CursorPositionType.Merge;

    public bool IsPaused { get; private set; }
    public bool IsMapOpened { get; private set; }
    public bool IsTutorialOpened { get; set; }
    
    private CanvasGroupSwitcher _canvasGroupSwitcher;
    
    public Transform PlayerStatusUI => playerStatusUI;
    public Transform EnemyStatusUIContainer => enemyStatusUIContainer;
    public Canvas GetUICanvas() => uiCanvas;
    public Camera GetUICamera() => uiCamera;
    public Transform GetEnemyUIContainer() => uiCanvas.transform.Find("EnemyStatusUIContainer");
    public bool IsVirtualMouseActive() => virtualMouse.GetComponent<MyVirtualMouseInput>().isActive;
    public void ShowRelicDescriptionWindow(RelicData r, GameObject g) => descriptionWindow.ShowWindowWithHoverCheck(r, g).Forget();

    public void ShowBallDescriptionWindow(BallData b, GameObject g, int level) =>
        descriptionWindow.ShowWindowWithHoverCheck(b, g, level).Forget();

    public void ToggleCursorState()
    {
        if (inventoryCanvasBlocker.activeSelf) return;
        
        _cursorPosition = _cursorPosition.Toggle();
        
        // 状態異常がない時はスキップする
        if (_cursorPosition == CursorPositionType.StatusEffect && !GetFirstSelectableStatusEffectUI())
            _cursorPosition = _cursorPosition.Toggle();
        
        ResetSelectedGameObject();
    }

    [CanBeNull]
    private GameObject GetFirstSelectableStatusEffectUI()
    {
        var statusEffectUIs = new List<StatusEffectUI>(){playerStatusUI.GetComponent<StatusEffectUI>()};
        statusEffectUIs.AddRange(enemyStatusUIContainer.GetComponentsInChildren<StatusEffectUI>().ToList());
        foreach (var seUI in statusEffectUIs)
        {
            var selectables = seUI.GetStatusEffectIcons();
            if (selectables.Count > 0) return selectables[0].gameObject;
        }
        return null;
    }

    private void SetCursorState(CursorPositionType position)
    {
        _cursorPosition = position;
        ResetSelectedGameObject(true);
    }
    
    public void LockCursorToInventory(bool b)
    {
        SetCursorState(b ? CursorPositionType.Ball : CursorPositionType.Merge);

        SelectionCursor.LockCursorToInventory(b);
        MouseHoverUISelector.LockCursorToInventory(b);
        inventoryCanvasBlocker.SetActive(b);
    }
    
    public void EnableCanvasGroup(string canvasName, bool e) => EnableCanvasGroupAsync(canvasName, e).Forget();

    public bool IsEnableCanvasGroup(string canvasName)
    {
        if (canvasGroups == null || canvasGroups.Count == 0) return false;
        return canvasGroups.Find(c => c.name == canvasName).alpha > 0;
    }

    public bool IsAnyCanvasGroupEnabled()
    {
        if (canvasGroups == null || canvasGroups.Count == 0) return false;
        return canvasGroups.Exists(c => c.alpha > 0);
    }

    public GameObject GetTopCanvasGroup() => _canvasGroupSwitcher?.GetTopCanvasGroup();
    
    private void UpdateCoinText(System.Numerics.BigInteger amount) => coinText.text = "coin: " + amount;

    public void ResetSelectedGameObject(bool isToggle = false)
    {
        var topCanvasGroup = GetTopCanvasGroup();
        // もしウィンドウが開いている場合は、そちらを優先して選択
        if (topCanvasGroup && !isToggle && _cursorPosition != CursorPositionType.Merge)
        {
            if (topCanvasGroup.name == "Pause" || topCanvasGroup.name == "Tutorial")
            {
                var focusSelectable = topCanvasGroup.GetComponentInChildren<FocusSelectable>();
                if (!focusSelectable) return;
                SelectionCursor.SetSelectedGameObjectSafe(focusSelectable.gameObject);
            }
            else
            {
                var target = _cursorPosition switch
                {
                    CursorPositionType.Merge => mergeArea,
                    CursorPositionType.Ball => ballUIContainer.childCount > 0 ? ballUIContainer.GetChild(0).gameObject : null,
                    CursorPositionType.Relic => relicContainer.childCount > 0 ? relicContainer.GetChild(0).gameObject : null,
                    CursorPositionType.StatusEffect => GetFirstSelectableStatusEffectUI(),
                    _ => null,
                };
                SelectionCursor.SetSelectedGameObjectSafe(target);
            }
        }
        else if (topCanvasGroup && !isToggle && _cursorPosition == CursorPositionType.Merge)
        {
            var focusSelectable = topCanvasGroup.GetComponentInChildren<FocusSelectable>();
            if (!focusSelectable) return;
            SelectionCursor.SetSelectedGameObjectSafe(focusSelectable.gameObject);
        }
        else
        {
            // レリックがない場合は状態異常またはマージにスキップ
            if (_cursorPosition == CursorPositionType.Relic && relicContainer.childCount == 0)
            {
                var nextState = GetFirstSelectableStatusEffectUI() 
                    ? CursorPositionType.StatusEffect 
                    : CursorPositionType.Merge;
                SetCursorState(nextState);
                ResetSelectedGameObject();
                return;
            }
            
            if (_cursorPosition == CursorPositionType.StatusEffect && !GetFirstSelectableStatusEffectUI())
            {
                SetCursorState(CursorPositionType.Merge);
                ResetSelectedGameObject();
                return;
            }
            
            // ウィンドウがない時はcursorStateに従って選択をリセット
            var target = _cursorPosition switch
            {
                CursorPositionType.Merge => mergeArea,
                CursorPositionType.Ball => ballUIContainer.childCount > 0 ? ballUIContainer.GetChild(0).gameObject : null,
                CursorPositionType.Relic => relicContainer.childCount > 0 ? relicContainer.GetChild(0).gameObject : null,
                CursorPositionType.StatusEffect => GetFirstSelectableStatusEffectUI(),
                _ => null,
            };

            SelectionCursor.SetSelectedGameObjectSafe(target);
        }
    }
    
    /// <summary>
    /// 仮想マウスを任意の位置へ移動させるメソッド
    /// </summary>
    public void MoveVirtualMouseToCenter()
    {
        var vm = virtualMouse.GetComponent<MyVirtualMouseInput>();
        var centerPos = new Vector2(Screen.width / 2f, Screen.height / 2f);
        InputState.Change(vm.virtualMouse.position, centerPos);
        // ソフトウェアカーソル（UI上の表示）の位置も更新（存在する場合）
        // もしCanvasのスケール等を考慮する必要があるなら、ここで座標変換を行う
        virtualMouse.transform.position = Vector2.zero;
    }

    public void ToggleVirtualMouse()
    {
        if (IsVirtualMouseActive())
        {
            virtualMouse.GetComponent<MyVirtualMouseInput>().isActive = false;
            EventSystem.current.sendNavigationEvents = true;
        }
        else
        {
            virtualMouse.GetComponent<MyVirtualMouseInput>().isActive = true;
            EventSystem.current.sendNavigationEvents = false;
            MoveVirtualMouseToCenter();
        }
    }
    
    public void SetVirtualMousePosition(Vector2 pos)
    {
        var rectTransform = virtualMouse.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = pos;
    }
    
    private async UniTaskVoid EnableCanvasGroupAsync(string canvasName, bool e)
    {
        await _canvasGroupSwitcher.EnableCanvasGroupAsync(canvasName, e);
        
        inputGuide.UpdateText(IsAnyCanvasGroupEnabled() ? InputGuide.InputGuideType.Navigate : InputGuide.InputGuideType.Merge);
        
        // FocusSelectableがアタッチされているオブジェクトがあればフォーカス
        ResetSelectedGameObject();
        descriptionWindow.HideWindowFromNavigation();
    }

    // private void UpdateExpText(int now, int max)
    // {
    //     if (GameManager.Instance.Player.Level >= Player.MAX_LEVEL)
    //     {
    //         expText.text = "max level";
    //     }
    //     expText.text = "exp: " + now + "/" + max;
    // }

    public void OnClickPauseButton()
    {
        IsPaused = !IsPaused;
        Time.timeScale = IsPaused ? 0 : GameManager.Instance.TimeScale;
        EnableCanvasGroup("Pause", IsPaused);
    }

    public void OnClickSpeedButton()
    {
        GameManager.Instance.ChangeTimeScale();
    }
    
    public void OnClickMapButton()
    {
        IsMapOpened = !IsMapOpened;
        EnableCanvasGroup("Map", IsMapOpened);
    }
    
    public void OnClickMapButtonForce(bool e)
    {
        if (IsMapOpened == e) return;
        IsMapOpened = e;
        EnableCanvasGroup("Map", e);
    }
    
    public void OnClickTutorialButton()
    {
        IsTutorialOpened = !IsTutorialOpened;
        EnableCanvasGroup("Tutorial", IsTutorialOpened);
    }

    public void OnClickTitle()
    {
        fadeImage.color = new Color(0, 0, 0, 0);
        fadeImage.DOFade(1f, 1f).OnComplete(() => SceneManager.LoadScene("TitleScene")).SetUpdate(true);
    }

    public void OnClickRetry()
    {
        fadeImage.color = new Color(0, 0, 0, 0);
        fadeImage.DOFade(1f, 1f).OnComplete(() => SceneManager.LoadScene("MainScene")).SetUpdate(true);
    }
    
    public void OpenSteam()
    {
        Application.OpenURL("https://store.steampowered.com/app/3646540/Merge_Rogue/?beta=1");
    }
    
    private void UpdateStatusEffectUINavigation()
    {
        var seUIs = new List<StatusEffectUI>(){playerStatusUI.GetComponent<StatusEffectUI>()};
        seUIs.AddRange(enemyStatusUIContainer.GetComponentsInChildren<StatusEffectUI>().ToList());
        var selectables = new List<Selectable>();
        foreach (var seUI in seUIs)
        {
           selectables.AddRange(seUI.GetStatusEffectIcons());
        }
        if (selectables.Count == 0) return;
        selectables.SetHorizontalNavigation();
    }
    
    private void Fade(bool e)
    {
        if (e) fadeImage.DOFade(1, 2f).SetUpdate(true);
        else fadeImage.DOFade(0, 2f).SetUpdate(true);
    }
    
    private void SetVignette(float value)
    {
        if(!volume.profile.TryGet(out Vignette vignette)) return;
        vignette.intensity.value = value;
    }

    private void Awake()
    {
        if(!Instance) Instance = this;
        else Destroy(gameObject);
        
        _canvasGroupSwitcher = new CanvasGroupSwitcher(canvasGroups, "Treasure");
        
        _cursorPosition = CursorPositionType.Merge;
        
        inventoryCanvasBlocker.SetActive(false);
    }

    private void Start()
    {
        GameManager.Instance.Coin.Subscribe(UpdateCoinText).AddTo(this);
        // GameManager.Instance.Player.Exp.Subscribe((v) => UpdateExpText(v, GameManager.Instance.Player.MaxExp)).AddTo(this);
        GameManager.Instance.Player.Health.Subscribe((v) =>
        {
            hpSlider.value = v;
            hpText.text = v + "/" + GameManager.Instance.Player.MaxHealth;
            if (v < 30) SetVignette(((30.0f-v)/30.0f)*0.3f);
            else SetVignette(0);
        }).AddTo(this);
        GameManager.Instance.Player.MaxHealth.Subscribe((v) =>
        {
            hpSlider.maxValue = v;
            hpText.text = GameManager.Instance.Player.Health.Value + "/" + v;
        }).AddTo(this);
        
        bgmSlider.onValueChanged.AddListener((value) =>
        {
            BgmManager.Instance.BgmVolume = value;
        });
        seSlider.onValueChanged.AddListener((value) =>
        {
            SeManager.Instance.SeVolume = value;
        });

        var trigger = seSlider.gameObject.AddComponent<EventTrigger>();
        var entry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerUp
        };
        entry.callback.AddListener(_ => SeManager.Instance.PlaySe("button"));
        trigger.triggers.Add(entry);

        fadeImage.color = new Color(0, 0, 0, 1);
        Fade(false);
        
        ToggleVirtualMouse();
    }

    private void Update()
    {
        UpdateStatusEffectUINavigation();
    }
}
