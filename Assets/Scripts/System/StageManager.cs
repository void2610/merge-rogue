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
        public StageType Type;             // ステージの種類
        public Vector2 Position;           // マップ上の位置
        public readonly List<StageNode> Connections; // 次のステージへの接続
        public GameObject Obj;             // マップ上のオブジェクト

        public StageNode(StageType t)
        {
            Type = t;
            Connections = new List<StageNode>();
        }
        
        public Sprite GetIcon(List<StageData> list)
        {
            return list.First(s => s.stageType == Type).icon;
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
    private GameObject _playerIconObj;

    [Header("ステージ")]
    [SerializeField] private List<StageData> stageData　= new();
    [SerializeField] private List<StageType> stageTypes = new();
    [SerializeField] private Vector2Int mapSize;
    [SerializeField] private int pathCount;
    public readonly ReactiveProperty<int> CurrentStageCount = new(-1);
    private readonly List<List<StageNode>> _mapNodes = new();
    public StageNode CurrentStage { get; private set; } = null;
    private static readonly int _mainTex = Shader.PropertyToID("_MainTex");
    private Tween _torchTween;
    private int _act = 0;

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
        _mapNodes.Clear();
        CurrentStage = null;
        var icons = mapBackground.GetComponentsInChildren<Transform>().ToList();
        icons.Where(i => i != mapBackground.transform).ToList().ForEach(i => Destroy(i.gameObject));
        
        // マップの初期化
        for (var i = 0; i < mapSize.x; i++)
        {
            _mapNodes.Add(new List<StageNode>());
            var mid = mapSize.y / 2;
            for (var j = 0; j < mapSize.y; j++)
            {
                _mapNodes[i].Add(new StageNode(StageType.Undefined));
                var my = (j - mid) * mapMargin.y;
                _mapNodes[i][j].Position = new Vector2((i * mapMargin.x) + mapOffset.x, my + mapOffset.y);
            }
        }

        // スタートノードを作成
        var startNode = new StageNode(StageType.Enemy);
        _mapNodes[0][0] = startNode;
        _mapNodes[0][0].Position = new Vector2(mapOffset.x, mapOffset.y);

        // ゴールノードを作成
        var bossNode = new StageNode(StageType.Boss);
        _mapNodes[^1][0] = bossNode;
        _mapNodes[^1][0].Position = new Vector2((mapSize.x * mapMargin.x) + mapOffset.x, mapOffset.y);

        // スタートからゴールに向かってランダムに接続
        for (var _ = 0; _ < pathCount; _++)
        {
            var currentNode = _mapNodes[0][0];
            for (var i = 1; i < mapSize.x; i++)
            {
                var currentY = _mapNodes[i-1].FindIndex(node => node == currentNode);
                var randomYOffset = GameManager.Instance.RandomRange(-1, 2); // -1から1までの値
                var nextY = Mathf.Clamp(currentY + randomYOffset, 0, mapSize.y - 1);
                
                if( i == 1) nextY = GameManager.Instance.RandomRange(0, mapSize.y);
                else if (i == mapSize.x - 1) nextY = 0;
                
                var nextNode = _mapNodes[i][nextY];
                if (!currentNode.Connections.Contains(nextNode))
                    currentNode.Connections.Add(nextNode);
                currentNode = nextNode;
            }
        }
        
        // Undefined以外のステージタイプを割り当てる
        foreach (var node in _mapNodes.SelectMany(column => column.Where(node => node.Connections.Count > 0)))
        {
            node.Type = ChoseStage().stageType; // ランダムにステージタイプを割り当てる
        }
    }
    
    private void DrawLine(StageNode a, StageNode b)
    {
        var g = Instantiate(mapConnectionPrefab,a.Position, Quaternion.identity, mapBackground.transform);
        g.name = $"{a.Type} -> {b.Type}";
        var line = g.GetComponent<UILineRenderer>();
        if (Camera.main == null) return;
        var p1 = Camera.main.WorldToScreenPoint(a.Position);
        var p2 = Camera.main.WorldToScreenPoint(b.Position);
        var pos = new Vector2(p2.x - p1.x, p2.y - p1.y);
        line.points = new Vector2[2] {Vector2.zero, pos};
    }
    
    private void SetButtonEvent()
    {
        var b = _mapNodes[0][0].Obj.GetComponent<Button>();
        b.onClick.RemoveAllListeners();
        b.onClick.AddListener(() =>
        {
            NextStage(_mapNodes[0][0]).Forget();
        });
        
        foreach (var column in _mapNodes)
        {
            foreach (var node in column)
            {
                if (node.Type == StageType.Undefined) continue;
                foreach (var c in node.Connections)
                {
                    if (c.Type == StageType.Undefined) continue;
                    var button = c.Obj.GetComponent<Button>();
                    
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
        foreach (var c in _mapNodes[0][0].Connections.Where(c => c.Type != StageType.Undefined))
        {
            DrawLine(_mapNodes[0][0], c);
        }
        
        for (var i = 1; i < mapSize.x; i++)
        {
            for (var j = 0; j < mapSize.y; j++)
            {
                if (_mapNodes[i][j].Type == StageType.Undefined) continue;
                foreach (var c in _mapNodes[i][j].Connections.Where(c => c.Type != StageType.Undefined))
                {
                    DrawLine(_mapNodes[i][j], c);
                }
            }
        }
        
        // ノードを描画
        var s = Instantiate(mapNodePrefab, _mapNodes[0][0].Position , Quaternion.identity, mapBackground.transform);
        s.name = $"{_mapNodes[0][0].Type}";
        s.GetComponent<Image>().sprite = _mapNodes[0][0].GetIcon(stageData);
        _mapNodes[0][0].Obj = s;

        for (var i = 1; i < mapSize.x; i++)
        {
            for (var j = 0; j < mapSize.y; j++)
            {
                if (_mapNodes[i][j].Type == StageType.Undefined) continue;
                var g = Instantiate(mapNodePrefab, _mapNodes[i][j].Position, Quaternion.identity, mapBackground.transform);
                
                g.name = $"{_mapNodes[i][j].Type}";
                g.GetComponent<Image>().sprite = _mapNodes[i][j].GetIcon(stageData);
                if (_mapNodes[i][j].Type == StageType.Enemy)
                    g.GetComponent<Image>().color = Color.red;
                else if (_mapNodes[i][j].Type == StageType.Boss)
                    g.GetComponent<Image>().color = Color.black;
                
                _mapNodes[i][j].Obj = g;
            }
        }
    }

    public void SetNextNodeActive()
    {
        // ボスを倒したらマップを再生成して次のステージを設定
        if (CurrentStage?.Type == StageType.Boss)
        {
            _act++;
            // ボスを倒したら回復
            GameManager.Instance.Player.HealToFull();
            GenerateMap();
            DrawMap();
            SetButtonEvent();
            
            _playerIconObj = Instantiate(playerIconPrefab, mapBackground.transform);
            var pos = _mapNodes[^1][0].Obj.GetComponent<RectTransform>().localPosition;
            _playerIconObj.GetComponent<FloatMove>().MoveTo(pos + new Vector3(0, 2, 0), 0.5f);
        }
        
        var nextNodes = CurrentStage != null ? CurrentStage.Connections : new List<StageNode>{_mapNodes[0][0]};
        
        foreach (var column in _mapNodes)
        {
            foreach (var node in column)
            {
                if (node.Type == StageType.Undefined) continue;
                
                var button = node.Obj.GetComponent<Button>();
                button.interactable = nextNodes.Contains(node);
            }
        }
    }
    
    private void SetAllNodeInactive()
    {
        foreach (var button in _mapNodes.SelectMany(column => from node in column where node.Type != StageType.Undefined select node.Obj.GetComponent<Button>()))
        {
            button.interactable = false;
        }
    }

    private async UniTaskVoid NextStage(StageNode next)
    {
        if (GameManager.Instance.IsGameOver) return;
        
        // 演出
        SetAllNodeInactive();
        UIManager.Instance.EnableCanvasGroup("Map", false);
        SeManager.Instance.WaitAndPlaySe("footsteps", 0.2f);
        DOTween.To(() => m.GetTextureOffset(_mainTex), x => m.SetTextureOffset(_mainTex, x), new Vector2(1, 0), 2.0f)
            .SetEase(Ease.InOutSine).OnComplete(() =>
            {
                m.SetTextureOffset(_mainTex, new Vector2(0, 0));
                
                var tmp = torches[0];
                torches.RemoveAt(0);
                torches.Add(tmp);
                _torchTween.Kill();
                tmp.transform.position = defaultTorchPosition + new Vector3(torchInterval * (torches.Count-1), 0, 0);
            }); 
        
        for(var i = 0; i < torches.Count; i++)
        {
            var t = torches[i];
            var tween = t.transform.DOMove(t.transform.position - new Vector3(torchInterval, 0, 0), 2.0f)
                .SetEase(Ease.InOutSine);
            if (i == 0) _torchTween = tween;
        }
        torches[^1].SetActive(Random.Range(0.0f, 1.0f) < 0.5f);
        
        var pos = next.Obj.GetComponent<RectTransform>().localPosition;
        _playerIconObj.GetComponent<FloatMove>().MoveTo(pos + new Vector3(0, 2, 0), 0.5f);
        
        await UniTask.Delay(2000);
        
        // ステージ進行
        CurrentStageCount.Value++;
        CurrentStage = next;
        
        if(CurrentStage.Type == StageType.Events)
        {
            // ランダムなステージに移動
            var r = GameManager.Instance.RandomRange(0, 5);
            ProcessStage((StageType)r);
        }
        else
        {
            ProcessStage(CurrentStage.Type);
        }
    }

    private void ProcessStage(StageType s)
    {
        switch (s)
        {
            case StageType.Enemy:
                // 敵の出現量と強さを設定
                GameManager.Instance.EnemyContainer.SpawnEnemy(CurrentStageCount.Value + 1, _act, CurrentStageCount.Value);
                GameManager.Instance.ChangeState(GameManager.GameState.Merge);
                break;
            case StageType.Boss:
                GameManager.Instance.EnemyContainer.SpawnBoss(_act, CurrentStageCount.Value + 5);
                GameManager.Instance.ChangeState(GameManager.GameState.Merge);
                break;
            case StageType.Shop:
                GameManager.Instance.ChangeState(GameManager.GameState.Event);
                EventManager.OnShopEnter.Trigger(0);
                Shop.Instance.OpenShop();
                UIManager.Instance.EnableCanvasGroup("Shop", true);
                break;
            case StageType.Rest:
                GameManager.Instance.ChangeState(GameManager.GameState.Event);
                EventManager.OnRestEnter.Trigger(0);
                UIManager.Instance.EnableCanvasGroup("Rest", true);
                break;
            case StageType.Treasure:
                GameManager.Instance.ChangeState(GameManager.GameState.Event);
                UIManager.Instance.EnableCanvasGroup("Treasure", true); 
                    
                var count = GameManager.Instance.RandomRange(1, 4);
                var rarity = GameManager.Instance.RandomRange(0, 4);
                Treasure.Instance.OpenTreasure(count, (Rarity)rarity);
                break;
            case StageType.Events:
                GameManager.Instance.ChangeState(GameManager.GameState.Event);
                UIManager.Instance.EnableCanvasGroup("Event", true);
                
                EventProcessor.Instance.SetRandomEvent();
                break;
            case StageType.Undefined:
            default:
                throw new ArgumentOutOfRangeException(nameof(s), s, null);
        }
    }

    public void Awake()
    {
        GenerateMap();
        _mapNodes[0][0].Type = StageType.Enemy;
        CurrentStage = null;
        
        DrawMap();
        SetButtonEvent();
        SetAllNodeInactive();
        
        _playerIconObj = Instantiate(playerIconPrefab, mapBackground.transform);
        
        m.SetTextureOffset(_mainTex, new Vector2(0, 0)); 
    }

    public void Start()
    {
        NextStage(_mapNodes[0][0]).Forget();
    }
}