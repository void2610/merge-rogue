using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Cysharp.Threading.Tasks;

public class StageMapRenderer : MonoBehaviour
{
    [Header("マップ描画")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private GameObject playerIconPrefab;
    [SerializeField] private GameObject mapBackground;
    [SerializeField] private GameObject mapNodePrefab;
    [SerializeField] private GameObject mapConnectionPrefab;
    
    private GameObject _playerIconObj;
    private MapGenerator _mapGenerator;
    
    public void Initialize(MapGenerator mapGenerator)
    {
        _mapGenerator = mapGenerator;
    }
    
    public void DrawMap(List<List<StageNode>> mapNodes, List<StageData> stageData)
    {
        ClearMap();
        
        var mapSize = _mapGenerator.GetMapSize();
        
        // 先にノード間の線を描画
        DrawConnections(mapNodes, mapSize);
        
        // ノードを描画
        DrawNodes(mapNodes, mapSize, stageData);
    }
    
    private void ClearMap()
    {
        var icons = mapBackground.GetComponentsInChildren<Transform>().ToList();
        icons.Where(i => i != mapBackground.transform).ToList().ForEach(i => Destroy(i.gameObject));
    }
    
    private void DrawConnections(List<List<StageNode>> mapNodes, Vector2Int mapSize)
    {
        // スタートノードからの接続を描画
        foreach (var c in mapNodes[0][0].Connections.Where(c => c.Type != StageType.Undefined))
        {
            DrawLine(mapNodes[0][0], c);
        }
        
        // その他のノード間の接続を描画
        for (var i = 1; i < mapSize.x; i++)
        {
            for (var j = 0; j < mapSize.y; j++)
            {
                if (mapNodes[i][j].Type == StageType.Undefined) continue;
                foreach (var c in mapNodes[i][j].Connections.Where(c => c.Type != StageType.Undefined))
                {
                    DrawLine(mapNodes[i][j], c);
                }
            }
        }
    }
    
    private void DrawNodes(List<List<StageNode>> mapNodes, Vector2Int mapSize, List<StageData> stageData)
    {
        // デバッグ: ノード位置を確認
        Debug.Log($"StageMapRenderer - Drawing start node at position: {mapNodes[0][0].Position}");
        
        // スタートノードを描画
        var startNode = Instantiate(mapNodePrefab, mapBackground.transform);
        startNode.GetComponent<RectTransform>().localPosition = mapNodes[0][0].Position;
        startNode.name = $"{mapNodes[0][0].Type}";
        startNode.GetComponent<Image>().sprite = mapNodes[0][0].GetIcon(stageData);
        startNode.GetComponent<Image>().color = mapNodes[0][0].GetColor(stageData);
        mapNodes[0][0].Obj = startNode;
        Debug.Log($"Start node positioned at: {mapNodes[0][0].Position}");
        
        // その他のノードを描画
        for (var i = 1; i < mapSize.x; i++)
        {
            for (var j = 0; j < mapSize.y; j++)
            {
                if (mapNodes[i][j].Type == StageType.Undefined) continue;
                var nodeObj = Instantiate(mapNodePrefab, mapBackground.transform);
                nodeObj.GetComponent<RectTransform>().localPosition = mapNodes[i][j].Position;
                
                nodeObj.name = $"{mapNodes[i][j].Type}";
                nodeObj.GetComponent<Image>().sprite = mapNodes[i][j].GetIcon(stageData);
                nodeObj.GetComponent<Image>().color = mapNodes[i][j].GetColor(stageData);
                
                mapNodes[i][j].Obj = nodeObj;
                Debug.Log($"Node [{i},{j}] positioned at: {mapNodes[i][j].Position}");
            }
        }
    }
    
    private void DrawLine(StageNode a, StageNode b)
    {
        var g = Instantiate(mapConnectionPrefab, mapBackground.transform);
        g.GetComponent<RectTransform>().localPosition = a.Position;
        g.name = $"{a.Type} -> {b.Type}";
        var line = g.GetComponent<UILineRenderer>();
        
        // UI座標系で直接計算
        var pos = b.Position - a.Position;
        line.points = new Vector2[] {Vector2.zero, pos};
        Debug.Log($"Line from {a.Position} to {b.Position}, relative: {pos}");
    }
    
    public void SetButtonEvents(List<List<StageNode>> mapNodes, System.Action<StageNode> onNodeClick)
    {
        // スタートノードのボタンイベント
        var startButton = mapNodes[0][0].Obj.GetComponent<Button>();
        startButton.onClick.RemoveAllListeners();
        startButton.onClick.AddListener(() => onNodeClick(mapNodes[0][0]));
        
        // その他のノードのボタンイベント
        foreach (var column in mapNodes)
        {
            foreach (var node in column)
            {
                if (node.Type == StageType.Undefined) continue;
                foreach (var connection in node.Connections)
                {
                    if (connection.Type == StageType.Undefined) continue;
                    var button = connection.Obj.GetComponent<Button>();
                    
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(() => onNodeClick(connection));
                }
            }
        }
    }
    
    public void SetAllNodeInactive(List<List<StageNode>> mapNodes)
    {
        foreach (var button in mapNodes.SelectMany(column => from node in column 
                 where node.Type != StageType.Undefined 
                 select node.Obj.GetComponent<Button>()))
        {
            button.interactable = false;
        }
    }
    
    public void SetNextNodeActive(StageNode currentStage, List<List<StageNode>> mapNodes)
    {
        var nextNodes = currentStage != null ? currentStage.Connections : new List<StageNode>{mapNodes[0][0]};
        
        foreach (var column in mapNodes)
        {
            foreach (var node in column)
            {
                if (node.Type == StageType.Undefined) continue;
                
                var button = node.Obj.GetComponent<Button>();
                button.interactable = nextNodes.Contains(node);
            }
        }
    }
    
    public void ChangeFocusNode(StageNode node, List<List<StageNode>> mapNodes)
    {
        foreach (var n in mapNodes.SelectMany(column => column))
        {
            if (!n.Obj) continue;
            if (n.Obj.TryGetComponent(out FocusSelectable f)) Destroy(f);
        }
        
        node.Obj.AddComponent<FocusSelectable>();
    }
    
    public GameObject CreatePlayerIcon()
    {
        _playerIconObj = Instantiate(playerIconPrefab, mapBackground.transform);
        return _playerIconObj;
    }
    
    public void MovePlayerIcon(Vector3 targetPosition, float duration = 0.5f)
    {
        if (_playerIconObj)
        {
            // UI座標系での移動
            var uiPosition = targetPosition + new Vector3(0, 20, 0);  // UIスケールで調整
            _playerIconObj.GetComponent<FloatMove>().MoveTo(uiPosition, duration);
            Debug.Log($"Moving player icon to: {uiPosition}");
        }
    }
}