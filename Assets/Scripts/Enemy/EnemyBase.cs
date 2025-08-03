using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using VContainer;

public class EnemyBase : MonoBehaviour, IEntity
{
    public EnemyData Data { get; private set; }
    public string EnemyName => Data.displayName;
    public EnemyType EnemyType => Data.enemyType;
    public bool IsMelee { get; private set; } // 近接タイプかどうか
    public int ActionInterval { get; private set; } = 1;
    public int Attack { get; private set; } = 1;
    public int Coin { get; private set; } = 0;
    public int Exp { get; private set; } = 0;
    public int Health { get; protected set; }
    public int MaxHealth { get; protected set; }
    public Dictionary<StatusEffectType, int> StatusEffectStacks { get; } = new();

    protected int TurnCount = 0;
    protected int Stage = 0;
    protected float Magnification = 1;
    
    private EnemyActionData _nextAction;
    private CanvasGroup _hpSliderCanvasGroup;
    private TextMeshProUGUI _healthText;
    private Slider _healthSlider;
    private TextMeshProUGUI _attackCountText;
    private Image _attackIcon;
    protected IRandomService RandomService;
    
    public StatusEffectUI StatusEffectUI { get; private set; }

    /// <summary>
    /// 攻撃距離に到達したかチェック
    /// </summary>
    private bool IsInAttackRange()
    {
        if (!IsMelee) return true;
        
        var currentIndex = EnemyContainer.Instance.GetEnemyIndex(this);
        if (currentIndex < 0) return false;
        
        return currentIndex <= Data.attackRange;
    }

    public async UniTask UpdateStatusEffects()
    {
        if (!this.gameObject) return;
        await StatusEffectProcessor.ProcessTurnEnd(this);
    }

    private int ModifyIncomingDamage(int amount)
    {
        return StatusEffectProcessor.ModifyIncomingDamage(this, amount);
    }

    private int ModifyOutgoingAttack(AttackType type, int amount)
    {
        return StatusEffectProcessor.ModifyOutgoingAttack(this, type, amount);
    }
    
    /// <summary>
    /// HPバーの位置を更新
    /// </summary>
    public void UpdateHpBarPosition()
    {
        if (!_healthSlider) return;
        
        var c = UIManager.Instance.GetUICamera();
        var pos = c.WorldToScreenPoint(this.transform.position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            UIManager.Instance.GetUICanvas().GetComponent<RectTransform>(), pos, c, out var localPosition
        );
        localPosition.y += Data.hpSliderYOffset;
        _healthSlider.GetComponent<RectTransform>().anchoredPosition = localPosition;
    }
    
    public void Damage(AttackType type, int damage)
    {
        if(!this) return;
                
        // 演出
        var m = this.GetComponent<SpriteRenderer>().material;
        m.DOColor(Color.red, 0).OnComplete(() =>
        {
            m.DOColor(new Color(0.7f,0.7f,0.7f), 0.3f);
        });
        ParticleManager.Instance.HitParticle(this.transform.position + new Vector3(-0.3f, 0.2f, 0));
        
        // 状態異常でダメージを更新
        damage = ModifyIncomingDamage(damage);
        ParticleManager.Instance.DamageText(damage, this.transform.position.x, type.GetColor());
        Health -= damage;
        _healthSlider.value = Health;
        _healthText.text = Health + "/" + MaxHealth;
        if (Health > 0) return;
        
        Health = 0;
        _healthText.text = Health + "/" + MaxHealth;
        Death();
    }
    
    public void Heal(int healAmount)
    {
        Health += healAmount;
        Health = Mathf.Min(Health, MaxHealth);
        _healthSlider.value = Health;
        _healthText.text = Health + "/" + MaxHealth;
    }

    public void Action()
    {
        // Freeze状態なら行動しない
        if (StatusEffectProcessor.CheckFreeze(this)) return;
        
        // 攻撃距離に到達していない場合
        if (!IsInAttackRange())
        {
            // 移動
            EnemyContainer.Instance.MoveEnemyOneStep(this);
            
            // 移動後の表示更新（攻撃は次のターンから）
            if (IsInAttackRange())
            {
                // 射程に入ったが、このターンは移動のみ
                _attackCountText.text = (ActionInterval - TurnCount).ToString();
                UpdateAttackIcon(_nextAction);
            }
            else
            {
                // まだ射程外
                _attackCountText.text = "1";
                UpdateAttackIcon(new EnemyActionData { type = ActionType.Move });
            }
            return;
        }
        
        TurnCount++;
        if(TurnCount == ActionInterval)
        {
            TurnCount = 0;
            
            // _nextActionがnullの場合は初期化
            _nextAction ??= GetNextAction();
            
            _nextAction.Action();
            _nextAction = GetNextAction();
            UpdateAttackIcon(_nextAction);
        }
        else
        {
            this.transform.DOMoveY(0.75f, 0.05f).SetRelative(true).SetEase(Ease.OutQuad).OnComplete(() =>
            {
                this.transform.DOMoveY(-.75f, 0.05f).SetRelative(true).SetEase(Ease.InQuad).SetLink(gameObject);
            }).SetLink(gameObject);
        }

        _attackCountText.text = (ActionInterval - TurnCount).ToString();
    }
    
    protected virtual EnemyActionData GetNextAction()
    {
        var behaviour = ChooseByProbability(this.Data.actions);
        return EnemyActionFactory.CreateActionByName(behaviour.actionName, this, (int)(Stage * 0.6f + behaviour.value));
    }
    
