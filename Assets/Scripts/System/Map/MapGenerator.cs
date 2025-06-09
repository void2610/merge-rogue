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
        
        // マップの初期化
        InitializeMapGrid();
        
        // スタートノードとボスノードを作成
        CreateStartAndBossNodes();
        
        // パスを生成
        GeneratePaths();
        
        // ステージタイプを割り当て
        AssignStageTypes();
    }
    
    private void InitializeMapGrid()
    {
        for (var i = 0; i < mapSize.x; i++)
        {
            _mapNodes.Add(new List<StageNode>());
            var mid = mapSize.y / 2;
            for (var j = 0; j < mapSize.y; j++)
            {
                _mapNodes[i].Add(new StageNode(StageType.Undefined));
                var my = (j - mid) * mapMargin.y;
                var pos = new Vector2((i * mapMargin.x) + mapOffset.x, my + mapOffset.y);
                _mapNodes[i][j].Position = pos;
                Debug.Log($"Node [{i},{j}] position set to: {pos}");
            }
        }
    }
    
    private void CreateStartAndBossNodes()
    {
        // スタートノードの位置を保持（InitializeMapGridで設定された位置をそのまま使用）
        var startPosition = _mapNodes[0][0].Position;
        var startNode = new StageNode(StageType.Enemy);
        _mapNodes[0][0] = startNode;
        _mapNodes[0][0].Position = startPosition;  // 元の位置を保持
        Debug.Log($"Start node position: {startPosition}");
        
        // ボスノードの位置を保持
        var bossPosition = _mapNodes[^1][0].Position;
        var bossNode = new StageNode(StageType.Boss);
        _mapNodes[^1][0] = bossNode;
        _mapNodes[^1][0].Position = bossPosition;  // 元の位置を保持
        Debug.Log($"Boss node position: {bossPosition}");
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
        // Undefined以外のステージタイプを割り当てる
        foreach (var node in _mapNodes.SelectMany(column => column.Where(node => node.Connections.Count > 0)))
        {
            node.Type = ChooseStage().stageType;
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
            _mapNodes[0][0].Type = type;
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