using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    [Header("ステージ設定")]
    [SerializeField] private List<StageData> stageData = new();
    [SerializeField] private Vector2Int mapSize;
    [SerializeField] private int pathCount;
    [SerializeField] private StageType startStage;
    [SerializeField] private Vector2 mapOffset;
    [SerializeField] private Vector2 mapMargin;

    public List<List<StageNode>> MapNodes { get; } = new();

    public Vector2Int GetMapSize() => mapSize;
    
    public void GenerateMap()
    {
        MapNodes.Clear();
        
        // デバッグ: マップパラメータを確認
        Debug.Log($"MapGenerator - mapSize: {mapSize}, mapOffset: {mapOffset}, mapMargin: {mapMargin}, pathCount: {pathCount}");
        
        // マップグリッドを初期化
        InitializeMapGrid();
        
        // パスを生成しながらステージタイプを決定
        GeneratePaths();
    }
    
    private void InitializeMapGrid()
    {
        var stageDataDict = stageData.ToDictionary(s => s.stageType);
        
        for (var i = 0; i < mapSize.x; i++)
        {
            MapNodes.Add(new List<StageNode>());
            var mid = mapSize.y / 2;
            for (var j = 0; j < mapSize.y; j++)
            {
                StageData nodeStageData;
                
                // スタートノード
                if (i == 0 && j == 0)
                {
                    nodeStageData = stageDataDict.GetValueOrDefault(StageType.Enemy);
                }
                // ボスノード
                else if (i == mapSize.x - 1 && j == 0)
                {
                    nodeStageData = stageDataDict.GetValueOrDefault(StageType.Boss);
                }
                // その他のノードはランダムステージ
                else
                {
                    nodeStageData = ChooseStage();
                }
                
                var my = (j - mid) * mapMargin.y;
                var pos = new Vector2((i * mapMargin.x) + mapOffset.x, my + mapOffset.y);
                var node = new StageNode(nodeStageData, pos);
                
                MapNodes[i].Add(node);
                Debug.Log($"Node [{i},{j}] position set to: {pos}");
            }
        }
    }
    
    
    private void GeneratePaths()
    {
        // スタートからゴールに向かってランダムに接続
        for (var _ = 0; _ < pathCount; _++)
        {
            var currentNode = MapNodes[0][0];
            for (var i = 1; i < mapSize.x; i++)
            {
                var currentY = MapNodes[i-1].FindIndex(node => node == currentNode);
                var randomYOffset = GameManager.Instance.RandomRange(-1, 2); // -1から1までの値
                var nextY = Mathf.Clamp(currentY + randomYOffset, 0, mapSize.y - 1);
                
                if (i == 1) nextY = GameManager.Instance.RandomRange(0, mapSize.y);
                else if (i == mapSize.x - 1) nextY = 0;
                
                var nextNode = MapNodes[i][nextY];
                
                if (!currentNode.Connections.Contains(nextNode))
                    currentNode.Connections.Add(nextNode);
                currentNode = nextNode;
            }
        }
    }
    
    
    private StageData ChooseStage()
    {
        // ボスステージを除外したリストを作成
        var eligibleStages = stageData.Where(s => s.stageType != StageType.Boss).ToList();
        
        if (eligibleStages.Count == 0)
        {
            Debug.LogWarning("選択可能なステージがありません。");
            return stageData.FirstOrDefault();
        }
        
        var sum = 0f;
        foreach (var s in eligibleStages)
        {
            sum += s.probability;
        }
        
        var r = GameManager.Instance.RandomRange(0.0f, sum);
        float cumulative = 0;
        
        foreach (var s in eligibleStages)
        {
            cumulative += s.probability;
            if (r < cumulative)
            {
                return s;
            }
        }
        
        return eligibleStages[0];
    }
    
    public void SetStartStageType(StageType type)
    {
        if (MapNodes.Count > 0 && MapNodes[0].Count > 0)
        {
            var oldNode = MapNodes[0][0];
            var position = oldNode.Position;
            var connections = new List<StageNode>(oldNode.Connections);
            var newStageData = stageData.FirstOrDefault(s => s.stageType == type);
            
            // 新しいノードを作成
            MapNodes[0][0] = new StageNode(newStageData, position);
                
            // 接続を復元
            foreach (var connection in connections)
            {
                MapNodes[0][0].Connections.Add(connection);
            }
            
            // 他のノードからの参照を更新
            for (var i = 0; i < MapNodes.Count; i++)
            {
                for (var j = 0; j < MapNodes[i].Count; j++)
                {
                    var node = MapNodes[i][j];
                    for (var k = 0; k < node.Connections.Count; k++)
                    {
                        if (node.Connections[k] == oldNode)
                        {
                            node.Connections[k] = MapNodes[0][0];
                        }
                    }
                }
            }
        }
    }
    
    public List<StageData> GetStageData() => stageData;
    
    // デバッグ用: 典型的なマップパラメータを設定
    [ContextMenu("Set Default Map Parameters")]
    public void SetDefaultMapParameters()
    {
        mapSize = new Vector2Int(6, 5);
        pathCount = 3;
        mapOffset = new Vector2(-400, 0);
        mapMargin = new Vector2(200, 100);
        Debug.Log("Default map parameters set: mapSize(6,5), pathCount(3), mapOffset(-400,0), mapMargin(200,100)");
    }
}