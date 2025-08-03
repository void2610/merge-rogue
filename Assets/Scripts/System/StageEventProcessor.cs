using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using R3;
using VContainer;
using VContainer.Unity;

public class StageEventProcessor : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private List<GameObject> options;
    
    private StageEventBase _currentEvent;
    private IInputProvider _inputProvider;
    private IContentService _contentService;
    private IObjectResolver _resolver;
    
    [Inject]
    public void InjectDependencies(IInputProvider inputProvider, IContentService contentService, IObjectResolver resolver)
    {
        _inputProvider = inputProvider;
        _contentService = contentService;
        _resolver = resolver;
    }
    
    /// <summary>
    /// 指定時間 await するが、途中でクリックかキー操作がされた場合は即座に終了する
    /// </summary>
    private async UniTask WaitOrSkipInput(int delayTime, CancellationToken cancellationToken = default)
    {
        using (var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
        {
            var delayTask = UniTask.Delay(delayTime, cancellationToken: cts.Token);
            var conditionTask = UniTask.WaitUntil(() => _inputProvider.IsSkipButtonPressed(), cancellationToken: cts.Token);
            
            var result = await UniTask.WhenAny(delayTask, conditionTask);

            // 待機をスキップするためキャンセル
            if (result == 1) cts.Cancel();
        }
    }
    
    public void StartEvent() => SetRandomEventAsync().Forget();
    
    private async UniTaskVoid SetRandomEventAsync()
    {
        HideOptions();
        var type = _contentService.GetRandomEventType();
        _currentEvent = gameObject.AddComponent(type) as StageEventBase;
        if (!_currentEvent) throw new Exception("Failed to create StageEventBase instance");
        
        // VContainerで依存性を注入
        _resolver.Inject(_currentEvent);
        
        _currentEvent.Init();
        descriptionText.text = _currentEvent.MainDescription;

        using (var cts = new CancellationTokenSource())
        {
            // 説明文の表示アニメーションとクリック待機タスクを同時に開始
            var textTweenTask = descriptionText.ShowTextTween(0.1f, cts.Token);
            var clickTask = UniTask.WaitUntil(() => _inputProvider.IsSkipButtonPressed());
            var result = await UniTask.WhenAny(textTweenTask, clickTask);
            // クリックが先に検知されたらスキップ
            if (!result.hasResultLeft)
            {
                cts.Cancel();
                descriptionText.alpha = 1;
                var animator = new DOTweenTMPAnimator(descriptionText);
                animator.ResetAllChars();
            }

            await WaitOrSkipInput(500);

            // 各オプションについて処理
            for (var i = 0; i < _currentEvent.Options.Count; i++)
            {
                options[i].SetActive(true);
                options[i].GetComponent<Button>().interactable = false;
                SetOptionBehaviour(options[i].GetComponent<Button>(), _currentEvent.Options[i]);
                var optionText = options[i].GetComponentInChildren<TextMeshProUGUI>();
                optionText.text = _currentEvent.Options[i].description;

                var optionTweenTask = optionText.ShowTextTween(0.1f, cts.Token);
                var clickTaskOption = UniTask.WaitUntil(() => _inputProvider.IsSkipButtonPressed());
                var resultOption = await UniTask.WhenAny(optionTweenTask, clickTaskOption);
                if (!resultOption.hasResultLeft)
                {
                    cts.Cancel();
                    var animatorOption = new DOTweenTMPAnimator(optionText);
                    animatorOption.ResetAllChars();
                }
                optionText.alpha = 1;

                await WaitOrSkipInput(200);
                options[i].GetComponent<Button>().interactable = _currentEvent.Options[i].IsAvailable();

                EventManager.OnStageEventEnter.OnNext(R3.Unit.Default);
            }
            SelectionCursor.SetSelectedGameObjectSafe(options[0]);
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
        
        if (!option.isEndless) HideOptions();
        else UpdateOptions();

        descriptionText.text = option.resultDescription;

        using (var cts = new CancellationTokenSource())
        {
            var textTweenTask = descriptionText.ShowTextTween(0.1f, cts.Token);
            var clickTask = UniTask.WaitUntil(() => _inputProvider.IsSkipButtonPressed(), cancellationToken: cts.Token);
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

        UIManager.Instance.ResetSelectedGameObject();

        if (option.isEndless) return;
        
        await WaitOrSkipInput(2500);
        
        GameManager.Instance.ChangeState(GameManager.GameState.MapSelect);
        UIManager.Instance.EnableCanvasGroup("Event", false);
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
