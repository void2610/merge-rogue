using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class EnemyBase : MonoBehaviour, IEntity
{
    public EnemyData Data { get; private set; }
    public string EnemyName => Data.displayName;
    public EnemyType EnemyType => Data.enemyType;
    public int ActionInterval { get; private set; } = 1;
    public int Attack { get; private set; } = 1;
    public int Coin { get; private set; } = 0;
    public int Exp { get; private set; } = 0;
    public int Health { get; protected set; }
    public int MaxHealth { get; protected set; }
    public List<StatusEffectBase> StatusEffects { get; } = new();

    protected int TurnCount = 0;
    protected int Stage = 0;
    protected float Magnification = 1;
    
    private EnemyActionData _nextAction;
    private CanvasGroup _hpSliderCanvasGroup;
    private TextMeshProUGUI _healthText;
    private Slider _healthSlider;
    private TextMeshProUGUI _attackCountText;
    private StatusEffectUI _statusEffectUI;
    private Image _attackIcon;
    
    public StatusEffectUI StatusEffectUI => _statusEffectUI;

    
    public void AddStatusEffect(StatusEffectBase effect)
    {
        var existingEffect = StatusEffects.Find(e => e.Type == effect.Type);
        if (existingEffect != null)
            existingEffect.AddStack(effect.StackCount);
        else
        {
            effect.SetEntityPosition(this.transform.position);
            StatusEffects.Add(effect);
        }
        _statusEffectUI.UpdateUI(StatusEffects);
    }
    
    public void RemoveStatusEffect(StatusEffectType type, int stack)
    {
        var effect = StatusEffects.Find(e => e.Type == type);
        if (effect == null) return;
        if (effect.ReduceStack(stack))
            StatusEffects.Remove(effect);
        _statusEffectUI.UpdateUI(StatusEffects);
    }
    
    public async UniTask UpdateStatusEffects()
    {
        if (!this.gameObject) return;
        for (var i = StatusEffects.Count - 1; i >= 0; i--)
        {
            StatusEffects[i].OnTurnEnd(this);
            if (StatusEffects[i].ReduceStack()) StatusEffects.RemoveAt(i);
            await UniTask.Delay((int)(500 * GameManager.Instance.TimeScale));
        }
        
        _statusEffectUI.UpdateUI(StatusEffects);
    }
    
    public int ModifyIncomingDamage(int amount)
    {
        var v = StatusEffects.Aggregate(amount, (current, effect) => effect.ModifyDamage(this, current));
        _statusEffectUI.UpdateUI(StatusEffects);
        return v;
    }
    
    public Dictionary<AttackType, int> ModifyOutgoingAttack(Dictionary<AttackType, int> amount)
    {
        var v = StatusEffects.Aggregate(amount, (current, effect) => effect.ModifyAttack(this, current));
        _statusEffectUI.UpdateUI(StatusEffects);
        return v;
    }
    
    public void OnBattleEnd()
    {
        foreach (var effect in StatusEffects)
        {
            effect.OnBattleEnd(this);
        }
        _statusEffectUI.UpdateUI(StatusEffects);
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
        _statusEffectUI.UpdateUI(StatusEffects);
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
        FreezeEffect freeze = (FreezeEffect)StatusEffects.Find(e => e.Type == StatusEffectType.Freeze);
        if (freeze != null && freeze.IsFrozen()) return;
        
        TurnCount++;
        if(TurnCount == ActionInterval)
        {
            TurnCount = 0;
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
        var behaviour = this.Data.actions.ChooseByProbability();
        return EnemyActionFactory.CreateActionByName(behaviour.actionName, this, (int)(Stage * 0.6f + behaviour.value));
    }

    private void DoAttack()
    {
        // 状態異常で攻撃力を更新
        var dic = new Dictionary<AttackType, int> {{AttackType.Normal, Attack}};
        var damage = ModifyOutgoingAttack(dic)[AttackType.Normal];
        GameManager.Instance.Player.Damage(AttackType.Normal, Mathf.Max(1, damage));
        this.transform.DOMoveX(-0.75f, 0.02f).SetRelative(true).OnComplete(() =>
                {
                    this.transform.DOMoveX(0.75f, 0.2f).SetRelative(true).SetEase(Ease.OutExpo).SetLink(gameObject);
                }).SetLink(gameObject);
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
        for (var i = 0; i < Coin; i++)
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
            _ => Color.white
        };
    }
    
    public virtual void Init(EnemyData d, int stage)
    {
        // 敵のデータを設定
        Data = d;
        Stage = stage;
        Magnification = (stage * 0.8f)+1;
        MaxHealth = (int)(GameManager.Instance.RandomRange(d.maxHealthMin, d.maxHealthMax) * Magnification);
        Health = MaxHealth;
        Attack = (int)(d.attack * (Magnification * 0.2f));
        Exp = d.exp + (int)(Magnification);
        Coin = d.coin + (int)(Magnification);
        ActionInterval = d.interval;
        
        // スプライトアニメーションを設定
        this.GetComponent<SpriteSheetAnimator>().Setup(d.sprites, d.framePerSecond);
        this.transform.Translate(0, d.enemyYOffset, 0);

        // UIの初期化
        var c = UIManager.Instance.GetUICamera();
        var hpSlider = EnemyContainer.Instance.GetHpSliderPrefab();
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
        _statusEffectUI = slider.GetComponentInChildren<StatusEffectUI>();
        _healthSlider.maxValue = MaxHealth;
        _healthSlider.value = Health;
        _healthText.text = Health + "/" + MaxHealth;
        _attackCountText.text = (ActionInterval - TurnCount).ToString();
        
        // 通常攻撃の設定
        _nextAction = GetNextAction();
        UpdateAttackIcon(_nextAction);
        
        OnAppear();
    }
}
