using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class Player : MonoBehaviour
{
    public int health = 100;
    public int maxHealth = 100;
    public float attack = 1.5f;
    public int gold = 0;
    public int exp = 0;
    public List<int> levelUpExp = new List<int> { 10, 20, 40, 80, 160, 320, 640, 1280, 2560, 5120 };
    public int level = 1;
    public int maxSave = 5;
    public int save = 0;

    public bool isSaving { get; private set; } = false;

    [SerializeField]
    private GameObject canvas;
    [SerializeField]
    private GameObject damageTextPrefab;

    private TextMeshProUGUI healthText => canvas.transform.Find("HPText").GetComponent<TextMeshProUGUI>();
    private Slider healthSlider => canvas.transform.Find("HPSlider").GetComponent<Slider>();

    public void UpdateStatusDisplay()
    {
        healthSlider.maxValue = maxHealth;
        healthSlider.value = health;
        healthText.text = health + "/" + maxHealth;
    }

    public void TakeDamage(int damage)
    {
        Camera.main.GetComponent<CameraMove>().ShakeCamera(0.5f, 0.3f);
        ShowDamage(damage);
        health -= damage;
        if (health <= 0)
        {
            health = 0;
        }
        UpdateStatusDisplay();
    }

    public void AddSaveFromEnemy(int amount)
    {
        if (save + amount > maxSave)
        {
            TakeDamage(save + amount);
            save = 0;
        }
        else if (save + amount < 0)
        {
            save = 0;
        }
        else
        {
            save += amount;
        }
        UpdateStatusDisplay();
    }

    public void AddSaveFromItem(int amount)
    {
        save += amount;
        if (save > maxSave + 5)
        {
            save = maxSave + 5;
        }
        UpdateStatusDisplay();
    }

    public void Heal(int amount)
    {
        if (health >= maxHealth)
        {
            return;
        }

        health += amount;
        if (health > maxHealth)
        {
            health = maxHealth;
        }
        UpdateStatusDisplay();
    }

    public void HealFromItem(int amount)
    {
        health += amount;
        if (health > maxHealth + 5)
        {
            health = maxHealth + 5;
        }
        UpdateStatusDisplay();
    }

    public void AddGold(int amount)
    {
        gold += amount;
        if (gold < 0)
        {
            gold = 0;
        }
        GameManager.instance.uiManager.UpdateCoinText(gold);
    }

    public void AddExp(int amount)
    {
        exp += amount;
        GameManager.instance.uiManager.UpdateExpText(exp, levelUpExp[level - 1]);
    }

    private void ShowDamage(int damage)
    {
        float r = UnityEngine.Random.Range(-0.5f, 0.5f);
        var g = Instantiate(damageTextPrefab, this.transform.position + new Vector3(r, 0, 0), Quaternion.identity, this.canvas.transform);
        g.GetComponent<TextMeshProUGUI>().text = damage.ToString();

        g.GetComponent<TextMeshProUGUI>().color = new Color(1, 0, 0);
        g.GetComponent<TextMeshProUGUI>().DOColor(new Color(1, 1, 1), 0.5f);
        g.transform.DOScale(3f, 0.1f).SetEase(Ease.Linear).OnComplete(() =>
        {
            g.transform.DOScale(1.75f, 0.1f).SetEase(Ease.Linear);
        });

        if (r > 0.0f)
            g.transform.DOMoveX(-1.5f, 2f).SetRelative(true).SetEase(Ease.Linear);
        else
            g.transform.DOMoveX(1.5f, 2f).SetRelative(true).SetEase(Ease.Linear);

        g.transform.DOMoveY(0.75f, 0.75f).SetRelative(true).SetEase(Ease.OutQuad).OnComplete(() =>
        {
            g.GetComponent<TextMeshProUGUI>().DOFade(0, 0.5f);
            g.transform.DOMoveY(-1f, 0.5f).SetRelative(true).SetEase(Ease.InQuad).OnComplete(() =>
            {
                Destroy(g);
            });
        });
    }

    private bool CheckAndLevelUp()
    {
        if (exp < levelUpExp[level - 1])
        {
            return false;
        }

        exp -= levelUpExp[level - 1];
        level++;
        SeManager.instance.PlaySe("levelUp");
        GameManager.instance.uiManager.UpdateExpText(exp, levelUpExp[level - 1]);
        GameManager.instance.uiManager.UpdateLevelText(level);
        GameManager.instance.uiManager.remainingLevelUps++;
        GameManager.instance.ChangeState(GameManager.GameState.LevelUp);

        if (exp >= levelUpExp[level - 1])
        {
            CheckAndLevelUp();
        }

        return true;
    }

    public void Attack(List<EnemyBase> enemies)
    {
        GameManager.instance.ChangeState(GameManager.GameState.PlayerAttack);

        Utils.instance.WaitAndInvoke(0.75f, () =>
        {
            SeManager.instance.PlaySe("enemyAttack");
            int a = Mathf.FloorToInt((float)attack);
            foreach (EnemyBase enemy in enemies)
            {
                enemy.TakeDamage(a);
                Heal(1);
            }
            Camera.main.GetComponent<CameraMove>().ShakeCamera(0.5f, 0.3f);
            UpdateStatusDisplay();
            Utils.instance.WaitAndInvoke(1f, () =>
            {
                if (CheckAndLevelUp())
                {
                    GameManager.instance.ChangeState(GameManager.GameState.LevelUp);
                }
                else
                {
                    GameManager.instance.ChangeState(GameManager.GameState.EnemyAttack);
                }
            });
        });
    }

    public void Return(List<EnemyBase> enemies)
    {
        GameManager.instance.ChangeState(GameManager.GameState.PlayerAttack);
        if (save <= 0)
        {
            GameManager.instance.ChangeState(GameManager.GameState.EnemyAttack);
            return;
        }

        Utils.instance.WaitAndInvoke(1f, () =>
        {
            SeManager.instance.PlaySe("enemyAttack");
            int a = Mathf.FloorToInt(attack * (float)save);
            foreach (EnemyBase enemy in enemies)
            {
                enemy.TakeDamageFromReturn(a);
            }
            Camera.main.GetComponent<CameraMove>().ShakeCamera(0.5f, 0.3f);
            save = 0;
            UpdateStatusDisplay();
            Utils.instance.WaitAndInvoke(1, () =>
            {
                if (CheckAndLevelUp())
                {
                    GameManager.instance.ChangeState(GameManager.GameState.LevelUp);
                }
                else
                {
                    GameManager.instance.ChangeState(GameManager.GameState.EnemyAttack);
                }
            });
        });
    }

    public void EnableSave(bool enable)
    {
        if (enable) Save();
        else isSaving = false;
    }

    private void Save()
    {
        GameManager.instance.ChangeState(GameManager.GameState.PlayerAttack);
        isSaving = true;
        GameManager.instance.ChangeState(GameManager.GameState.EnemyAttack);
    }

    public void GrowAttack()
    {
        attack += 0.5f;
        GameManager.instance.uiManager.UpdateAttackText(attack);
        UpdateStatusDisplay();
    }

    public void GrowSave()
    {
        maxSave++;
        UpdateStatusDisplay();
    }

    public void GrowHp()
    {
        maxHealth += 5;
        health += 5;
        UpdateStatusDisplay();
    }

    void Start()
    {
        health = maxHealth;
        UpdateStatusDisplay();

        GameManager.instance.uiManager.UpdateCoinText(gold);
        GameManager.instance.uiManager.UpdateExpText(exp, levelUpExp[level - 1]);
        GameManager.instance.uiManager.UpdateLevelText(level);
        GameManager.instance.uiManager.UpdateAttackText(attack);
    }
}
