using System;
using System.Collections.Generic;
using System.Numerics;
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
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider seSlider;
    [SerializeField] private Image fadeImage;
    [SerializeField] private RelicDescriptionWindow relicDescriptionWindow;
    [SerializeField] private BallDescriptionWindow ballDescriptionWindow;
    
    [SerializeField] private Volume volume;
    [SerializeField] private List<CanvasGroup> canvasGroups;
    [SerializeField] private TextMeshProUGUI coinText;
    [SerializeField] private TextMeshProUGUI stageText;
    [SerializeField] private TextMeshProUGUI expText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] public Slider hpSlider;
    [SerializeField] public TextMeshProUGUI hpText;

    public int remainingLevelUps;

    public void EnableCanvasGroup(string canvasName, bool e)
    {
        var canvasGroup = canvasGroups.Find(c => c.name == canvasName);
        if (!canvasGroup) return;
        
        canvasGroup.interactable = e;
        canvasGroup.blocksRaycasts = e;
        
        if (e)
        {
            canvasGroup.transform.DOMoveY(-0.45f, 0).SetRelative(true).SetUpdate(true);
            canvasGroup.transform.DOMoveY(0.45f, 0.2f).SetRelative(true).SetUpdate(true).SetEase(Ease.OutBack);
            canvasGroup.DOFade(1, 0.2f).SetUpdate(true);
        }
        else
        {
            canvasGroup.DOFade(0, 0.2f).SetUpdate(true);
        }
    }
    
    private void UpdateCoinText(BigInteger amount)
    {
        coinText.text = "coin: " + amount.ToString();
    }

    private void UpdateExpText(int now, int max)
    {
        expText.text = "exp: " + now + "/" + max;
    }

    private void UpdateLevelText(int level)
    {
        if (!levelText) return;
        levelText.text = "level: " + level;
    }

    private void UpdateStageText(int stage)
    {
        int s = Mathf.Max(1, stage + 1);
        stageText.text = "stage: " + s;
    }
    
    public void OnClickRestButton()
    {
        EventManager.OnRest.Trigger(20);
        var v = EventManager.OnRest.GetAndResetValue();
        GameManager.Instance.Player.Heal(v);
        
        GameManager.Instance.ChangeState(GameManager.GameState.MapSelect);
        EnableCanvasGroup("Rest", false);
    }

    public void OnClickOrganiseButton()
    {
        EventManager.OnOrganise.Trigger(0);
        EnableCanvasGroup("Rest", false);
        InventoryManager.Instance.InventoryUI.StartOrganise();
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
    
    public void ShowRelicDescriptionWindow(RelicData r, Vector3 pos)
    {
        relicDescriptionWindow.ShowWindow(r, pos);
    }
    
    public void ShowBallDescriptionWindow(BallData b, Vector3 pos)
    {
        ballDescriptionWindow.ShowWindow(b, pos);
    }
    
    public void HideRelicDescriptionWindow()
    {
        relicDescriptionWindow.HideWindow();
    }
    
    public void HideBallDescriptionWindow()
    {
        ballDescriptionWindow.HideWindow();
    }

    private void Awake()
    {
        bgmSlider.value = PlayerPrefs.GetFloat("BgmVolume", 1.0f);
        seSlider.value = PlayerPrefs.GetFloat("SeVolume", 1.0f);
        
        canvasGroups.ForEach(c => EnableCanvasGroup(c.name, false));
    }

    private void Start()
    {
        GameManager.Instance.Coin.Subscribe(UpdateCoinText).AddTo(this);
        GameManager.Instance.StageManager.currentStageCount.Subscribe(UpdateStageText).AddTo(this);
        GameManager.Instance.Player.Exp.Subscribe((v) =>
        {
            UpdateExpText(v, GameManager.Instance.Player.MaxExp);
            UpdateLevelText(GameManager.Instance.Player.Level);
        }).AddTo(this);
        GameManager.Instance.Player.Health.Subscribe((v) =>
        {
            hpSlider.value = v;
            hpText.text = v + "/" + GameManager.Instance.Player.MaxHealth;
            if (v < 30)
            {
                SetVignette(((30.0f-v)/30.0f)*0.3f);
            }
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