    /// <summary>
    /// EnemyBehaviorDataのリストから、probabilityの重みに従ってランダムに1つ選ぶ
    /// </summary>
    private EnemyData.EnemyBehaviorData ChooseByProbability(List<EnemyData.EnemyBehaviorData> list)
    {
        if (list == null || list.Count == 0)
            throw new ArgumentException("EnemyBehaviorData list is empty or null");

        var total = list.Sum(x => x.probability);
        if (total <= 0f)
            throw new ArgumentException("Total probability must be greater than zero");

        var r = RandomService?.RandomRange(0f, total) ?? UnityEngine.Random.Range(0f, total);

        var cumulative = 0f;
        foreach (var item in list)
        {
            cumulative += item.probability;
            if (r <= cumulative)
                return item;
        }

        // fallback（浮動小数点の誤差対策）
        return list[^1];
    }
    
    private void OnAppear()
    {
        // 出現のTween
        this.GetComponent<SpriteRenderer>().sprite = Data.sprites[0];
        this.GetComponent<SpriteRenderer>().DOColor(new Color(1, 1, 1, 0), 0);
        _hpSliderCanvasGroup.DOFade(1, 0.5f).SetLink(gameObject);
        this.GetComponent<SpriteRenderer>().DOFade(1, 0.5f).SetLink(gameObject);
    }

    public void OnDisappear()
    {
        SeManager.Instance.PlaySe("coin");
        var coinPrefab = EnemyContainer.Instance.GetCoinPrefab();
        var displayCoinCount = CoinDisplayCalculator.CalculateDisplayCount(Coin);
        for (var i = 0; i < displayCoinCount; i++)
        {
            var c = Instantiate(coinPrefab).GetComponent<Coin>();
            c?.SetUp(this.transform.position.x);
        }
        GameManager.Instance.AddCoin(Coin);
        
        _hpSliderCanvasGroup.DOFade(0, 0.5f).SetLink(gameObject);
        
        this.GetComponent<SpriteRenderer>().DOFade(0, 0.5f).OnComplete(() =>
        {
            Destroy(_healthSlider.gameObject);
            Destroy(this.gameObject);
        }).SetLink(gameObject);
    }

    private void Death()
    {
        EventManager.OnEnemyDefeated.OnNext(this);
        this.transform.parent.GetComponent<EnemyContainer>().RemoveEnemy(this.gameObject);
    }

    private void UpdateAttackIcon(EnemyActionData a)
    {
        _attackIcon.color = a.type switch
        {
            ActionType.Attack => Color.red,
            ActionType.Heal => Color.green,
            ActionType.Buff => Color.cyan,
            ActionType.Debuff => Color.magenta,
            ActionType.Move => Color.blue,
            _ => Color.white
        };
    }
    
    public virtual void Init(EnemyData d, int stage, IRandomService randomService, IEnemyDifficultyService difficultyService, int act = 1)
    {
        RandomService = randomService;
        // 敵のデータを設定
        Data = d;
        Stage = stage;
        
        // 近接タイプの判定（EnemyDataのisMeleeフラグを使用）
        IsMelee = d.isMelee;
        
        // 難易度サービスから倍率を取得
        Magnification = difficultyService.CalculateMagnification(stage, act, d.enemyType);
        var healthMagnification = difficultyService.CalculateHealthMagnification(Magnification, d.enemyType);
        var attackMagnification = difficultyService.CalculateAttackMagnification(Magnification, d.enemyType);
        var rewardMagnification = difficultyService.CalculateRewardMagnification(Magnification, d.enemyType);
        
        MaxHealth = (int)(RandomService.RandomRange(d.maxHealthMin, d.maxHealthMax) * healthMagnification);
        Health = MaxHealth;
        Attack = (int)(d.attack * attackMagnification);
        Exp = d.exp + (int)(Magnification * rewardMagnification);
        Coin = d.coin + (int)(Magnification * rewardMagnification);
        ActionInterval = d.interval;
        
        // スプライトアニメーションを設定
        this.GetComponent<SpriteSheetAnimator>().Setup(d.sprites, d.framePerSecond);
        this.transform.Translate(0, d.enemyYOffset, 0);
        this.transform.localScale *= 0.7f;

        // UIの初期化
        var c = UIManager.Instance.GetUICamera();
        var hpSlider = EnemyContainer.Instance.GetHpSliderPrefab();
        hpSlider.transform.localScale = Vector3.one * 0.6f;
        var slider = Instantiate(hpSlider, UIManager.Instance.GetEnemyUIContainer());
        var pos = c.WorldToScreenPoint(this.transform.position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            UIManager.Instance.GetUICanvas().GetComponent<RectTransform>(), pos, c, out var localPosition
        );
        localPosition.y += d.hpSliderYOffset;
        slider.GetComponent<RectTransform>().anchoredPosition = localPosition;
        _hpSliderCanvasGroup = slider.GetComponent<CanvasGroup>();
        _healthText = slider.transform.Find("HPText").GetComponent<TextMeshProUGUI>();
        _healthSlider = slider.GetComponent<Slider>();
        _attackCountText = slider.transform.Find("AttackCount").GetComponent<TextMeshProUGUI>();
        _attackIcon = slider.transform.Find("AttackIcon").GetComponent<Image>();
        StatusEffectUI = slider.GetComponentInChildren<StatusEffectUI>();
        _healthSlider.maxValue = MaxHealth;
        _healthSlider.value = Health;
        _healthText.text = Health + "/" + MaxHealth;
        // 次の行動を必ず設定
        _nextAction = GetNextAction();
        
        // 初期状態での表示設定
        if (!IsInAttackRange())
        {
            // 移動が必要な場合
            _attackCountText.text = "1";
            UpdateAttackIcon(new EnemyActionData { type = ActionType.Move });
        }
        else
        {
            // 攻撃可能な場合
            _attackCountText.text = (ActionInterval - TurnCount).ToString();
            UpdateAttackIcon(_nextAction);
        }
        
        OnAppear();
    }
}
