using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// レリック管理を担当するサービス実装クラス
/// レリックの追加、削除、効果の適用などのビジネスロジックを提供
/// UI表示とは分離されたpure C#クラス
/// </summary>
public class RelicService : IRelicService, IDisposable
{
    private const int MAX_RELICS = 27;
    private readonly List<RelicData> _relics = new();
    private readonly List<RelicBase> _behaviors = new();
    
    private readonly IRandomService _randomService;
    private readonly IContentService _contentService;
    private readonly IInventoryService _inventoryService;
    
    /// <summary>
    /// 現在所持しているレリックのリスト
    /// </summary>
    public IReadOnlyList<RelicData> Relics => _relics.AsReadOnly();
    
    /// <summary>
    /// 現在アクティブなレリック効果のリスト
    /// </summary>
    public IReadOnlyList<RelicBase> RelicBehaviors => _behaviors.AsReadOnly();
    
    /// <summary>
    /// 最大所持可能レリック数
    /// </summary>
    public int MaxRelics => MAX_RELICS;
    
    /// <summary>
    /// レリックが追加された際のイベント
    /// </summary>
    public event Action<RelicData, RelicBase> OnRelicAdded;
    
    /// <summary>
    /// レリックが削除された際のイベント
    /// </summary>
    public event Action<RelicData> OnRelicRemoved;
    
    /// <summary>
    /// コンストラクタ - 依存性注入
    /// </summary>
    public RelicService(IRandomService randomService, IContentService contentService, IInventoryService inventoryService)
    {
        _randomService = randomService;
        _contentService = contentService;
        _inventoryService = inventoryService;
    }
    
    /// <summary>
    /// レリックを追加する
    /// </summary>
    /// <param name="relic">追加するレリック</param>
    /// <returns>追加に成功したかどうか</returns>
    public bool AddRelic(RelicData relic)
    {
        if (_relics.Count >= MAX_RELICS)
        {
            NotifyWindow.Instance.Notify(NotifyWindow.NotifyType.CantHoldAnyMoreRelics);
            return false;
        }
        
        var behavior = CreateRelicBehavior(relic);
        
        if (behavior != null)
        {
            _relics.Add(relic);
            OnRelicAdded?.Invoke(relic, behavior);
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// レリックを削除する
    /// </summary>
    /// <param name="relic">削除するレリック</param>
    /// <returns>削除に成功したかどうか</returns>
    public bool RemoveRelic(RelicData relic)
    {
        var index = _relics.FindIndex(r => r.className == relic.className);
        if (index == -1)
        {
            Debug.LogError("指定されたレリックが存在しません: " + relic.className);
            return false;
        }

        var behavior = _behaviors[index];
        behavior.Dispose();
        _behaviors.Remove(behavior);
        _relics.Remove(relic);

        OnRelicRemoved?.Invoke(relic);
        return true;
    }
    
    /// <summary>
    /// 指定したタイプのレリックを所持しているかチェック
    /// </summary>
    /// <param name="t">レリックタイプ</param>
    /// <returns>所持している場合true</returns>
    public bool HasRelic(Type t)
    {
        if (!typeof(RelicBase).IsAssignableFrom(t))
            throw new ArgumentException("指定されたクラスはRelicBaseを継承していません: " + t);
        return _behaviors.Exists(b => b.GetType() == t);
    }
    
    /// <summary>
    /// レリック効果を作成する
    /// </summary>
    /// <param name="relic">レリックデータ</param>
    /// <returns>作成されたレリック効果、失敗時はnull</returns>
    private RelicBase CreateRelicBehavior(RelicData relic)
    {
        var type = Type.GetType(relic.className);
        if (type == null)
        {
            Debug.LogError("指定されたクラスが見つかりません: " + relic.className);
            return null;
        }

        try
        {
            var behaviour = Activator.CreateInstance(type) as RelicBase;
            if (behaviour == null)
            {
                Debug.LogError("指定されたクラスはRelicBaseを継承していません: " + relic.className);
                return null;
            }
            
            // 依存性注入
            behaviour.InjectDependencies(_randomService, _contentService, _inventoryService, this);
            
            behaviour.RegisterEffects();
            _behaviors.Add(behaviour);
            
            return behaviour;
        }
        catch (Exception ex)
        {
            Debug.LogError($"レリックのインスタンス化に失敗しました: {relic.className}\n{ex.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// すべてのレリック効果をクリーンアップする
    /// </summary>
    public void Dispose()
    {
        // すべてのレリック効果をクリーンアップ
        foreach (var behavior in _behaviors)
        {
            behavior?.Dispose();
        }
        _behaviors.Clear();
        _relics.Clear();
        
        // イベントのクリーンアップ
        OnRelicAdded = null;
        OnRelicRemoved = null;
    }
}