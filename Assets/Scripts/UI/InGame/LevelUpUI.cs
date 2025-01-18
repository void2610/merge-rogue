using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelUpUI : MonoBehaviour
{
    private enum LevelUpType
    {
        Attack,
        Width,
        Ball
    }

    [SerializeField] private LevelUpType type;
    [SerializeField] private GameObject gaugePrefab;
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private Sprite gaugeSprite;
    [SerializeField] private Sprite fillGaugeSprite;
    [SerializeField] private Button button;
    [SerializeField] private Vector2 offset;
    [SerializeField] private float align;
    [SerializeField] private int maxLevel;
    private readonly List<Image> _gaugeList = new List<Image>();
    private int _level = 0;

    private void Awake()
    {
        title.text = type.ToString();
        for (var i = 0; i < maxLevel; i++)
        {
            var gauge = Instantiate(gaugePrefab, this.transform).GetComponent<Image>();
            gauge.sprite = gaugeSprite;
            gauge.transform.localScale = Vector3.one * 0.75f;
            gauge.transform.SetParent(this.transform);
            gauge.rectTransform.anchoredPosition = new Vector2(align * i, 0) + offset;
            _gaugeList.Add(gauge);
        }
        
        button.onClick.AddListener(LevelUp);
    }

    private void LevelUp() => LevelUpAsync().Forget();
    
    private async UniTaskVoid LevelUpAsync()
    {
        if (_level >= maxLevel) return;
        _level++;
        _gaugeList[_level - 1].sprite = fillGaugeSprite;
        switch (type)
        {
            case LevelUpType.Attack:
                MergeManager.Instance.LevelUpAttack();
                break;
            case LevelUpType.Width:
                MergeManager.Instance.LevelUpWallWidth();
                break;
            case LevelUpType.Ball:
                MergeManager.Instance.LevelUpBallAmount();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        if (_level >= maxLevel)
        {
            button.interactable = false;
            button.gameObject.SetActive(false);
        }
        
        UIManager.Instance.remainingLevelUps--;
        if (UIManager.Instance.remainingLevelUps > 0) return;
        SeManager.Instance.PlaySe("levelUp");
        CameraMove.Instance.ShakeCamera(0.5f, 0.3f);
        
        await UniTask.Delay(1000);
        
        UIManager.Instance.EnableCanvasGroup("LevelUp", false);
        GameManager.Instance.ChangeState(GameManager.GameState.MapSelect);
    }
}
