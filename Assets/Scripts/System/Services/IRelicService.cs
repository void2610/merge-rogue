using System;
using System.Collections.Generic;

/// <summary>
/// レリック管理を担当するサービスインターフェース
/// レリックの追加、削除、効果の適用などのビジネスロジックを提供
/// </summary>
public interface IRelicService
{
    /// <summary>
    /// レリックが追加された際のイベント
    /// </summary>
    event Action<RelicData, RelicBase> OnRelicAdded;
    
    /// <summary>
    /// レリックが削除された際のイベント
    /// </summary>
    event Action<RelicData> OnRelicRemoved;
    /// <summary>
    /// 現在所持しているレリックのリスト
    /// </summary>
    IReadOnlyList<RelicData> Relics { get; }
    
    /// <summary>
    /// 現在アクティブなレリック効果のリスト
    /// </summary>
    IReadOnlyList<RelicBase> RelicBehaviors { get; }
    
    /// <summary>
    /// 最大所持可能レリック数
    /// </summary>
    int MaxRelics { get; }
    
    /// <summary>
    /// レリックを追加する
    /// </summary>
    /// <param name="relic">追加するレリック</param>
    /// <returns>追加に成功したかどうか</returns>
    bool AddRelic(RelicData relic);
    
    /// <summary>
    /// レリックを削除する
    /// </summary>
    /// <param name="relic">削除するレリック</param>
    /// <returns>削除に成功したかどうか</returns>
    bool RemoveRelic(RelicData relic);
    
    /// <summary>
    /// 指定したタイプのレリックを所持しているかチェック
    /// </summary>
    /// <param name="t">レリックタイプ</param>
    /// <returns>所持している場合true</returns>
    bool HasRelic(Type t);
    
    /// <summary>
    /// すべてのレリック効果をクリーンアップする
    /// </summary>
    void Dispose();
}