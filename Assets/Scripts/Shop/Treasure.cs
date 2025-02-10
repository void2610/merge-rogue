using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class Treasure : MonoBehaviour
{
    public enum TreasureType
    {
        Normal,
        Initial,
        Boss,
    }
    
    [SerializeField] private List<GameObject> items;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Image fadeImage;
    [SerializeField] private StageManager stageManager;
    [SerializeField] private Vector3 itemPosition;
    [SerializeField] private float itemOffset;
    private const int MAX_ITEMS = 3;
    private readonly Vector3 _disablePosition = new (100, 100, 0);
    private TreasureType _currentType;

    public void OpenTreasure(TreasureType type)
    {
        UIManager.Instance.EnableCanvasGroup("Treasure", true); 
        var y = type == TreasureType.Initial ? 1f : -0.3f;
        this.transform.position = new Vector3(0, y, 0);

        _currentType = type;
        
        descriptionText.text = type switch
        {
            TreasureType.Normal => "宝箱を見つけた！",
            TreasureType.Initial => "「力を与えよう...」",
            TreasureType.Boss => "「ボスを撃破した！」",
            _ => "Treasure"
        };
        
        var count = type switch
        {
            TreasureType.Normal => GameManager.Instance.RandomRange(1, 4),
            TreasureType.Initial => 3,
            TreasureType.Boss => 3,
            _ => 1
        };
        
        if (count is > MAX_ITEMS or <= 0) throw new System.Exception("Invalid count");
        for (var i = 0; i < items.Count; i++)
        {
            if (i < count)
                items[i].transform.localPosition = itemPosition + Vector3.right * (itemOffset * (i - (count - 1) / 2f));
            else
                items[i].transform.position = _disablePosition;
        }
        
        // 同じレアリティのレリックを被りなしでランダムに選ぶ
        var rarity = type switch
        {
            TreasureType.Normal => ContentProvider.Instance.GetRandomRarity(),
            TreasureType.Initial => Rarity.Rare,
            TreasureType.Boss => Rarity.Boss,
            _ => Rarity.Common
        };
        var relics = ContentProvider.Instance.GetRelicDataByRarity(rarity);
        for (var i = 0; i < count; i++)
        {
            var index = GameManager.Instance.RandomRange(0, relics.Count);
            SetEvent(items[i], relics[index]);
            relics.RemoveAt(index);
        }
    }
    
    public void CloseTreasure()
    {
        items.ForEach(item => item.transform.position = _disablePosition);
        UIManager.Instance.EnableCanvasGroup("Treasure", false);

        if (_currentType == TreasureType.Initial)
        {
            stageManager.StartFirstStage();
            fadeImage.DOFade(0, 2f);
        }
        else if(_currentType == TreasureType.Boss)
        {
            EnemyContainer.Instance.EndBattle().Forget();
        }
        else
        {
            GameManager.Instance.ChangeState(GameManager.GameState.MapSelect);
        }
    }
    
    private void SetEvent(GameObject g, RelicData relic)
    {
        Utils.RemoveAllEventFromObject(g);
        var image = g.transform.Find("Icon").GetComponent<Image>();
        image.sprite = relic.sprite;
        var button = g.GetComponent<Button>();
        if (button)
        {
            button.onClick.AddListener(() =>
            {
                if (!relic) return;
                
                RelicManager.Instance.AddRelic(relic);
                SeManager.Instance.PlaySe("coin");
                g.transform.position = _disablePosition;
                EventManager.OnRelicObtainedTreasure.Trigger(relic);
                CloseTreasure();
            });
        }

        Utils.AddEventToObject(g, () => { 
            UIManager.Instance.ShowRelicDescriptionWindow(relic, g);
        }, EventTriggerType.PointerEnter);
    }
    
    private void Awake()
    {
        foreach (var item in items)
        {
            item.transform.position = _disablePosition;
        }
        fadeImage.color = new Color(0, 0, 0, 1);
    }
}