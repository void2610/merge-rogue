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
    public int health = 100;
    public int maxHealth = 100;
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

    protected int hMax = 100;
    protected int hMin = 1;
    protected int turnCount = 0;
    protected List<AttackData> enemyActions = new List<AttackData>();
    protected AttackData nextAction;

    private TextMeshProUGUI nameText => canvas.transform.Find("NameText").GetComponent<TextMeshProUGUI>();
    private TextMeshProUGUI healthText => canvas.transform.Find("HPText").GetComponent<TextMeshProUGUI>();
    private Slider healthSlider => canvas.transform.Find("HPSlider").GetComponent<Slider>();
    private TextMeshProUGUI attackText => canvas.transform.Find("AttackText").GetComponent<TextMeshProUGUI>();
    private Image attackImage => canvas.transform.Find("AttackIcon").GetComponent<Image>();

    public void TakeDamage(int damage)
    {
        ShowDamage(damage);
        health -= damage;
        if (health <= 1)
        {
            health = 1;
        }
        healthSlider.value = health;
        healthText.text = health + "/" + maxHealth;
    }

    public void TakeDamageFromReturn(int damage)
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
    public bool Attack(Player player)
    {
        bool b = nextAction.action(player);
        DecideNextAction();
        return b;
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
        // ゆらゆらと左右に揺れながら上に登っていく
        this.transform.DOPunchPosition(new Vector3(0.5f, 0, 0), 2f, 3, 1f).SetRelative(true);
        this.transform.DOMoveY(1, 2f).SetRelative(true);
        this.GetComponent<SpriteRenderer>().DOFade(0, 2f).OnComplete(() =>
        {
            Destroy(this.transform.parent.gameObject);
        });
    }

    protected virtual void DecideNextAction()
    {
        float sum = 0;
        foreach (var a in enemyActions)
        {
            sum += a.probability;
        }
        float random = GameManager.instance.RandomRange(0.0f, sum);
        foreach (var a in enemyActions)
        {
            random -= a.probability;
            if (random <= 0)
            {
                nextAction = a;
                break;
            }
        }

        UpadateActionIcon();
    }

    protected void UpadateActionIcon()
    {
        attackText.text = nextAction.name;
        attackImage.color = nextAction.color;
        attackImage.GetComponent<OverRayWindow>().text = nextAction.description;
    }

    protected virtual bool NormalAttack(Player player)
    {
        if (player.isSaving) player.AddSaveFromEnemy(attack);
        else player.TakeDamage(attack);
        return true;
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
        nameText.text = enemyName;
        healthSlider.maxValue = maxHealth;
        healthSlider.value = health;
        healthText.text = health + "/" + maxHealth;

        enemyActions.Add(new AttackData { name = "こうげき", action = NormalAttack, probability = 0.8f, color = Color.red, description = "いたい！\n"+attack+"ダメージ" });

        DecideNextAction();
        OnAppear();
    }
}
