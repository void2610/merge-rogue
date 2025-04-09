using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelUpUI : MonoBehaviour
{
    [Serializable]
    private enum LevelUpType
    {
        Attack,
        Width,
        Ball
    }

    [SerializeField] private LevelUpType type;
    [SerializeField] private GameObject gaugePrefab;
    [SerializeField] private GameObject levelUpParticle;
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private Sprite gaugeSprite;
    [SerializeField] private Sprite fillGaugeSprite;
    [SerializeField] private Button button;
    [SerializeField] private Vector2 offset;
    [SerializeField] private float align;
    [SerializeField] private int maxLevel;
    private readonly List<SpriteRenderer> _gaugeList = new();
    private int _level = 0;

    private async UniTaskVoid Awake()
    {
        title.text = type.ToString();
        for (var i = 0; i < maxLevel; i++)
        {
            var gauge = Instantiate(gaugePrefab, this.transform).GetComponent<SpriteRenderer>();
            gauge.sprite = gaugeSprite;
            gauge.transform.localScale = Vector3.one * 70f;
            gauge.transform.SetParent(this.transform);
            gauge.transform.localPosition = new Vector2(align * i, 0) + offset;
            gauge.gameObject.SetActive(false);
            _gaugeList.Add(gauge);
        }
        
        button.onClick.AddListener(LevelUp);
        
        var canvasGroup = this.transform.parent.GetComponent<CanvasGroup>();
        Observable.EveryUpdate()
            .Select(_ => canvasGroup.alpha) // CanvasGroupのalpha値を取得
            .DistinctUntilChanged()        // 値が変化したときのみ処理を実行
            .Subscribe(alpha =>
            {
                // スプライトの色を取得してアルファ値を変更
                _gaugeList.ForEach(gauge => gauge.color = new Color(1.3f, 1.3f, 1.3f, alpha));
            })
            .AddTo(this);
        
        //ちょっと経ったらゲージを表示しておく
        await UniTask.Delay(500);
        _gaugeList.ForEach(gauge => gauge.gameObject.SetActive(true));
    }

    private void LevelUp() => LevelUpAsync().Forget();
    
    private async UniTaskVoid LevelUpAsync()
    {
        if (_level >= maxLevel) return;
        _level++;
        _gaugeList[_level - 1].sprite = fillGaugeSprite;
        _gaugeList[_level - 1].color = new Color(1.3f, 1.3f, 1.3f, 1);
        var p = Instantiate(levelUpParticle, _gaugeList[_level - 1].transform.position, Quaternion.identity);

        switch (type)
        {
            case LevelUpType.Attack:
                MergeManager.Instance.LevelUpAttack();
                break;
            case LevelUpType.Width:
                MergeManager.Instance.LevelUpWallWidth();
                break;
            case LevelUpType.Ball:
                MergeManager.Instance.LevelUpBallAmount();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        if (_level >= maxLevel)
        {
            button.interactable = false;
            button.gameObject.SetActive(false);
        }
        
        Player.RemainingLevelUps--;
        SeManager.Instance.PlaySe("levelUp");
        CameraMove.Instance.ShakeCamera(0.5f, 0.3f);
        Destroy(p, 1f);
        
        if (Player.RemainingLevelUps > 0) return;
        
        this.transform.parent.GetComponent<CanvasGroup>().interactable = false;
        
        await UniTask.Delay(1500);
        
        UIManager.Instance.EnableCanvasGroup("LevelUp", false);
        GameManager.Instance.ChangeState(GameManager.GameState.AfterBattle);
    }
}
