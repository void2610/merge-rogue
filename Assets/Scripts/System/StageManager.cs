using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using R3;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class StageManager : MonoBehaviour
{
    [Serializable]
    public class StageData
    {
        public StageType stageType;
        public float probability;
        public Sprite icon;
    }
    
    public enum StageType
    {
        Enemy,
        Shop,
        Treasure,
        Rest,
        Events,
        Boss,
        Undefined
    }
    
    public class StageNode
    {
        public StageType type;             // ステージの種類
        public Vector2 position;           // マップ上の位置
        public List<StageNode> connections; // 次のステージへの接続
        public GameObject obj;             // マップ上のオブジェクト

        public StageNode(StageType t)
        {
            type = t;
            connections = new List<StageNode>();
        }
        
        public Sprite GetIcon(List<StageData> list)
        {
            return list.First(s => s.stageType == type).icon;
        }
    }

    [Header("背景")]
    [SerializeField] private Material m;
    [SerializeField] private List<GameObject> torches = new();
    [SerializeField] private Vector3 defaultTorchPosition;
    [SerializeField] private float torchInterval = 5;
    
    [Header("マップ描画")]
    [SerializeField] private GameObject playerIconPrefab;
    [SerializeField] private GameObject mapBackground;
    [SerializeField] private GameObject mapNodePrefab;
    [SerializeField] private GameObject mapConnectionPrefab;
    [SerializeField] private Vector2 mapOffset;
    [SerializeField] private Vector2 mapMargin;
    private GameObject playerIconObj;

    [Header("ステージ")]
    [SerializeField] private List<StageData> stageData　= new();
    [SerializeField] private List<StageType> stageTypes = new();
    [SerializeField] private Vector2Int mapSize;
    [SerializeField] private int pathCount;
    public readonly ReactiveProperty<int> currentStageCount = new(-1);
    private readonly List<List<StageNode>> mapNodes = new();
    public StageNode currentStage { get; private set; } = null;
    private static readonly int mainTex = Shader.PropertyToID("_MainTex");
    private Tween torchTween;

    private StageData ChoseStage()
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
    
    private void GenerateMap()
    {
        mapNodes.Clear();
        currentStage = null;
        var icons = mapBackground.GetComponentsInChildren<Transform>().ToList();
        icons.Where(i => i != mapBackground.transform).ToList().ForEach(i => Destroy(i.gameObject));
        
        // マップの初期化
        for (var i = 0; i < mapSize.x; i++)
        {
            mapNodes.Add(new List<StageNode>());
            var mid = mapSize.y / 2;
            for (var j = 0; j < mapSize.y; j++)
            {
                mapNodes[i].Add(new StageNode(StageType.Undefined));
                var my = (j - mid) * mapMargin.y;
                mapNodes[i][j].position = new Vector2((i * mapMargin.x) + mapOffset.x, my + mapOffset.y);
            }
        }

        // スタートノードを作成
        var startNode = new StageNode(StageType.Enemy);
        mapNodes[0][0] = startNode;
        mapNodes[0][0].position = new Vector2(mapOffset.x, mapOffset.y);

        // ゴールノードを作成
        var bossNode = new StageNode(StageType.Boss);
        mapNodes[^1][0] = bossNode;
        mapNodes[^1][0].position = new Vector2((mapSize.x * mapMargin.x) + mapOffset.x, mapOffset.y);

        // スタートからゴールに向かってランダムに接続
        for (var _ = 0; _ < pathCount; _++)
        {
            var currentNode = mapNodes[0][0];
            for (var i = 1; i < mapSize.x; i++)
            {
                var currentY = mapNodes[i-1].FindIndex(node => node == currentNode);
                var randomYOffset = GameManager.Instance.RandomRange(-1, 2); // -1から1までの値
                var nextY = Mathf.Clamp(currentY + randomYOffset, 0, mapSize.y - 1);
                
                if( i == 1) nextY = GameManager.Instance.RandomRange(0, mapSize.y);
                else if (i == mapSize.x - 1) nextY = 0;
                
                var nextNode = mapNodes[i][nextY];
                if (!currentNode.connections.Contains(nextNode))
                    currentNode.connections.Add(nextNode);
                currentNode = nextNode;
            }
        }
        
        // Undefined以外のステージタイプを割り当てる
        foreach (var node in mapNodes.SelectMany(column => column.Where(node => node.connections.Count > 0)))
        {
            node.type = ChoseStage().stageType; // ランダムにステージタイプを割り当てる
        }
    }
    
    private void DrawLine(StageNode a, StageNode b)
    {
        var g = Instantiate(mapConnectionPrefab,a.position, Quaternion.identity, mapBackground.transform);
        g.name = $"{a.type} -> {b.type}";
        var line = g.GetComponent<UILineRenderer>();
        if (Camera.main == null) return;
        var p1 = Camera.main.WorldToScreenPoint(a.position);
        var p2 = Camera.main.WorldToScreenPoint(b.position);
        var pos = new Vector2(p2.x - p1.x, p2.y - p1.y);
        line.points = new Vector2[2] {Vector2.zero, pos};
    }
    
    private void SetButtonEvent()
    {
        var b = mapNodes[0][0].obj.GetComponent<Button>();
        b.onClick.RemoveAllListeners();
        b.onClick.AddListener(() =>
        {
            NextStage(mapNodes[0][0]).Forget();
        });
        
        foreach (var column in mapNodes)
        {
            foreach (var node in column)
            {
                if (node.type == StageType.Undefined) continue;
                foreach (var c in node.connections)
                {
                    if (c.type == StageType.Undefined) continue;
                    var button = c.obj.GetComponent<Button>();
                    
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(() =>
                    {
                        NextStage(c).Forget();
                    });
                }
            }
        }
    }

    private void DrawMap()
    {
        // 先にノード間の線を描画
        foreach (var c in mapNodes[0][0].connections.Where(c => c.type != StageType.Undefined))
        {
            DrawLine(mapNodes[0][0], c);
        }
        
        for (var i = 1; i < mapSize.x; i++)
        {
            for (var j = 0; j < mapSize.y; j++)
            {
                if (mapNodes[i][j].type == StageType.Undefined) continue;
                foreach (var c in mapNodes[i][j].connections.Where(c => c.type != StageType.Undefined))
                {
                    DrawLine(mapNodes[i][j], c);
                }
            }
        }
        
        // ノードを描画
        var s = Instantiate(mapNodePrefab, mapNodes[0][0].position , Quaternion.identity, mapBackground.transform);
        s.name = $"{mapNodes[0][0].type}";
        s.GetComponent<Image>().sprite = mapNodes[0][0].GetIcon(stageData);
        mapNodes[0][0].obj = s;

        for (var i = 1; i < mapSize.x; i++)
        {
            for (var j = 0; j < mapSize.y; j++)
            {
                if (mapNodes[i][j].type == StageType.Undefined) continue;
                var g = Instantiate(mapNodePrefab, mapNodes[i][j].position, Quaternion.identity, mapBackground.transform);
                
                g.name = $"{mapNodes[i][j].type}";
                g.GetComponent<Image>().sprite = mapNodes[i][j].GetIcon(stageData);
                if (mapNodes[i][j].type == StageType.Enemy)
                    g.GetComponent<Image>().color = Color.red;
                else if (mapNodes[i][j].type == StageType.Boss)
                    g.GetComponent<Image>().color = Color.black;
                
                mapNodes[i][j].obj = g;
            }
        }
    }

    public void SetNextNodeActive()
    {
        // ボスを倒したらマップを再生成して次のステージを設定
        if (currentStage?.type == StageType.Boss)
        {
            // ボスを倒したら回復
            GameManager.Instance.Player.HealToFull();
            GenerateMap();
            DrawMap();
            SetButtonEvent();
            
            playerIconObj = Instantiate(playerIconPrefab, mapBackground.transform);
            var pos = mapNodes[^1][0].obj.GetComponent<RectTransform>().localPosition;
            playerIconObj.GetComponent<FloatMove>().MoveTo(pos + new Vector3(0, 2, 0), 0.5f);
        }
        
        var nextNodes = currentStage != null ? currentStage.connections : new List<StageNode>{mapNodes[0][0]};
        
        foreach (var column in mapNodes)
        {
            foreach (var node in column)
            {
                if (node.type == StageType.Undefined) continue;
                
                var button = node.obj.GetComponent<Button>();
                button.interactable = nextNodes.Contains(node);
            }
        }
    }
    
    private void SetAllNodeInactive()
    {
        foreach (var button in mapNodes.SelectMany(column => from node in column where node.type != StageType.Undefined select node.obj.GetComponent<Button>()))
        {
            button.interactable = false;
        }
    }

    private async UniTaskVoid NextStage(StageNode next)
    {
        if (GameManager.Instance.IsGameOver) return;
        
        // 演出
        SetAllNodeInactive();
        GameManager.Instance.UIManager.EnableCanvasGroup("Map", false);
        SeManager.Instance.WaitAndPlaySe("footsteps", 0.2f);
        DOTween.To(() => m.GetTextureOffset(mainTex), x => m.SetTextureOffset(mainTex, x), new Vector2(1, 0), 2.0f)
            .SetEase(Ease.InOutSine).OnComplete(() =>
            {
                m.SetTextureOffset(mainTex, new Vector2(0, 0));
                
                var tmp = torches[0];
                torches.RemoveAt(0);
                torches.Add(tmp);
                torchTween.Kill();
                tmp.transform.position = defaultTorchPosition + new Vector3(torchInterval * (torches.Count-1), 0, 0);
            }); 
        
        for(var i = 0; i < torches.Count; i++)
        {
            var t = torches[i];
            var tween = t.transform.DOMove(t.transform.position - new Vector3(torchInterval, 0, 0), 2.0f)
                .SetEase(Ease.InOutSine);
            if (i == 0) torchTween = tween;
        }
        torches[^1].SetActive(Random.Range(0.0f, 1.0f) < 0.5f);
        
        var pos = next.obj.GetComponent<RectTransform>().localPosition;
        playerIconObj.GetComponent<FloatMove>().MoveTo(pos + new Vector3(0, 2, 0), 0.5f);
        
        await UniTask.Delay(2000);
        
        // ステージ進行
        currentStageCount.Value++;
        currentStage = next;
        
        if(currentStage.type == StageType.Events)
        {
            // ランダムなステージに移動
            var r = GameManager.Instance.RandomRange(0, 4);
            ProcessStage((StageType)r);
        }
        else
        {
            ProcessStage(currentStage.type);
        }
    }

    private void ProcessStage(StageType s)
    {
        switch (s)
        {
            case StageType.Enemy:
                // 敵の出現量と強さを設定
                GameManager.Instance.EnemyContainer.SpawnEnemy(currentStageCount.Value + 1, currentStageCount.Value);
                GameManager.Instance.ChangeState(GameManager.GameState.Merge);
                break;
            case StageType.Boss:
                GameManager.Instance.EnemyContainer.SpawnEnemy(currentStageCount.Value + 1, currentStageCount.Value + 5);
                GameManager.Instance.ChangeState(GameManager.GameState.Merge);
                break;
            case StageType.Shop:
                GameManager.Instance.ChangeState(GameManager.GameState.Event);
                EventManager.OnShopEnter.Trigger(0);
                Shop.Instance.OpenShop();
                GameManager.Instance.UIManager.EnableCanvasGroup("Shop", true);
                break;
            case StageType.Rest:
                GameManager.Instance.ChangeState(GameManager.GameState.Event);
                EventManager.OnRestEnter.Trigger(0);
                GameManager.Instance.UIManager.EnableCanvasGroup("Rest", true);
                break;
            case StageType.Treasure:
                GameManager.Instance.ChangeState(GameManager.GameState.Event);
                GameManager.Instance.UIManager.EnableCanvasGroup("Treasure", true); 
                    
                var count = GameManager.Instance.RandomRange(1, 4);
                var rarity = GameManager.Instance.RandomRange(0, 4);
                Treasure.Instance.OpenTreasure(count, (Rarity)rarity);
                break;
            case StageType.Events:
            case StageType.Undefined:
            default:
                throw new ArgumentOutOfRangeException(nameof(s), s, null);
        }
    }

    public void Awake()
    {
        GenerateMap();
        mapNodes[0][0].type = StageType.Enemy;
        currentStage = null;
        
        DrawMap();
        SetButtonEvent();
        SetAllNodeInactive();
        
        playerIconObj = Instantiate(playerIconPrefab, mapBackground.transform);
        
        m.SetTextureOffset(mainTex, new Vector2(0, 0)); 
    }

    public void Start()
    {
        NextStage(mapNodes[0][0]).Forget();
    }
}