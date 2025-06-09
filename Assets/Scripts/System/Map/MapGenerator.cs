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
    
    private readonly List<List<StageNode>> _mapNodes = new();
    
    public List<List<StageNode>> MapNodes => _mapNodes;
    public Vector2Int GetMapSize() => mapSize;
    
    public void GenerateMap()
    {
        _mapNodes.Clear();
        
        // デバッグ: マップパラメータを確認
        Debug.Log($"MapGenerator - mapSize: {mapSize}, mapOffset: {mapOffset}, mapMargin: {mapMargin}, pathCount: {pathCount}");
        
        // マップの初期化（StageDataを使用）
        InitializeMapGrid();
        
        // パスを生成
        GeneratePaths();
        
        // ステージタイプを割り当て（スタートとボスノードも含む）
        AssignStageTypes();
    }
    
    private void InitializeMapGrid()
    {
        // StageDataの辞書を作成
        var stageDataDict = stageData.ToDictionary(s => s.stageType);
        
        for (var i = 0; i < mapSize.x; i++)
        {
            _mapNodes.Add(new List<StageNode>());
            var mid = mapSize.y / 2;
            for (var j = 0; j < mapSize.y; j++)
            {
                StageNode node;
                
                // スタートノード
                if (i == 0 && j == 0)
                {
                    var startStageData = stageDataDict.GetValueOrDefault(StageType.Enemy);
                    node = new StageNode(startStageData);
                }
                // ボスノード
                else if (i == mapSize.x - 1 && j == 0)
                {
                    var bossStageData = stageDataDict.GetValueOrDefault(StageType.Boss);
                    node = new StageNode(bossStageData);
                }
                // その他のノード（後でタイプを割り当てる）
                else
                {
                    node = new StageNode();
                }
                
                var my = (j - mid) * mapMargin.y;
                var pos = new Vector2((i * mapMargin.x) + mapOffset.x, my + mapOffset.y);
                node.Position = pos;
                
                _mapNodes[i].Add(node);
                Debug.Log($"Node [{i},{j}] position set to: {pos}");
            }
        }
    }
    
    
    private void GeneratePaths()
    {
        // スタートからゴールに向かってランダムに接続
        for (var _ = 0; _ < pathCount; _++)
        {
            var currentNode = _mapNodes[0][0];
            for (var i = 1; i < mapSize.x; i++)
            {
                var currentY = _mapNodes[i-1].FindIndex(node => node == currentNode);
                var randomYOffset = GameManager.Instance.RandomRange(-1, 2); // -1から1までの値
                var nextY = Mathf.Clamp(currentY + randomYOffset, 0, mapSize.y - 1);
                
                if (i == 1) nextY = GameManager.Instance.RandomRange(0, mapSize.y);
                else if (i == mapSize.x - 1) nextY = 0;
                
                var nextNode = _mapNodes[i][nextY];
                if (!currentNode.Connections.Contains(nextNode))
                    currentNode.Connections.Add(nextNode);
                currentNode = nextNode;
            }
        }
    }
    
    private void AssignStageTypes()
    {
        // Undefinedのステージタイプを割り当てる
        for (var i = 0; i < _mapNodes.Count; i++)
        {
            for (var j = 0; j < _mapNodes[i].Count; j++)
            {
                var node = _mapNodes[i][j];
                if (node.Connections.Count > 0 && node.Type == StageType.Undefined)
                {
                    var chosenStageData = ChooseStage();
                    var position = node.Position;
                    var connections = new List<StageNode>(node.Connections);
                    
                    // 新しいノードを作成
                    _mapNodes[i][j] = new StageNode(chosenStageData)
                    {
                        Position = position
                    };
                    
                    // 接続を復元
                    foreach (var connection in connections)
                    {
                        _mapNodes[i][j].Connections.Add(connection);
                    }
                    
                    // 他のノードからこのノードへの参照を更新
                    UpdateReferencesToNode(node, _mapNodes[i][j]);
                }
            }
        }
    }
    
    /// <summary>
    /// 古いノードへの参照を新しいノードへの参照に更新
    /// </summary>
    private void UpdateReferencesToNode(StageNode oldNode, StageNode newNode)
    {
        foreach (var column in _mapNodes)
        {
            foreach (var node in column)
            {
                for (var i = 0; i < node.Connections.Count; i++)
                {
                    if (node.Connections[i] == oldNode)
                    {
                        node.Connections[i] = newNode;
                    }
                }
            }
        }
    }
    
    private StageData ChooseStage()
    {
        float sum = 0;
        foreach (var s in stageData)
        {
            sum += s.probability;
        }
        
        float r = GameManager.Instance.RandomRange(0.0f, sum);
        float cumulative = 0;
        
        foreach (var s in stageData)
        {
            cumulative += s.probability;
            if (r < cumulative)
            {
                return s;
            }
        }
        
        return stageData[0];
    }
    
    public void SetStartStageType(StageType type)
    {
        if (_mapNodes.Count > 0 && _mapNodes[0].Count > 0)
        {
            var oldNode = _mapNodes[0][0];
            var position = oldNode.Position;
            var connections = new List<StageNode>(oldNode.Connections);
            var newStageData = stageData.FirstOrDefault(s => s.stageType == type);
            
            // 新しいノードを作成
            _mapNodes[0][0] = new StageNode(newStageData) { Position = position };
                
            // 接続を復元
            foreach (var connection in connections)
            {
                _mapNodes[0][0].Connections.Add(connection);
            }
            
            // 他のノードからの参照を更新
            UpdateReferencesToNode(oldNode, _mapNodes[0][0]);
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