using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StageNode
{
    public StageType Type { get; private set; }  // ステージの種類
    public Vector2 Position { get; private set; } // マップ上の位置
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
    /// StageDataと位置を指定してノードを作成
    /// </summary>
    public StageNode(StageData data, Vector2 position)
    {
        _stageData = data;
        Type = data?.stageType ?? StageType.Enemy;
        Position = position;
        Connections = new List<StageNode>();
    }
    
}