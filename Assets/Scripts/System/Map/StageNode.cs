using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StageNode
{
    public StageType Type;             // ステージの種類
    public Vector2 Position;           // マップ上の位置
    public readonly List<StageNode> Connections; // 次のステージへの接続
    public GameObject Obj;             // マップ上のオブジェクト

    public StageNode(StageType t)
    {
        Type = t;
        Connections = new List<StageNode>();
    }
    
    public Sprite GetIcon(List<StageData> list) => list.First(s => s.stageType == Type).icon;
    public Color GetColor(List<StageData> list) => list.First(s => s.stageType == Type).color;
}