using System;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>
/// StageEventBaseの生成を担当するファクトリー実装クラス
/// ContentProviderのGameObjectを使ってMonoBehaviourコンポーネントを生成
/// </summary>
public class StageEventFactory : IStageEventFactory
{
    private readonly GameObject _gameObject;
    
    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="gameObject">コンポーネント追加対象のGameObject</param>
    public StageEventFactory(GameObject gameObject)
    {
        _gameObject = gameObject;
    }
    
    /// <summary>
    /// ランダムなStageEventBaseインスタンスを生成する
    /// </summary>
    /// <param name="eventData">イベントデータ</param>
    /// <returns>生成されたStageEventBaseインスタンス</returns>
    public StageEventBase CreateRandomEvent(Object eventData)
    {
        if (eventData == null)
        {
            throw new ArgumentNullException(nameof(eventData));
        }
        
        var type = Type.GetType(eventData.name);
        if (type != null && type.IsSubclassOf(typeof(StageEventBase)))
        {
            var eventInstance = _gameObject.AddComponent(type) as StageEventBase;
            if (eventInstance != null)
            {
                eventInstance.Init();
                return eventInstance;
            }
        }
        
        throw new Exception($"Event not found or invalid type: {eventData.name}");
    }
}