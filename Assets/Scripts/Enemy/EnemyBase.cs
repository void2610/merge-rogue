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

    private int health { get; set; }
    private int maxHealth { get; set; }

    protected int turnCount = 0;
    protected List<AttackData> enemyActions = new List<AttackData>();
    protected AttackData nextAction;

    private TextMeshProUGUI healthText => canvas.transform.Find("HPText").GetComponent<TextMeshProUGUI>();
    private Slider healthSlider => canvas.transform.Find("HPSlider").GetComponent<Slider>();
    private TextMeshProUGUI attackCountText => canvas.transform.Find("AttackCount").GetComponent<TextMeshProUGUI>();

    public void TakeDamage(int damage)
    {
        GameManager.Instance.ShowDamage(damage, this.transform.position);
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
            this.transform.DOMoveY(0.75f, 0.15f).SetRelative(true).SetEase(Ease.InQuad).OnComplete(() =>
            {
                this.transform.DOMoveY(-.75f, 0.15f).SetRelative(true).SetEase(Ease.OutQuad).SetLink(gameObject);
            }).SetLink(gameObject);
        }

        attackCountText.text = (actionInterval - turnCount).ToString();
    }

    // 返り値で、攻撃モーションを再生するかどうかを返す
    private void Attack()
    {
        GameManager.Instance.player.TakeDamage(attack);
        this.transform.DOMoveX(-0.75f, 0.02f).SetRelative(true).OnComplete(() =>
                {
                    this.transform.DOMoveX(0.75f, 0.2f).SetRelative(true).SetEase(Ease.OutExpo).SetLink(gameObject);
                }).SetLink(gameObject);
        SeManager.Instance.PlaySe("enemyAttack");
    }

    private void OnAppear()
    {
        CanvasGroup cg = canvas.GetComponent<CanvasGroup>();
        cg.DOFade(1, 0.5f).SetLink(gameObject);
        this.GetComponent<SpriteRenderer>().DOFade(1, 0.5f).SetLink(gameObject);
    }

    public void OnDisappear()
    {
        for (int i = 0; i < coin; i++)
        {
            Utils.Instance.WaitAndInvoke(i * 0.1f, () =>
            {
                Instantiate(coinPrefab, this.transform.position, Quaternion.identity);
            });
        }
        CanvasGroup cg = canvas.GetComponent<CanvasGroup>();
        cg.DOFade(0, 0.5f).SetLink(gameObject);

        this.GetComponent<SpriteRenderer>().DOFade(0, 0.5f).OnComplete(() =>
        {
            Destroy(this.transform.parent.gameObject);
        }).SetLink(gameObject);
    }

    private void Death()
    {
        this.transform.parent.parent.GetComponent<EnemyContainer>().RemoveEnemy(this.gameObject);
    }

    public virtual void Init(float magnification)
    {
        maxHealth = (int)(GameManager.Instance.RandomRange(hMin, hMax) * magnification);
        health = maxHealth;
        attack = (int)(attack * magnification);

        GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0);
        canvas.GetComponent<CanvasGroup>().alpha = 0;

        healthSlider.maxValue = maxHealth;
        healthSlider.value = health;
        healthText.text = health + "/" + maxHealth;
        attackCountText.text = (actionInterval - turnCount).ToString();
        
        OnAppear();
    }
}
