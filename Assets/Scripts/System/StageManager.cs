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
    }
    
    [Serializable]
    public class StageNode
    {
        public StageType type;             // ステージの種類
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
    [SerializeField] private Vector2 mapOffset;
    [SerializeField] private Vector2 mapMargin;

    [Header("ステージ")]
    [SerializeField] private int stageLength = 10;
    [SerializeField] private List<StageData> stageData　= new();
    [SerializeField] private List<StageType> stageTypes = new();
    [SerializeField] private StageNode startNode;
    
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
        var sum = stageData.Sum(s => s.probability);
        var r = GameManager.Instance.RandomRange(0.0f, sum);
        return stageData.First(s => r < s.probability);
    }
    
    private void GenerateMap()
    {
        // スタートノードを作成
        var current = new StageNode(StageType.Enemy);
        startNode = current;

        // 中間階層のノードを作成
        for (var i = 0; i < stageLength; i++)
        {
            var type = (StageType)GameManager.Instance.RandomRange(0, Enum.GetValues(typeof(StageType)).Length - 1);
            current.connections.Add(new StageNode(type));
            current = current.connections[0];
        }

        // ノードをランダムに接続
        // var current = s;
        // foreach (var node in intermediateNodes)
        // {
        //     current.connections.Add(node);
        //     current = node;
        // }
        
        // ゴールノードを作成
        var goalNode = new StageNode(StageType.Boss);
        current.connections.Add(goalNode); // 最後の中間階層をゴールに接続

        // ランダムに枝分かれを追加
        // foreach (var node in intermediateNodes)
        // {
        //     if (GameManager.Instance.RandomRange(0.0f, 1.0f) < 0.5f)
        //     {
        //         var type = (StageType)GameManager.Instance.RandomRange(0, Enum.GetValues(typeof(StageType)).Length - 1);
        //         var branch = new StageNode(type);
        //         node.connections.Add(branch);
        //     }
        // }
    }

    private void DrawMap(StageNode node)
    {
        int depth = 0;
        var start = Instantiate(mapNodePrefab, new Vector3((depth * mapMargin.x) + mapOffset.x, mapOffset.y, 0), Quaternion.identity, mapBackground.transform);
        start.GetComponent<Image>().sprite = node.GetIcon(stageData);
        Debug.Log(node.type);

        while (true)
        {
            depth++;
            if (node.connections.Count == 0) break;
            node = node.connections[0];
            Debug.Log(node.type);
            int index = 0;
            foreach (var c in node.connections)
            {
                var g = Instantiate(mapNodePrefab, new Vector3((depth * mapMargin.x) + mapOffset.x, (index * mapMargin.y) + mapOffset.y, 0), Quaternion.identity, mapBackground.transform);
                g.GetComponent<Image>().sprite = node.GetIcon(stageData);
                index++;
            }
        }

        depth--;
        var boss = Instantiate(mapNodePrefab, new Vector3((depth * mapMargin.x) + mapOffset.x, mapOffset.y, 0), Quaternion.identity, mapBackground.transform);
        boss.GetComponent<Image>().sprite = node.GetIcon(stageData);
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

    public void Start()
    {
        GenerateMap();
        DrawMap(startNode);
        // stageTypes[0] = StageType.Shop;
        
        m.SetTextureOffset(mainTex, new Vector2(0, 0)); 
    }
}
