using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class EnemyBase : MonoBehaviour, IEntity
{
    public string enemyName = "Enemy";
    public int actionInterval = 1;
    public int hMax = 100;
    public int hMin = 1;
    public int attack = 2;
    public int coin;
    public int exp;

    [SerializeField]
    private GameObject canvas;
    [SerializeField]
    private GameObject coinPrefab;

    public int Health { get; protected set; }
    public int MaxHealth { get; protected set; }
    public List<StatusEffectBase> StatusEffects { get; } = new();

    protected int TurnCount = 0;

    private TextMeshProUGUI HealthText => canvas.transform.Find("HPText").GetComponent<TextMeshProUGUI>();
    private Slider HealthSlider => canvas.transform.Find("HPSlider").GetComponent<Slider>();
    private TextMeshProUGUI AttackCountText => canvas.transform.Find("AttackCount").GetComponent<TextMeshProUGUI>();
    private StatusEffectUI StatusEffectUI => canvas.transform.Find("StatusEffectUI").GetComponent<StatusEffectUI>();
    
    public void AddStatusEffect(StatusEffectBase effect)
    {
        var existingEffect = StatusEffects.Find(e => e.Type == effect.Type);
        if (existingEffect != null)
            existingEffect.AddStack(effect.StackCount);
        else
            StatusEffects.Add(effect);
        
        StatusEffectUI.UpdateUI(StatusEffects);
    }
    
    public void UpdateStatusEffects()
    {
        for (var i = StatusEffects.Count - 1; i >= 0; i--)
        {
            StatusEffects[i].ApplyEffect(this);
            if (StatusEffects[i].ReduceStack())
                StatusEffects.RemoveAt(i);
        }
        
        StatusEffectUI.UpdateUI(StatusEffects);
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

        // ダメージ処理
        Health -= damage;
        HealthSlider.value = Health;
        HealthText.text = Health + "/" + MaxHealth;
        if (Health > 0) return;
        
        Health = 0;
        HealthText.text = Health + "/" + MaxHealth;
        Death();
    }
    
    public void Heal(int healAmount)
    {
        Health += healAmount;
        Health = Mathf.Min(Health, MaxHealth);
        HealthSlider.value = Health;
        HealthText.text = Health + "/" + MaxHealth;
    }

    public void Action()
    {
        TurnCount++;
        if(TurnCount == actionInterval)
        {
            TurnCount = 0;
            Attack();
        }
        else
        {
            this.transform.DOMoveY(0.75f, 0.05f).SetRelative(true).SetEase(Ease.OutQuad).OnComplete(() =>
            {
                this.transform.DOMoveY(-.75f, 0.05f).SetRelative(true).SetEase(Ease.InQuad).SetLink(gameObject);
            }).SetLink(gameObject);
        }

        AttackCountText.text = (actionInterval - TurnCount).ToString();
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
        var cg = canvas.GetComponent<CanvasGroup>();
        cg.DOFade(1, 0.5f).SetLink(gameObject);
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
        var cg = canvas.GetComponent<CanvasGroup>();
        cg.DOFade(0, 0.5f).SetLink(gameObject);

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

    public virtual void Init(float magnification)
    {
        MaxHealth = (int)(GameManager.Instance.RandomRange(hMin, hMax) * magnification);
        Health = MaxHealth;
        attack = (int)(attack * (magnification * 0.3f));

        GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0);
        canvas.GetComponent<CanvasGroup>().alpha = 0;

        HealthSlider.maxValue = MaxHealth;
        HealthSlider.value = Health;
        HealthText.text = Health + "/" + MaxHealth;
        AttackCountText.text = (actionInterval - TurnCount).ToString();
        
        OnAppear();
    }
}
