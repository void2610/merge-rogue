using System;
using System.Collections.Generic;
using System.Numerics;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening;
using R3;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Vector3 = UnityEngine.Vector3;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider seSlider;
    [SerializeField] private Image fadeImage;
    [SerializeField] private DescriptionWindow descriptionWindow;
    
    [SerializeField] private Volume volume;
    [SerializeField] private List<CanvasGroup> canvasGroups;
    [SerializeField] private TextMeshProUGUI coinText;
    [SerializeField] private TextMeshProUGUI stageText;
    [SerializeField] private TextMeshProUGUI expText;
    [SerializeField] public Slider hpSlider;
    [SerializeField] public TextMeshProUGUI hpText;

    public int remainingLevelUps;
    
    private readonly Dictionary<string, Sequence> _canvasGroupTween = new();

    public void EnableCanvasGroup(string canvasName, bool e) => EnableCanvasGroupAsync(canvasName, e).Forget();
    private async UniTaskVoid EnableCanvasGroupAsync(string canvasName, bool e)
    {
        var cg = canvasGroups.Find(c => c.name == canvasName);
        if (!cg) return;
        if (_canvasGroupTween[canvasName].IsActive()) return;

        // アニメーション中は操作をブロック
        cg.interactable = false;
        cg.blocksRaycasts = false;
        
        var seq = DOTween.Sequence();
        seq.SetUpdate(true);
        if (e)
        {
            seq.Join(cg.transform.DOMoveY(-0.45f, 0).SetRelative(true));
            seq.Join(cg.transform.DOMoveY(0.45f, 0.2f).SetRelative(true).SetEase(Ease.OutBack));
            seq.Join(cg.DOFade(1, 0.2f));
        }
        else
        {
            seq.Join(cg.DOFade(0, 0.2f));
        }
        
        _canvasGroupTween[canvasName] = seq;
        await seq.AsyncWaitForCompletion();
        _canvasGroupTween[canvasName] = null;
        cg.interactable = e;
        cg.blocksRaycasts = e;
    }
    
    private void UpdateCoinText(BigInteger amount) => coinText.text = "coin: " + amount;
    private void UpdateExpText(int now, int max) => expText.text = "exp: " + now + "/" + max;
    private void UpdateStageText(int stage) => stageText.text = "stage: " + Mathf.Max(1, stage + 1);
    public void ShowRelicDescriptionWindow(RelicData r, GameObject g) => descriptionWindow.ShowWindow(r, g);
    public void ShowBallDescriptionWindow(BallData b, GameObject g) => descriptionWindow.ShowWindow(b, g);
    
    public void OnClickRestButton()
    {
        var restAmount = GameManager.Instance.Player.MaxHealth.Value  * 0.2f;
        EventManager.OnRest.Trigger((int)restAmount);
        GameManager.Instance.Player.Heal(EventManager.OnRest.GetAndResetValue());
        
        GameManager.Instance.ChangeState(GameManager.GameState.MapSelect);
        EnableCanvasGroup("Rest", false);
    }

    public void OnClickOrganiseButton()
    {
        SeManager.Instance.PlaySe("button");
        EventManager.OnOrganise.Trigger(0);
        EnableCanvasGroup("Rest", false);
        InventoryManager.Instance.InventoryUI.StartEdit(InventoryUI.InventoryUIState.Swap);
    }
    
    public void OnClickSkippRestButton()
    {
        SeManager.Instance.PlaySe("button");
        GameManager.Instance.ChangeState(GameManager.GameState.MapSelect);
        EnableCanvasGroup("Rest", false);
    }

    public void OnClickShopExit()
    {
        SeManager.Instance.PlaySe("button");
        EnableCanvasGroup("Shop", false);
        GameManager.Instance.ChangeState(GameManager.GameState.MapSelect);
        Shop.Instance.CloseShop();
    }
    
    public void OnClickTreasureExit()
    {
        SeManager.Instance.PlaySe("button");
        EnableCanvasGroup("Treasure", false);
        GameManager.Instance.ChangeState(GameManager.GameState.MapSelect);
        Treasure.Instance.CloseTreasure();
    }

    public void OnClickPause()
    {
        SeManager.Instance.PlaySe("button");
        Time.timeScale = 0;
        EnableCanvasGroup("Pause", true);
    }

    public void OnClickSpeed()
    {
        SeManager.Instance.PlaySe("button");
        GameManager.Instance.ChangeTimeScale();
    }

    public void OnClickResume()
    {
        SeManager.Instance.PlaySe("button");
        Time.timeScale = GameManager.Instance.TimeScale;
        EnableCanvasGroup("Pause", false);
    }

    public void OnClickTitle()
    {
        SeManager.Instance.PlaySe("button");
        fadeImage.color = new Color(0, 0, 0, 0);
        fadeImage.DOFade(1f, 1f).OnComplete(() => SceneManager.LoadScene("TitleScene")).SetUpdate(true);
    }

    public void OnClickRetry()
    {
        SeManager.Instance.PlaySe("button");
        fadeImage.color = new Color(0, 0, 0, 0);
        fadeImage.DOFade(1f, 1f).OnComplete(() => SceneManager.LoadScene("MainScene")).SetUpdate(true);
    }

    public void OpenMap()
    {
        SeManager.Instance.PlaySe("button");
        EnableCanvasGroup("Map", true);
    }
    
    public void CloseMap()
    {
        SeManager.Instance.PlaySe("button");
        EnableCanvasGroup("Map", false);
    }
    
    private void SetVignette(float value)
    {
        if(!volume.profile.TryGet(out Vignette vignette)) return;
        vignette.intensity.value = value;
    }

    private void Awake()
    {
        if(Instance == null) Instance = this;
        else Destroy(gameObject);
        
        bgmSlider.value = PlayerPrefs.GetFloat("BgmVolume", 1.0f);
        seSlider.value = PlayerPrefs.GetFloat("SeVolume", 1.0f);

        foreach (var canvasGroup in canvasGroups)
        {
            _canvasGroupTween.Add(canvasGroup.name, null);
            EnableCanvasGroup(canvasGroup.name, false);
        }
    }

    private void Start()
    {
        GameManager.Instance.Coin.Subscribe(UpdateCoinText).AddTo(this);
        GameManager.Instance.StageManager.CurrentStageCount.Subscribe(UpdateStageText).AddTo(this);
        GameManager.Instance.Player.Exp.Subscribe((v) => UpdateExpText(v, GameManager.Instance.Player.MaxExp)).AddTo(this);
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
        fadeImage.DOFade(0, 2f).SetUpdate(true);
    }
}
