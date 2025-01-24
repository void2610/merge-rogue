using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EventProcessor : MonoBehaviour
{
    public static EventProcessor Instance;
    
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private List<GameObject> options;
    
    private StageEventBase _currentEvent;
    
    public void StartEvent() => SetRandomEventAsync().Forget();
    
    private async UniTaskVoid SetRandomEventAsync()
    {
        _currentEvent = ContentProvider.Instance.GetRandomEvent();
        _currentEvent.Init();
        HideOptions();
        await ShowTextAsync(descriptionText, _currentEvent.MainDescription, 0.05f);
        await UniTask.Delay(TimeSpan.FromSeconds(0.5f));
        
        for (var i = 0; i < _currentEvent.Options.Count; i++)
        {
            options[i].SetActive(true);
            SetOptionBehaviour(options[i].GetComponent<Button>(), _currentEvent.Options[i]);
            await ShowTextAsync(options[i].GetComponentInChildren<TextMeshProUGUI>(), _currentEvent.Options[i].description, 0.05f);
            await UniTask.Delay(TimeSpan.FromSeconds(0.2f));
            options[i].GetComponent<Button>().interactable = _currentEvent.Options[i].IsAvailable();
        }
        EventManager.OnStageEventEnter.Trigger(_currentEvent);
    }

    private void SetOptionBehaviour(Button button, StageEventBase.OptionData option)
    {
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
        {
            ProcessActionAsync(option).Forget();
        });
    }
    
    private async UniTaskVoid ProcessActionAsync(StageEventBase.OptionData option)
    {
        option.Action();
        HideOptions();
        await ShowTextAsync(descriptionText, option.resultDescription, 0.05f);
        if (option.isEndless) return;
        
        await UniTask.Delay(TimeSpan.FromSeconds(2.5f));
        UIManager.Instance.EnableCanvasGroup("Event", false);
        GameManager.Instance.ChangeState(GameManager.GameState.MapSelect);
    }
    
    private async UniTask ShowTextAsync(TextMeshProUGUI text, string description, float duration)
    {
        text.text = description;
        text.alpha = 0;
        var animator = new DOTweenTMPAnimator(text);

        for (var i = 0; i < animator.textInfo.characterCount; i++)
        {
            // 改行文字かどうかを確認
            if (description[i] == '\n')
                await UniTask.Delay(TimeSpan.FromSeconds(duration * 5));
            await animator.DOFadeChar(i, 1, duration);
        }
    }

    
    private void HideOptions() => options.ForEach(option => option.SetActive(false));
    
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
}
