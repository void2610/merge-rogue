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

/// <summary>
/// ステージイベントのUI表示を管理するView
/// </summary>
public class StageEventView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private List<GameObject> optionButtons;
    
    private IInputProvider _inputProvider;
    private IStageEventService _stageEventService;
    private StageEventData _currentEventData;
    
    /// <summary>
    /// 選択肢が選択されたときのイベント
    /// </summary>
    public event Action<StageEventData.EventOptionData> OnOptionSelected;
    
    [Inject]
    public void InjectDependencies(IInputProvider inputProvider, IStageEventService stageEventService)
    {
        _inputProvider = inputProvider;
        _stageEventService = stageEventService;
    }
    
    /// <summary>
    /// イベントを表示
    /// </summary>
    public async UniTask ShowEvent(StageEventData eventData, CancellationToken cancellationToken = default)
    {
        _currentEventData = eventData;
        HideAllOptions();
        
        // メイン説明文を表示
        await ShowMainDescription(eventData.GetMainDescriptionKey(), cancellationToken);
        
        // 選択肢を表示
        await ShowOptions(eventData.options, cancellationToken);
        
        // 最初の選択可能なオプションを選択
        SelectFirstAvailableOption();
    }
    
    /// <summary>
    /// メイン説明文を表示
    /// </summary>
    private async UniTask ShowMainDescription(string descriptionKey, CancellationToken cancellationToken)
    {
        // ローカライゼーション対応
        descriptionText.text = LocalizeStringLoader.Instance.Get(descriptionKey);
        
        // テキストアニメーション
        await ShowTextWithAnimation(descriptionText, cancellationToken);
        await WaitOrSkipInput(500, cancellationToken);
    }
    
    /// <summary>
    /// 選択肢を表示
    /// </summary>
    private async UniTask ShowOptions(List<StageEventData.EventOptionData> options, CancellationToken cancellationToken)
    {
        for (int i = 0; i < options.Count && i < optionButtons.Count; i++)
        {
            var option = options[i];
            var button = optionButtons[i];
            
            // ボタンを有効化
            button.SetActive(true);
            
            // テキストを設定
            var buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
            var description = LocalizeStringLoader.Instance.Get(option.GetDescriptionKey(_currentEventData.eventId, i));
            var effectDescription = LocalizeStringLoader.Instance.Get(option.GetEffectDescriptionKey(_currentEventData.eventId, i));
            buttonText.text = $"{description}({effectDescription})";
            
            // ボタンのクリックイベントを設定
            var buttonComponent = button.GetComponent<Button>();
            buttonComponent.onClick.RemoveAllListeners();
            buttonComponent.onClick.AddListener(() => OnOptionClicked(option));
            
            // 利用可能かチェック
            buttonComponent.interactable = _stageEventService.IsOptionAvailable(option);
            
            // テキストアニメーション
            await ShowTextWithAnimation(buttonText, cancellationToken);
            await WaitOrSkipInput(200, cancellationToken);
        }
    }
    
    /// <summary>
    /// 結果を表示
    /// </summary>
    public async UniTask ShowResult(StageEventData.EventOptionData option, int optionIndex, CancellationToken cancellationToken = default)
    {
        descriptionText.text = LocalizeStringLoader.Instance.Get(option.GetResultDescriptionKey(_currentEventData.eventId, optionIndex));
        await ShowTextWithAnimation(descriptionText, cancellationToken);
    }
    
    /// <summary>
    /// 全ての選択肢を非表示
    /// </summary>
    public void HideAllOptions()
    {
        foreach (var option in optionButtons)
        {
            option.SetActive(false);
        }
    }
    
    /// <summary>
    /// 選択肢を更新（エンドレスオプション用）
    /// </summary>
    public void UpdateOptions()
    {
        if (!_currentEventData) return;
        
        HideAllOptions();
        
        for (var i = 0; i < _currentEventData.options.Count && i < optionButtons.Count; i++)
        {
            var option = _currentEventData.options[i];
            var button = optionButtons[i];
            
            button.SetActive(true);
            
            var buttonComponent = button.GetComponent<Button>();
            buttonComponent.interactable = _stageEventService.IsOptionAvailable(option);
            
            var buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
            var description = LocalizeStringLoader.Instance.Get(option.GetDescriptionKey(_currentEventData.eventId, i));
            var effectDescription = LocalizeStringLoader.Instance.Get(option.GetEffectDescriptionKey(_currentEventData.eventId, i));
            buttonText.text = $"{description}({effectDescription})";
        }
    }
    
    /// <summary>
    /// オプションがクリックされたとき
    /// </summary>
    private void OnOptionClicked(StageEventData.EventOptionData option)
    {
        OnOptionSelected?.Invoke(option);
    }
    
    /// <summary>
    /// テキストアニメーション付きで表示
    /// </summary>
    private async UniTask ShowTextWithAnimation(TextMeshProUGUI text, CancellationToken cancellationToken)
    {
        using (var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
        {
            var textTweenTask = text.ShowTextTween(0.1f, cts.Token);
            var skipTask = UniTask.WaitUntil(() => _inputProvider.IsSkipButtonPressed(), cancellationToken: cts.Token);
            
            var result = await UniTask.WhenAny(textTweenTask, skipTask);
            
            // スキップされた場合
            if (!result.hasResultLeft)
            {
                cts.Cancel();
                text.alpha = 1;
                var animator = new DOTweenTMPAnimator(text);
                animator.ResetAllChars();
            }
        }
    }
    
    /// <summary>
    /// 指定時間待機するか、入力でスキップ
    /// </summary>
    private async UniTask WaitOrSkipInput(int delayTime, CancellationToken cancellationToken = default)
    {
        using (var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
        {
            var delayTask = UniTask.Delay(delayTime, cancellationToken: cts.Token);
            var skipTask = UniTask.WaitUntil(() => _inputProvider.IsSkipButtonPressed(), cancellationToken: cts.Token);
            
            var result = await UniTask.WhenAny(delayTask, skipTask);
            
            // スキップされた場合
            if (result == 1) cts.Cancel();
        }
    }
    
    /// <summary>
    /// 最初の選択可能なオプションを選択
    /// </summary>
    private void SelectFirstAvailableOption()
    {
        foreach (var button in optionButtons)
        {
            if (button.activeSelf && button.GetComponent<Button>().interactable)
            {
                SelectionCursor.SetSelectedGameObjectSafe(button);
                break;
            }
        }
    }
}