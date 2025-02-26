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
        _currentEvent = ContentProvider.Instance.GetRandomEvent();
        _currentEvent.Init();
        HideOptions();
        descriptionText.text = _currentEvent.MainDescription;

        // グローバルなキャンセル用 CancellationTokenSource を作成
        using (var globalCts = new CancellationTokenSource())
        {
            try
            {
                // 説明文の表示アニメーションとクリック待機タスクを同時に開始
                var textTweenTask = descriptionText.ShowTextTween(0.1f, globalCts.Token);
                var clickTask = UniTask.WaitUntil(() => Input.anyKeyDown || Input.GetMouseButtonDown(0), cancellationToken: globalCts.Token);
                var result = await UniTask.WhenAny(textTweenTask, clickTask);
                if (!result.hasResultLeft)
                {
                    // クリックが先に検知された場合、グローバルキャンセルを実行
                    globalCts.Cancel();
                    DOTween.Kill(descriptionText);
                    // 説明文を即時最終状態にする
                    descriptionText.alpha = 1;
                    var animator = new DOTweenTMPAnimator(descriptionText);
                    for (int i = 0; i < animator.textInfo.characterCount; i++)
                    {
                        animator.SetCharAlpha(i, 1);
                        animator.SetCharOffset(i, Vector3.zero);
                        animator.SetCharRotation(i, Vector3.zero);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // キャンセルされた場合はここに入りますが、既に最終状態にしているので何もしない
            }

            // キャンセル状態の場合は Delay もスキップ
            try { await UniTask.Delay(TimeSpan.FromSeconds(0.5f), cancellationToken: globalCts.Token); }
            catch (OperationCanceledException) { }

            // 各オプションについて処理
            for (var i = 0; i < _currentEvent.Options.Count; i++)
            {
                options[i].SetActive(true);
                SetOptionBehaviour(options[i].GetComponent<Button>(), _currentEvent.Options[i]);
                var optionText = options[i].GetComponentInChildren<TextMeshProUGUI>();
                optionText.text = _currentEvent.Options[i].description;

                try
                {
                    var optionTweenTask = optionText.ShowTextTween(0.1f, globalCts.Token);
                    var clickTaskOption = UniTask.WaitUntil(() => Input.anyKeyDown || Input.GetMouseButtonDown(0), cancellationToken: globalCts.Token);
                    var resultOption = await UniTask.WhenAny(optionTweenTask, clickTaskOption);
                    if (!resultOption.hasResultLeft)
                    {
                        globalCts.Cancel();
                        DOTween.Kill(optionText);
                        optionText.alpha = 1;
                        var animatorOption = new DOTweenTMPAnimator(optionText);
                        for (int j = 0; j < animatorOption.textInfo.characterCount; j++)
                        {
                            animatorOption.SetCharAlpha(j, 1);
                            animatorOption.SetCharOffset(j, Vector3.zero);
                            animatorOption.SetCharRotation(j, Vector3.zero);
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    // キャンセルされていれば即時完了しているので何もしない
                }

                try { await UniTask.Delay(TimeSpan.FromSeconds(0.2f), cancellationToken: globalCts.Token); }
                catch (OperationCanceledException) { }

                options[i].GetComponent<Button>().interactable = _currentEvent.Options[i].IsAvailable();
            }

            EventManager.OnStageEventEnter.Trigger(_currentEvent);
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
        HideOptions();
        descriptionText.text = option.resultDescription;

        // 説明文のテキスト表示のTween処理にキャンセル処理を追加
        using (var cts = new CancellationTokenSource())
        {
            // 説明文のTweenタスクとクリック待機タスクを同時に開始
            var textTweenTask = descriptionText.ShowTextTween(0.1f, cts.Token);
            var clickTask = UniTask.WaitUntil(() => Input.anyKeyDown || Input.GetMouseButtonDown(0), cancellationToken: cts.Token);
            var result = await UniTask.WhenAny(textTweenTask, clickTask);
        
            // クリック（またはキー入力）が先に完了した場合
            if (!result.hasResultLeft)
            {
                cts.Cancel(); // Tween側にキャンセル要求を伝播
                DOTween.Kill(descriptionText); // 説明文に関連する全Tweenを停止
                // 残りの文字を最終状態にする
                descriptionText.alpha = 1;
                var animator = new DOTweenTMPAnimator(descriptionText);
                for (int i = 0; i < animator.textInfo.characterCount; i++)
                {
                    animator.SetCharAlpha(i, 1);
                    animator.SetCharOffset(i, Vector3.zero);
                    animator.SetCharRotation(i, Vector3.zero);
                }
            }
        }

        // endlessの場合はこれ以上待たずに終了
        if (option.isEndless) return;

        // 次に、2.5秒待機処理にもキャンセル処理を追加
        using (var ctsDelay = new CancellationTokenSource())
        {
            var delayTask = UniTask.Delay(TimeSpan.FromSeconds(2.5f), cancellationToken: ctsDelay.Token);
            var clickTaskDelay = UniTask.WaitUntil(() => Input.anyKeyDown || Input.GetMouseButtonDown(0), cancellationToken: ctsDelay.Token);
            var resultDelay = await UniTask.WhenAny(delayTask, clickTaskDelay);
            if (resultDelay == 1)
            {
                ctsDelay.Cancel(); // Delay処理をキャンセル
            }
        }

        UIManager.Instance.EnableCanvasGroup("Event", false);
        GameManager.Instance.ChangeState(GameManager.GameState.MapSelect);
    }
    
    private void HideOptions() => options.ForEach(option => option.SetActive(false));
}
