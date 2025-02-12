using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class EnemyBase : MonoBehaviour, IEntity
{
    public enum EnemyType
    {
        Normal,
        Minion,
        MiniBoss,
        Boss,
    }
    
    public enum ActionType
    {
        Attack,
        Heal,
        Buff,
        Debuff,
    }
    [Serializable]
    public class ActionData
    {
        public ActionType type;
        public Action Action;
    }
    
    public string enemyName = "Enemy";
    public EnemyType enemyType;
    public int actionInterval = 1;
    public int hMax = 100;
    public int hMin = 1;
    public int attack = 2;
    public int coin;
    public int exp;

    [SerializeField] private GameObject hpSliderPrefab;
    [SerializeField] private GameObject coinPrefab;

    public int Health { get; protected set; }
    public int MaxHealth { get; protected set; }
    public List<StatusEffectBase> StatusEffects { get; } = new();

    protected int TurnCount = 0;
    protected readonly ActionData NormalAttack = new ();
    private ActionData _nextAction;

    private Canvas _uiCanvas;
    private CanvasGroup _canvasGroup;
    private TextMeshProUGUI _healthText;
    private Slider _healthSlider;
    private TextMeshProUGUI _attackCountText;
    private StatusEffectUI _statusEffectUI;
    private Image _attackIcon;
    
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
    
    public void UpdateStatusEffects()
    {
        for (var i = StatusEffects.Count - 1; i >= 0; i--)
        {
            StatusEffects[i].OnTurnEnd(this);
            if (StatusEffects[i].ReduceStack())
                StatusEffects.RemoveAt(i);
        }
        
        _statusEffectUI.UpdateUI(StatusEffects);
    }
    
    public int ModifyIncomingDamage(int amount)
    {
        return StatusEffects.Aggregate(amount, (current, effect) => effect.ModifyDamage(current));
    }
    
    public void OnBattleEnd()
    {
        foreach (var effect in StatusEffects)
        {
            effect.OnBattleEnd();
        }
        _statusEffectUI.UpdateUI(StatusEffects);
    }
    
    public void Damage(int damage)
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
        if (StatusEffects.Any(e => e.Type == StatusEffectType.Freeze)) return;
        
        TurnCount++;
        if(TurnCount == actionInterval)
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

        _attackCountText.text = (actionInterval - TurnCount).ToString();
    }
    
    protected virtual ActionData GetNextAction()
    {
        return NormalAttack;
    }

    private void Attack()
    {
        GameManager.Instance.Player.Damage(Mathf.Max(1, attack));
        this.transform.DOMoveX(-0.75f, 0.02f).SetRelative(true).OnComplete(() =>
                {
                    this.transform.DOMoveX(0.75f, 0.2f).SetRelative(true).SetEase(Ease.OutExpo).SetLink(gameObject);
                }).SetLink(gameObject);
    }

    private void OnAppear()
    {
        _canvasGroup.DOFade(1, 0.5f).SetLink(gameObject);
        this.GetComponent<SpriteRenderer>().DOFade(1, 0.5f).SetLink(gameObject);
    }

    public void OnDisappear()
    {
        SeManager.Instance.PlaySe("coin");
        for (int i = 0; i < coin; i++)
        {
            // コイン出現中に敵が消えるとエラーが出る
            var c = Instantiate(coinPrefab).GetComponent<Coin>();
            c?.SetUp(this.transform.position.x);
        }
        _canvasGroup.DOFade(0, 0.5f).SetLink(gameObject);

        this.GetComponent<SpriteRenderer>().DOFade(0, 0.5f).OnComplete(() =>
        {
            Destroy(this.transform.parent.gameObject);
        }).SetLink(gameObject);
    }

    private void Death()
    {
        EventManager.OnEnemyDefeated.Trigger(this);
        this.transform.parent.parent.GetComponent<EnemyContainer>().RemoveEnemy(this.gameObject);
    }

    private void UpdateAttackIcon(ActionData a)
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

    public virtual void Init(float magnification)
    {
        _uiCanvas = UIManager.Instance.GetUICanvas();
        var c = UIManager.Instance.GetUICamera();
        var g = Instantiate(hpSliderPrefab, _uiCanvas.transform);
        var pos = c.WorldToScreenPoint(this.transform.position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _uiCanvas.GetComponent<RectTransform>(), pos, c, out Vector2 localPosition
        );
        localPosition.y += 60;
        g.GetComponent<RectTransform>().anchoredPosition = localPosition;
        
        _canvasGroup = g.GetComponent<CanvasGroup>();
        _healthText = g.transform.Find("HPText").GetComponent<TextMeshProUGUI>();
        _healthSlider = g.GetComponent<Slider>();
        _attackCountText = g.transform.Find("AttackCount").GetComponent<TextMeshProUGUI>();
        _attackIcon = g.transform.Find("AttackIcon").GetComponent<Image>();
        _statusEffectUI = g.GetComponentInChildren<StatusEffectUI>();
        
        MaxHealth = (int)(GameManager.Instance.RandomRange(hMin, hMax) * magnification);
        Health = MaxHealth;
        attack = (int)(attack * (magnification * 0.3f));
        exp = exp + (int)(magnification);

        GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0);
        // _canvas.GetComponent<CanvasGroup>().alpha = 0;

        _healthSlider.maxValue = MaxHealth;
        _healthSlider.value = Health;
        _healthText.text = Health + "/" + MaxHealth;
        _attackCountText.text = (actionInterval - TurnCount).ToString();
        
        // 通常攻撃の設定
        NormalAttack.type = ActionType.Attack;
        NormalAttack.Action = Attack;
        _nextAction = GetNextAction();
        UpdateAttackIcon(_nextAction);
        
        OnAppear();
    }
}
