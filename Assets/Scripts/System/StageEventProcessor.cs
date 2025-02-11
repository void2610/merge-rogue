using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StageEventProcessor : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private List<GameObject> options;
    
    private StageEventBase _currentEvent;
    
    public void StartEvent() => SetRandomEventAsync().Forget();
    
    private async UniTaskVoid SetRandomEventAsync()
    {
        _currentEvent = ContentProvider.Instance.GetRandomEvent();
        _currentEvent.Init();
        HideOptions();
        descriptionText.text = _currentEvent.MainDescription;
        await descriptionText.ShowTextTween();
        await UniTask.Delay(TimeSpan.FromSeconds(0.5f));
        
        for (var i = 0; i < _currentEvent.Options.Count; i++)
        {
            options[i].SetActive(true);
            SetOptionBehaviour(options[i].GetComponent<Button>(), _currentEvent.Options[i]);

            options[i].GetComponentInChildren<TextMeshProUGUI>().text = _currentEvent.Options[i].description;
            await options[i].GetComponentInChildren<TextMeshProUGUI>().ShowTextTween();
            
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
        descriptionText.text = option.resultDescription;
        await descriptionText.ShowTextTween();
        if (option.isEndless) return;
        
        await UniTask.Delay(TimeSpan.FromSeconds(2.5f));
        UIManager.Instance.EnableCanvasGroup("Event", false);
        GameManager.Instance.ChangeState(GameManager.GameState.MapSelect);
    }
    
    private void HideOptions() => options.ForEach(option => option.SetActive(false));
}
