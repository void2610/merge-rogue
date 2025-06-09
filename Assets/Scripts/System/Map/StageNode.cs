using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StageNode
{
    public StageType Type { get; private set; }  // ステージの種類
    public Vector2 Position;                      // マップ上の位置
    public readonly List<StageNode> Connections;  // 次のステージへの接続
    public GameObject Obj;                        // マップ上のオブジェクト
    private readonly StageData _stageData;        // ステージデータ

    /// <summary>
    /// ステージのアイコンを取得
    /// </summary>
    public Sprite Icon => _stageData?.icon;
    
    /// <summary>
    /// ステージの色を取得
    /// </summary>
    public Color Color => _stageData?.color ?? Color.white;
    
    /// <summary>
    /// StageDataを指定してノードを作成（nullの場合はUndefinedノード）
    /// </summary>
    public StageNode(StageData data = null)
    {
        _stageData = data;
        Type = data?.stageType ?? StageType.Undefined;
        Connections = new List<StageNode>();
    }
}