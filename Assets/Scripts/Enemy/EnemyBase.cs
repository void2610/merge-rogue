using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class EnemyBase : MonoBehaviour
{
    public class AttackData
    {
        public string name;
        public Func<Player, bool> action;
        public float probability;
        public Color color = Color.white;
        public string description;
    }
    public string enemyName = "Enemy";
    public float atackSpeed = 1.0f;
    public int hMax = 100;
    public int hMin = 1;
    public int attack = 2;
    public int defense = 1;
    public int gold = 0;
    public int exp = 0;

    [SerializeField]
    private GameObject canvas;
    [SerializeField]
    private GameObject damageTextPrefab;
    [SerializeField]
    private GameObject coinPrefab;

    public int health { get; private set; }
    public int maxHealth { get; private set; }

    protected int turnCount = 0;
    protected List<AttackData> enemyActions = new List<AttackData>();
    protected AttackData nextAction;

    private TextMeshProUGUI healthText => canvas.transform.Find("HPText").GetComponent<TextMeshProUGUI>();
    private Slider healthSlider => canvas.transform.Find("HPSlider").GetComponent<Slider>();
    private float lastAttackTime = 0.0f;

    public void TakeDamage(int damage)
    {
        ShowDamage(damage);
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

    public void Heal(int amount)
    {
        health += amount;
        healthSlider.value = health;
        healthText.text = health + "/" + maxHealth;
        if (health > maxHealth)
        {
            health = maxHealth;
        }
    }

    // 返り値で、攻撃モーションを再生するかどうかを返す
    public void Attack()
    {
        GameManager.instance.player.TakeDamage(attack);
        this.transform.DOMoveX(-0.75f, 0.02f).SetRelative(true).OnComplete(() =>
                {
                    this.transform.DOMoveX(0.75f, 0.2f).SetRelative(true).SetEase(Ease.OutExpo);
                });
        SeManager.instance.PlaySe("enemyAttack");
    }

    public void OnAppear()
    {
        CanvasGroup cg = canvas.GetComponent<CanvasGroup>();
        cg.DOFade(1, 0.5f);
        this.GetComponent<SpriteRenderer>().DOFade(1, 0.5f);
    }

    public void OnDisappear()
    {
        for (int i = 0; i < gold; i++)
        {
            Utils.instance.WaitAndInvoke(i * 0.1f, () =>
            {
                Instantiate(coinPrefab, this.transform.position, Quaternion.identity);
            });
        }
        CanvasGroup cg = canvas.GetComponent<CanvasGroup>();
        cg.DOFade(0, 0.5f);

        this.GetComponent<SpriteRenderer>().DOFade(0, 0.5f).OnComplete(() =>
        {
            Destroy(this.transform.parent.gameObject);
        });
    }

    public void Death()
    {
        this.transform.parent.parent.GetComponent<EnemyContainer>().RemoveEnemy(this.gameObject);
    }

    private void ShowDamage(int damage)
    {
        float r = UnityEngine.Random.Range(-0.5f, 0.5f);
        var g = Instantiate(damageTextPrefab, this.transform.position + new Vector3(r, 0, 0), Quaternion.identity, this.canvas.transform);
        g.GetComponent<TextMeshProUGUI>().text = damage.ToString();

        g.GetComponent<TextMeshProUGUI>().color = Color.red;
        g.GetComponent<TextMeshProUGUI>().DOColor(Color.white, 0.5f);

        g.transform.DOScale(3f, 0.1f).SetEase(Ease.Linear).OnComplete(() =>
        {
            g.transform.DOScale(1.75f, 0.1f).SetEase(Ease.Linear);
        });

        g.transform.DOMoveX(r > 0.0f ? -1.5f : 1.5f, 2f).SetRelative(true).SetEase(Ease.Linear);

        g.transform.DOMoveY(0.75f, 0.75f).SetRelative(true).SetEase(Ease.OutQuad).OnComplete(() =>
        {
            g.GetComponent<TextMeshProUGUI>().DOFade(0, 0.5f);
            g.transform.DOMoveY(-1f, 0.5f).SetRelative(true).SetEase(Ease.InQuad).OnComplete(() => Destroy(g));
        });
    }

    protected virtual void Awake()
    {
        maxHealth = GameManager.instance.RandomRange(hMin, hMax);
        health = maxHealth;

        this.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0);
        canvas.GetComponent<CanvasGroup>().alpha = 0;

        healthSlider.maxValue = maxHealth;
        healthSlider.value = health;
        healthText.text = health + "/" + maxHealth;

        OnAppear();
    }

    protected virtual void Update()
    {
        if (Time.time - lastAttackTime > atackSpeed)
        {
            lastAttackTime = Time.time;
            Attack();
        }
    }
}
