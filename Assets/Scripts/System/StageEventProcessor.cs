using System;
using System.Collections.Generic;
using System.Threading;
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
        HideOptions();
        _currentEvent = ContentProvider.Instance.GetRandomEvent();
        _currentEvent.Init();
        descriptionText.text = _currentEvent.MainDescription;

        using (var cts = new CancellationTokenSource())
        {
            // 説明文の表示アニメーションとクリック待機タスクを同時に開始
            var textTweenTask = descriptionText.ShowTextTween(0.1f, cts.Token);
            var clickTask = UniTask.WaitUntil(() => Input.anyKeyDown || Input.GetMouseButtonDown(0));
            var result = await UniTask.WhenAny(textTweenTask, clickTask);
            // クリックが先に検知されたらスキップ
            if (!result.hasResultLeft)
            {
                cts.Cancel();
                descriptionText.alpha = 1;
                var animator = new DOTweenTMPAnimator(descriptionText);
                animator.ResetAllChars();
            }

            await Utils.WaitOrSkipInput(500);

            // 各オプションについて処理
            for (var i = 0; i < _currentEvent.Options.Count; i++)
            {
                options[i].SetActive(true);
                options[i].GetComponent<Button>().interactable = false;
                SetOptionBehaviour(options[i].GetComponent<Button>(), _currentEvent.Options[i]);
                var optionText = options[i].GetComponentInChildren<TextMeshProUGUI>();
                optionText.text = _currentEvent.Options[i].description;

                var optionTweenTask = optionText.ShowTextTween(0.1f, cts.Token);
                var clickTaskOption = UniTask.WaitUntil(() => Input.anyKeyDown || Input.GetMouseButtonDown(0));
                var resultOption = await UniTask.WhenAny(optionTweenTask, clickTaskOption);
                if (!resultOption.hasResultLeft)
                {
                    cts.Cancel();
                    optionText.alpha = 1;
                    var animatorOption = new DOTweenTMPAnimator(optionText);
                    animatorOption.ResetAllChars();
                }

                await Utils.WaitOrSkipInput(200);
                options[i].GetComponent<Button>().interactable = _currentEvent.Options[i].IsAvailable();

                EventManager.OnStageEventEnter.Trigger(_currentEvent);
            }
        }
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
        if(!option.isEndless) HideOptions();
        descriptionText.text = option.resultDescription;

        using (var cts = new CancellationTokenSource())
        {
            var textTweenTask = descriptionText.ShowTextTween(0.1f, cts.Token);
            var clickTask = UniTask.WaitUntil(() => Input.anyKeyDown || Input.GetMouseButtonDown(0), cancellationToken: cts.Token);
            var result = await UniTask.WhenAny(textTweenTask, clickTask);
        
            // クリック（またはキー入力）が先に完了した場合
            if (!result.hasResultLeft)
            {
                cts.Cancel();
                // 残りの文字を最終状態にする
                descriptionText.alpha = 1;
                var animator = new DOTweenTMPAnimator(descriptionText);
                animator.ResetAllChars();
            }
        }

        if (option.isEndless)
        {
            UpdateOptions();
            return;
        }
        
        await Utils.WaitOrSkipInput(2500);
        UIManager.Instance.EnableCanvasGroup("Event", false);
        GameManager.Instance.ChangeState(GameManager.GameState.MapSelect);
    }
    
    private void HideOptions() => options.ForEach(option => option.SetActive(false));
    
    private void UpdateOptions()
    {
        HideOptions();
        for (var i = 0; i < _currentEvent.Options.Count; i++)
        {
            options[i].SetActive(true);
            SetOptionBehaviour(options[i].GetComponent<Button>(), _currentEvent.Options[i]);
            options[i].GetComponent<Button>().interactable = _currentEvent.Options[i].IsAvailable();
            options[i].GetComponentInChildren<TextMeshProUGUI>().text = _currentEvent.Options[i].description;
        }
    }
}
