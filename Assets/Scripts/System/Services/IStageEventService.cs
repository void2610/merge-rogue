/// <summary>
/// ステージイベントのロジックを管理するサービス
/// </summary>
public interface IStageEventService
{
    /// <summary>
    /// ランダムなステージイベントを取得
    /// </summary>
    StageEventData GetRandomStageEvent();
    
    /// <summary>
    /// イベントオプションが利用可能かチェック
    /// </summary>
    bool IsOptionAvailable(StageEventData.EventOptionData option);
    
    /// <summary>
    /// イベントオプションを実行
    /// </summary>
    void ExecuteOption(StageEventData.EventOptionData option);
}