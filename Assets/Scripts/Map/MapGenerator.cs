using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VContainer;

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
    
    private IRandomService _randomService;
    
    [Inject]
    public void InjectDependencies(IRandomService randomService)
    {
        _randomService = randomService;
    }

    public Vector2Int GetMapSize() => mapSize;
    public List<StageData> GetStageData() => stageData;
    public StageNode GetStartNode() 
    {
        if (MapNodes.Count == 0 || MapNodes[0].Count <= mapSize.y / 2) return null;
        return MapNodes[0][mapSize.y / 2];
    }
    
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
                    nodeStageData = stageDataDict.GetValueOrDefault(StageType.Boss);
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
                var randomYOffset = _randomService.RandomRange(-1, 2); // -1から1までの値
                var nextY = Mathf.Clamp(currentY + randomYOffset, 0, mapSize.y - 1);
                
                // スタートノードとゴールノードの特別な処理
                if (i == 1) nextY = _randomService.RandomRange(0, mapSize.y);
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
        // ボスタイプを除外
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
        
        var r = _randomService.RandomRange(0.0f, sum);
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
        if (startNode == null) return; // スタートノードが存在しない場合は何もしない
        
        queue.Enqueue(startNode);
        reachableNodes.Add(startNode);
        
        // BFS で到達可能なノードを全て探索
        while (queue.Count > 0)
        {
            var currentNode = queue.Dequeue();
            
            foreach (var connection in currentNode.Connections)
            {
                if (reachableNodes.Add(connection))
                {
                    queue.Enqueue(connection);
                }
            }
        }
        
        // 到達不可能なノードを削除
        for (var i = 0; i < mapSize.x; i++)
        {
            for (var j = 0; j < mapSize.y; j++)
            {
                var node = MapNodes[i][j];
                if (!reachableNodes.Contains(node))
                {
                    // 既に描画されている場合はオブジェクトを削除
                    if (node.Obj)
                    {
                        Destroy(node.Obj);
                    }
                    
                    // ノードをMapNodesから削除
                    MapNodes[i][j] = null;
                }
            }
        }
        
        // 全ノードの接続リストから削除されたノードへの参照を削除
        for (var i = 0; i < mapSize.x; i++)
        {
            for (var j = 0; j < mapSize.y; j++)
            {
                var node = MapNodes[i][j];
                if (node != null)
                {
                    node.Connections.RemoveAll(connection => !reachableNodes.Contains(connection));
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
            if (oldNode == null) return; // スタートノードが削除されている場合は何もしない
            
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
                    if (node == null) continue; // 削除されたノードはスキップ
                    
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