using UnityEngine;

/// <summary>
/// StageEventBaseの生成を担当するファクトリーインターフェース
/// MonoBehaviourコンポーネントの生成を抽象化
/// </summary>
public interface IStageEventFactory
{
    /// <summary>
    /// ランダムなStageEventBaseインスタンスを生成する
    /// </summary>
    /// <param name="eventData">イベントデータ</param>
    /// <returns>生成されたStageEventBaseインスタンス</returns>
    StageEventBase CreateRandomEvent(Object eventData);
}