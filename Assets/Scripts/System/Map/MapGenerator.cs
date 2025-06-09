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
    public List<StageData> GetStageData() => stageData;
    public StageNode GetStartNode() => MapNodes[0][mapSize.y / 2];
    
    public void GenerateMap()
    {
        MapNodes.Clear();
        
        // マップグリッドを初期化
        InitializeMapGrid();
        
        // パスを生成しながらステージタイプを決定
        GeneratePaths();
        
        // 到達不可能なノードを削除
        RemoveUnreachableNodes();
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
                if (i == 0 && j == mid)
                {
                    nodeStageData = stageDataDict.GetValueOrDefault(StageType.Enemy);
                }
                // ボスノード
                else if (i == mapSize.x - 1 && j == mid)
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
            }
        }
    }
    
    
    private void GeneratePaths()
    {
        var mid = mapSize.y / 2;
        
        // スタートからゴールに向かってランダムに接続
        for (var _ = 0; _ < pathCount; _++)
        {
            var currentNode = MapNodes[0][mid];
            for (var i = 1; i < mapSize.x; i++)
            {
                var currentY = MapNodes[i-1].FindIndex(node => node == currentNode);
                var randomYOffset = GameManager.Instance.RandomRange(-1, 2); // -1から1までの値
                var nextY = Mathf.Clamp(currentY + randomYOffset, 0, mapSize.y - 1);
                
                if (i == 1) nextY = GameManager.Instance.RandomRange(0, mapSize.y);
                else if (i == mapSize.x - 1) nextY = mid;
                
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
    
    private void RemoveUnreachableNodes()
    {
        var reachableNodes = new HashSet<StageNode>();
        var queue = new Queue<StageNode>();
        
        // スタートノードから探索開始
        var startNode = GetStartNode();
        queue.Enqueue(startNode);
        reachableNodes.Add(startNode);
        
        // BFS で到達可能なノードを全て探索
        while (queue.Count > 0)
        {
            var currentNode = queue.Dequeue();
            
            foreach (var connection in currentNode.Connections)
            {
                if (!reachableNodes.Contains(connection) && connection.Type != StageType.Undefined)
                {
                    reachableNodes.Add(connection);
                    queue.Enqueue(connection);
                }
            }
        }
        
        // 到達不可能なノードのタイプをUndefinedに設定
        for (var i = 0; i < mapSize.x; i++)
        {
            for (var j = 0; j < mapSize.y; j++)
            {
                var node = MapNodes[i][j];
                if (!reachableNodes.Contains(node))
                {
                    node.SetType(StageType.Undefined);
                }
            }
        }
    }
    
    public void SetStartStageType(StageType type)
    {
        var mid = mapSize.y / 2;
        if (MapNodes.Count > 0 && MapNodes[0].Count > mid)
        {
            var oldNode = MapNodes[0][mid];
            var position = oldNode.Position;
            var connections = new List<StageNode>(oldNode.Connections);
            var newStageData = stageData.FirstOrDefault(s => s.stageType == type);
            
            // 新しいノードを作成
            MapNodes[0][mid] = new StageNode(newStageData, position);
                
            // 接続を復元
            foreach (var connection in connections)
            {
                MapNodes[0][mid].Connections.Add(connection);
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
                            node.Connections[k] = MapNodes[0][mid];
                        }
                    }
                }
            }
        }
    }
}