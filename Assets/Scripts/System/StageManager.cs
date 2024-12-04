using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
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
        public int interval;
        public Sprite icon;
    }
    
    public enum StageType
    {
        Enemy,
        Shop,
        Events,
        Treasure,
        Boss,
        Undefined
    }
    
    [Serializable]
    public class StageNode
    {
        public StageType type;             // ステージの種類
        public Vector2 position;           // マップ上の位置
        public List<StageNode> connections; // 次のステージへの接続

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
    [SerializeField] private GameObject mapBackground;
    [SerializeField] private GameObject mapNodePrefab;
    [SerializeField] private GameObject mapConnectionPrefab;
    [SerializeField] private Vector2 mapOffset;
    [SerializeField] private Vector2 mapMargin;

    [Header("ステージ")]
    [SerializeField] private List<StageData> stageData　= new();
    [SerializeField] private List<StageType> stageTypes = new();
    [SerializeField] private Vector2Int mapSize;
    private List<List<StageNode>> mapNodes = new();
    
    public readonly ReactiveProperty<int> currentStage = new(-1);
    private static readonly int mainTex = Shader.PropertyToID("_MainTex");
    private int enemyStageCount;
    private Tween torchTween;

    public StageType GetCurrentStageType()
    {
        return stageTypes[currentStage.Value];
    }

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
        // マップの初期化
        for(var i = 0; i < mapSize.x; i++)
        {
            mapNodes.Add(new List<StageNode>());
            for(var j = 0; j < mapSize.y; j++)
            {
                mapNodes[i].Add(new StageNode(StageType.Undefined));
            }
        }
        
        // ノードを作成
        var startNode = new StageNode(StageType.Enemy);
        mapNodes[0] = new List<StageNode> {startNode};
        mapNodes[0][0].position = new Vector2(mapOffset.x, mapOffset.y);

        var mid = (mapSize.x / 2);
        for (var i = 1; i < mapSize.x; i++)
        {
            var cnt = GameManager.Instance.RandomRange(3, mapSize.x-1);
            for(var j = 0; j < cnt; j++)
            {
                var r = GameManager.Instance.RandomRange(0, mapSize.y);
                if (mapNodes[i][r].type != StageType.Undefined)
                {
                    j--;
                    continue;
                }
                mapNodes[i][r] = new StageNode(ChoseStage().stageType);
                var my = (r - mid) * mapMargin.y;
                mapNodes[i][r].position = new Vector2((i * mapMargin.x) + mapOffset.x, my + mapOffset.y);
            }
        }
        
        var bossNode = new StageNode(StageType.Boss);
        mapNodes.Add(new List<StageNode> {bossNode});
        mapNodes[^1][0].position = new Vector2((mapSize.x * mapMargin.x) + mapOffset.x, mapOffset.y);
        
        // ノード間を接続
        foreach (var n in mapNodes[1])
        {
            if (n.type == StageType.Undefined) continue;
            startNode.connections.Add(n);
        }
        
        foreach (var n in mapNodes[^2])
        {
            if (n.type == StageType.Undefined) continue;
            n.connections.Add(bossNode);
        }
    }
    
    private void DrawLine(StageNode a, StageNode b)
    {
        var g = Instantiate(mapConnectionPrefab,a.position, Quaternion.identity, mapBackground.transform);
        g.name = $"{a.type} -> {b.type}";
        var line = g.GetComponent<UILineRenderer>();
        var p1 = Camera.main.WorldToScreenPoint(a.position);
        var p2 = Camera.main.WorldToScreenPoint(b.position);
        var pos = new Vector2(p2.x - p1.x - 35, p2.y - p1.y);
        line.points = new Vector2[2] {Vector2.zero, pos};
    }

    private void DrawMap()
    {
        // 先にノード間の線を描画
        foreach (var c in mapNodes[0][0].connections)
        {
            if (c.type == StageType.Undefined) continue;
            DrawLine(mapNodes[0][0], c);
        }
        
        foreach (var c in mapNodes[^2])
        {
            if (c.type == StageType.Undefined) continue;
            DrawLine(c, mapNodes[^1][0]);
        }
        
        // ノードを描画
        var s = Instantiate(mapNodePrefab, mapNodes[0][0].position , Quaternion.identity, mapBackground.transform);
        s.GetComponent<Image>().sprite = mapNodes[0][0].GetIcon(stageData);

        for (var i = 1; i < mapSize.x; i++)
        {
            for (var j = 0; j < mapSize.y; j++)
            {
                if (mapNodes[i][j].type == StageType.Undefined) continue;
                var g = Instantiate(mapNodePrefab, mapNodes[i][j].position, Quaternion.identity, mapBackground.transform);
                g.GetComponent<Image>().sprite = mapNodes[i][j].GetIcon(stageData);
            }
        }
        
        var e = Instantiate(mapNodePrefab, mapNodes[^1][0].position, Quaternion.identity, mapBackground.transform);
        e.GetComponent<Image>().sprite = mapNodes[^1][0].GetIcon(stageData);
    }

    public void NextStage()
    {
        return;
        // 演出
        Utils.Instance.WaitAndInvoke(0.2f, () =>
        {
            SeManager.Instance.PlaySe("footsteps");
        });
        DOTween.To(() => m.GetTextureOffset(mainTex), x => m.SetTextureOffset(mainTex, x), new Vector2(1, 0), 2.0f)
            .SetEase(Ease.InOutSine).OnComplete(() =>
            {
                m.SetTextureOffset(mainTex, new Vector2(0, 0));
                
                var tmp = torches[0];
                torches.RemoveAt(0);
                torches.Add(tmp);
                torchTween.Kill();
                tmp.transform.position = defaultTorchPosition + new Vector3(torchInterval * (torches.Count-1), 0, 0);
            }).SetUpdate(true); 
        
        for(var i = 0; i < torches.Count; i++)
        {
            var t = torches[i];
            var tween = t.transform.DOMove(t.transform.position - new Vector3(torchInterval, 0, 0), 2.0f)
                .SetEase(Ease.InOutSine)
                .SetUpdate(true);
            if (i == 0) torchTween = tween;
        }
        torches[^1].SetActive(Random.Range(0.0f, 1.0f) < 0.5f);

        

        // ステージ進行
        Utils.Instance.WaitAndInvoke(2.0f, () =>
        {
            if (currentStage.Value + 1 < stageTypes.Count)
                currentStage.Value++;
            else
                currentStage.Value = 0;

            switch (stageTypes[currentStage.Value])
            {
                case StageType.Enemy:
                    enemyStageCount++;
                    GameManager.Instance.enemyContainer.SpawnEnemy(enemyStageCount);
                    GameManager.Instance.ChangeState(GameManager.GameState.BattlePreparation);
                    break;
                case StageType.Boss:
                    GameManager.Instance.enemyContainer.SpawnBoss();
                    GameManager.Instance.ChangeState(GameManager.GameState.BattlePreparation);
                    break;
                case StageType.Shop:
                    GameManager.Instance.ChangeState(GameManager.GameState.Shop);
                    EventManager.OnShopEnter.Trigger(0);
                    break;
                case StageType.Events:
                    GameManager.Instance.ChangeState(GameManager.GameState.Other);
                    break;
                case StageType.Treasure:
                    GameManager.Instance.ChangeState(GameManager.GameState.Shop);
                    break;
            }
        });
    }
    
    private void DecideStage()
    {
        var availableStages = new List<StageType>();
        var stageIntervals = new Dictionary<StageType, int>();

        foreach (var data in stageData)
        {
            for (var i = 0; i < data.probability * 100; i++)
            {
                availableStages.Add(data.stageType);
            }
            stageIntervals[data.stageType] = 0;
        }

        for (var i = 0; i < mapSize.y; i++)
        {
            var selectableStages = availableStages.FindAll(stage => stageIntervals[stage] <= 0);
            if (selectableStages.Count == 0)
            {
                foreach (var key in stageIntervals.Keys.ToList())
                {
                    stageIntervals[key]--;
                }
                selectableStages = availableStages;
            }

            StageType selectedStage = selectableStages[UnityEngine.Random.Range(0, selectableStages.Count)];
            stageTypes.Add(selectedStage);

            foreach (var key in stageIntervals.Keys.ToList())
            {
                stageIntervals[key]--;
            }
            stageIntervals[selectedStage] = stageData.Find(data => data.stageType == selectedStage).interval;
        }
    }

    public void Start()
    {
        GenerateMap();
        DrawMap();
        
        // DecideStage();
        // stageTypes[0] = StageType.Shop;
        
        m.SetTextureOffset(mainTex, new Vector2(0, 0)); 
    }
}
