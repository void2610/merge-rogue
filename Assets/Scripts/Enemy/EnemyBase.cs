using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class EnemyBase : MonoBehaviour
{
    protected class AttackData
    {
        public string name;
        public Func<Player, bool> action;
        public float probability;
        public Color color = Color.white;
        public string description;
    }
    public string enemyName = "Enemy";
    public int actionInterval = 1;
    public int hMax = 100;
    public int hMin = 1;
    public int attack = 2;
    public int defense = 1;
    public int coin;
    public int exp;

    [SerializeField]
    private GameObject canvas;
    [SerializeField]
    private GameObject coinPrefab;

    public int health { get; protected set; }
    public int maxHealth { get; protected set; }

    protected int turnCount = 0;
    protected List<AttackData> enemyActions = new List<AttackData>();
    protected AttackData nextAction;

    private TextMeshProUGUI healthText => canvas.transform.Find("HPText").GetComponent<TextMeshProUGUI>();
    private Slider healthSlider => canvas.transform.Find("HPSlider").GetComponent<Slider>();
    private TextMeshProUGUI attackCountText => canvas.transform.Find("AttackCount").GetComponent<TextMeshProUGUI>();

    public void TakeDamage(int damage, bool isEmitEffect = true)
    {
        if(!this) return;
        
        ParticleManager.Instance.DamageText(damage, this.transform.position.x);
        if(isEmitEffect)
            ParticleManager.Instance.HitParticle(this.transform.position + new Vector3(-0.3f, 0.2f, 0));
        
        health -= damage;
        healthSlider.value = health;
        healthText.text = health + "/" + maxHealth;
        if (health <= 0)
        {
            health = 0;
            healthText.text = health + "/" + maxHealth;
            Death();
        }
    }

    public void Action()
    {
        turnCount++;
        if(turnCount == actionInterval)
        {
            turnCount = 0;
            Attack();
        }
        else
        {
            this.transform.DOMoveY(0.75f, 0.05f).SetRelative(true).SetEase(Ease.OutQuad).OnComplete(() =>
            {
                this.transform.DOMoveY(-.75f, 0.05f).SetRelative(true).SetEase(Ease.InQuad).SetLink(gameObject);
            }).SetLink(gameObject);
        }

        attackCountText.text = (actionInterval - turnCount).ToString();
    }

    private void Attack()
    {
        GameManager.Instance.player.TakeDamage(Mathf.Max(1, attack));
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
        maxHealth = (int)(GameManager.Instance.RandomRange(hMin, hMax) * magnification);
        health = maxHealth;
        attack = (int)(attack * (magnification * 0.5f));

        GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0);
        canvas.GetComponent<CanvasGroup>().alpha = 0;

        healthSlider.maxValue = maxHealth;
        healthSlider.value = health;
        healthText.text = health + "/" + maxHealth;
        attackCountText.text = (actionInterval - turnCount).ToString();
        
        OnAppear();
    }
}
