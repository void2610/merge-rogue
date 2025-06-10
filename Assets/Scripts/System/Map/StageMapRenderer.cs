using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Cysharp.Threading.Tasks;

public class StageMapRenderer : MonoBehaviour
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private GameObject playerIconPrefab;
    [SerializeField] private GameObject mapBackground;
    [SerializeField] private GameObject mapNodePrefab;
    [SerializeField] private GameObject mapConnectionPrefab;

    private GameObject _playerIconObj;
    private MapGenerator _mapGenerator;
    
    
    public void DrawMap(List<List<StageNode>> mapNodes, List<StageData> stageData)
    {
        ClearMap();

        var mapSize = _mapGenerator.GetMapSize();

        // 先にノード間の線を描画
        DrawConnections(mapNodes, mapSize);

        // ノードを描画
        DrawNodes(mapNodes, mapSize);
    }

    private void ClearMap()
    {
        var icons = mapBackground.GetComponentsInChildren<Transform>().ToList();
        // プレイヤーアイコンを保護して、他のオブジェクトのみ削除
        icons.Where(i => i != mapBackground.transform && i.gameObject != _playerIconObj)
            .ToList().ForEach(i => Destroy(i.gameObject));
    }

    private void DrawConnections(List<List<StageNode>> mapNodes, Vector2Int mapSize)
    {
        // スタートノードからの接続を描画
        var startNode = _mapGenerator.GetStartNode();
        foreach (var c in startNode.Connections)
        {
            DrawLine(startNode, c);
        }

        // その他のノード間の接続を描画
        for (var i = 1; i < mapSize.x; i++)
        {
            for (var j = 0; j < mapSize.y; j++)
            {
                if (mapNodes[i][j] == null) continue;
                foreach (var c in mapNodes[i][j].Connections)
                {
                    DrawLine(mapNodes[i][j], c);
                }
            }
        }
    }

    private void DrawNodes(List<List<StageNode>> mapNodes, Vector2Int mapSize)
    {
        DrawSingleNode(_mapGenerator.GetStartNode());

        for (var i = 1; i < mapSize.x; i++)
        {
            for (var j = 0; j < mapSize.y; j++)
            {
                if (mapNodes[i][j] == null) continue;
                DrawSingleNode(mapNodes[i][j]);
            }
        }
    }

    /// <summary>
    /// 単一のノードを描画
    /// </summary>
    private void DrawSingleNode(StageNode node)
    {
        var nodeObj = Instantiate(mapNodePrefab, mapBackground.transform);
        nodeObj.GetComponent<RectTransform>().localPosition = node.Position;
        nodeObj.name = $"{node.Type}";

        var image = nodeObj.GetComponent<Image>();
        image.sprite = node.Icon;
        image.color = node.Color;

        node.Obj = nodeObj;
    }

    private void DrawLine(StageNode a, StageNode b)
    {
        var g = Instantiate(mapConnectionPrefab, mapBackground.transform);
        g.GetComponent<RectTransform>().localPosition = a.Position;
        g.name = $"{a.Type} -> {b.Type}";
        var line = g.GetComponent<UILineRenderer>();

        // UI座標系で直接計算
        var pos = b.Position - a.Position;
        line.points = new[] { Vector2.zero, pos };
    }

    public void SetButtonEvents(List<List<StageNode>> mapNodes, System.Action<StageNode> onNodeClick)
    {
        // スタートノードのボタンイベント
        var startNode = _mapGenerator.GetStartNode();
        if (startNode.Obj)
        {
            var startButton = startNode.Obj.GetComponent<Button>();
            startButton.onClick.RemoveAllListeners();
            startButton.onClick.AddListener(() => onNodeClick(startNode));
        }

        // その他のノードのボタンイベント
        foreach (var column in mapNodes)
        {
            foreach (var node in column)
            {
                if (node == null || !node.Obj) continue;
                foreach (var connection in node.Connections)
                {
                    if (connection == null || !connection.Obj) continue;
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
                     where node != null && node.Obj
                     select node.Obj.GetComponent<Button>()))
        {
            button.interactable = false;
        }
    }

    public void SetNextNodeActive(StageNode currentStage, List<List<StageNode>> mapNodes)
    {
        var nextNodes = currentStage != null ? currentStage.Connections : new List<StageNode> { _mapGenerator.GetStartNode() };

        foreach (var column in mapNodes)
        {
            foreach (var node in column)
            {
                if (node == null || !node.Obj) continue;

                var button = node.Obj.GetComponent<Button>();
                button.interactable = nextNodes.Contains(node);
            }
        }
    }

    public void ChangeFocusNode(StageNode node, List<List<StageNode>> mapNodes)
    {
        foreach (var n in mapNodes.SelectMany(column => column))
        {
            if (n == null || !n.Obj) continue;
            if (n.Obj.TryGetComponent(out FocusSelectable f)) Destroy(f);
        }

        if (node?.Obj)
        {
            node.Obj.AddComponent<FocusSelectable>();
        }
    }


    public void MovePlayerIcon(RectTransform targetTransform, float duration = 0.5f)
    {
        var targetPosition = targetTransform.localPosition;
        var floatMove = _playerIconObj.GetComponent<FloatMove>();

        if (duration <= 0f)
        {
            _playerIconObj.GetComponent<RectTransform>().localPosition = targetPosition;
        }
        else
        {
            floatMove.MoveTo(targetPosition, duration);
        }
    }

    private void Awake()
    {
        _mapGenerator = this.GetComponent<MapGenerator>();
        _playerIconObj = Instantiate(playerIconPrefab, mapBackground.transform);
    }
}