using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening;
using R3;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

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
        
        canvasGroup.alpha = e ? 1 : 0;
        canvasGroup.interactable = e;
        canvasGroup.blocksRaycasts = e;
    }
    
    private void UpdateCoinText(int amount)
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

    public void OnClickShopExit()
    {
        SeManager.Instance.PlaySe("button");
        EnableCanvasGroup("Shop", false);
        GameManager.Instance.ChangeState(GameManager.GameState.MapSelect);
    }

    public void OnClickPause()
    {
        SeManager.Instance.PlaySe("button");
        Time.timeScale = 0;
        EnableCanvasGroup("Pause", true);
    }

    public void OnClickResume()
    {
        SeManager.Instance.PlaySe("button");
        Time.timeScale = 1;
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
    
    public void SetVignette(float value)
    {
        Vignette vignette;
        if(!volume.profile.TryGet(out vignette)) return;
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

        EnableCanvasGroup("Pause", false);
        EnableCanvasGroup("Shop", false);
        EnableCanvasGroup("GameOver", false);
        EnableCanvasGroup("LevelUp", false);
        EnableCanvasGroup("Clear", false);
        EnableCanvasGroup("Map", false);
    }

    private void Start()
    {
        GameManager.Instance.coin.Subscribe(UpdateCoinText).AddTo(this);
        GameManager.Instance.stageManager.currentStageCount.Subscribe(UpdateStageText).AddTo(this);
        GameManager.Instance.player.exp.Subscribe((v) =>
        {
            UpdateExpText(v, GameManager.Instance.player.maxExp);
            UpdateLevelText(GameManager.Instance.player.level);
        }).AddTo(this);
        GameManager.Instance.player.health.Subscribe((v) =>
        {
            hpSlider.value = v;
            hpText.text = v + "/" + GameManager.Instance.player.maxHealth;
            if (v < 30)
            {
                SetVignette(((30.0f-v)/30.0f)*0.3f);
            }
        }).AddTo(this);
        GameManager.Instance.player.maxHealth.Subscribe((v) =>
        {
            hpSlider.maxValue = v;
            hpText.text = GameManager.Instance.player.health.Value + "/" + v;
        }).AddTo(this);
        
        bgmSlider.onValueChanged.AddListener((value) =>
        {
            BgmManager.Instance.BgmVolume = value;
        });
        seSlider.onValueChanged.AddListener((value) =>
        {
            SeManager.Instance.seVolume = value;
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
