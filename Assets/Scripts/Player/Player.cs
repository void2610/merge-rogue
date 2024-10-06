using UnityEngine;
using TMPro;
using System.Collections.Generic;
using DG.Tweening;
using R3;

public class Player : MonoBehaviour
{
    public float attack = 1.5f;
    public readonly ReactiveProperty<int> exp = new(0);
    public readonly ReactiveProperty<int> health = new(10);
    public readonly ReactiveProperty<int> maxHealth = new(10);
    public List<int> levelUpExp = new() { 10, 20, 40, 80, 160, 320, 640, 1280, 2560, 5120 };
    public int maxExp { get; private set; } = 10;
    public int level { get; private set; } = 1;
    [SerializeField]
    private GameObject canvas;
    [SerializeField]
    private GameObject damageTextPrefab;

    public void TakeDamage(int damage)
    {
        Camera.main?.GetComponent<CameraMove>().ShakeCamera(0.5f, 0.2f);
        ShowDamage(damage);
        health.Value -= damage;
        if (health.Value <= 0)
        {
            health.Value = 0;
            GameManager.Instance.GameOver();
        }
    }

    public void Heal(int amount)
    {
        if (health.Value >= maxHealth.Value)
        {
            return;
        }

        health.Value += amount;
        if (health.Value > maxHealth.Value)
        {
            health.Value = maxHealth.Value;
        }
    }

    public void HealFromItem(int amount)
    {
        health.Value += amount;
        if (health.Value > maxHealth.Value + 5)
        {
            health.Value = maxHealth.Value + 5;
        }
    }

    public void AddExp(int amount)
    {
        exp.Value += amount;
        CheckAndLevelUp();
    }

    private void ShowDamage(int damage)
    {
        var r = Random.Range(-0.5f, 0.5f);
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
        if (exp.Value < levelUpExp[level - 1])
        {
            return false;
        }

        exp.Value -= levelUpExp[level - 1];
        maxExp = levelUpExp[level];
        level++;
        SeManager.Instance.PlaySe("levelUp");
        Time.timeScale = 0.0f;
        GameManager.Instance.uiManager.remainingLevelUps++;
        GameManager.Instance.uiManager.EnableLevelUpOptions(true);

        if (exp.Value >= levelUpExp[level - 1])
        {
            CheckAndLevelUp();
        }
        exp.ForceNotify();
        return true;
    }

    private void Start()
    {
        health.Value = maxHealth.Value;
    }
}
