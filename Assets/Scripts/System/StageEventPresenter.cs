using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using R3;

/// <summary>
/// ステージイベントのプレゼンター
/// ViewとServiceを繋いでイベントの流れを制御
/// </summary>
public class StageEventPresenter
{
    private readonly StageEventView _view;
    private readonly IStageEventService _stageEventService;
    private StageEventData _currentEventData;
    
    public StageEventPresenter(IStageEventService stageEventService)
    {
        _stageEventService = stageEventService;
        _view = Object.FindAnyObjectByType<StageEventView>();
        _view.OnOptionSelected += OnOptionSelected;
    }
    
    /// <summary>
    /// ステージイベントを開始
    /// </summary>
    public void StartEvent()
    {
        ProcessEventAsync().Forget();
    }
    
    /// <summary>
    /// イベント処理の非同期フロー
    /// </summary>
    private async UniTaskVoid ProcessEventAsync()
    {
        // ランダムなイベントを取得
        _currentEventData = _stageEventService.GetRandomStageEvent();
        
        // イベントを表示
        await _view.ShowEvent(_currentEventData);
        
        // イベント開始を通知
        EventManager.OnStageEventEnter.OnNext(Unit.Default);
    }
    
    /// <summary>
    /// オプションが選択されたとき
    /// </summary>
    private void OnOptionSelected(StageEventData.EventOptionData option)
    {
        ProcessOptionAsync(option).Forget();
    }
    
    /// <summary>
    /// オプション実行の非同期フロー
    /// </summary>
    private async UniTaskVoid ProcessOptionAsync(StageEventData.EventOptionData option)
    {
        // アクションを実行
        _stageEventService.ExecuteOption(option);
        
        // オプションのインデックスを取得
        var optionIndex = _currentEventData.options.IndexOf(option);
        
        // 結果を表示
        await _view.ShowResult(option, optionIndex);
        
        if (option.isEndless)
        {
            // エンドレスオプションの場合は選択肢を更新
            _view.UpdateOptions();
        }
        else
        {
            // 通常のオプションの場合は選択肢を非表示
            _view.HideAllOptions();
            
            // UIをリセット
            UIManager.Instance.ResetSelectedGameObject();
            
            // 少し待機してからイベント終了
            await UniTask.Delay(2500);
            
            EndEvent();
        }
    }
    
    /// <summary>
    /// イベントを終了
    /// </summary>
    private void EndEvent()
    {
        // マップ選択画面へ遷移
        GameManager.Instance.ChangeState(GameManager.GameState.MapSelect);
        
        // イベントUIを非表示
        UIManager.Instance.EnableCanvasGroup("Event", false);
    }
}