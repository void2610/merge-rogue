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

    [SerializeField]
    private GameObject canvas;
    [SerializeField]
    private GameObject damageTextPrefab;

    private void UpdateStatusDisplay()
    {
        Slider s = GameManager.Instance.uiManager.hpSlider;
        s.maxValue = maxHealth;
        s.value = health;
        var t = GameManager.Instance.uiManager.hpText;
        t.text = health + "/" + maxHealth;
    
        GameManager.Instance.uiManager.UpdateExpText(exp, levelUpExp[level - 1]);
        GameManager.Instance.uiManager.UpdateLevelText(level);
    }

    public void TakeDamage(int damage)
    {
        Camera.main?.GetComponent<CameraMove>().ShakeCamera(0.5f, 0.2f);
        ShowDamage(damage);
        health -= damage;
        if (health <= 0)
        {
            health = 0;
            GameManager.Instance.GameOver();
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

    public void AddExp(int amount)
    {
        exp += amount;
        CheckAndLevelUp();
        UpdateStatusDisplay();
    }

    public void AddGold(int amount)
    {
        gold += amount;
        if (gold < 0)
        {
            gold = 0;
        }
        GameManager.Instance.uiManager.UpdateCoinText(gold);
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

            });
        });
        Utils.Instance.WaitAndInvoke(5f, () =>
        {
            Destroy(g);
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
        SeManager.Instance.PlaySe("levelUp");
        Time.timeScale = 0.0f;
        GameManager.Instance.uiManager.remainingLevelUps++;
        GameManager.Instance.uiManager.EnableLevelUpOptions(true);

        if (exp >= levelUpExp[level - 1])
        {
            CheckAndLevelUp();
        }

        return true;
    }

    private void Start()
    {
        health = maxHealth;
        UpdateStatusDisplay();

        GameManager.Instance.uiManager.UpdateCoinText(gold);
    }
}
