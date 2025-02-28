using System.Collections.Generic;
using UnityEngine;

public class RingShop : StageEventBase
{
    private int _count = 1;
    private int _price = 15;
    public override void Init()
    {
        EventName = "RingShop";
        MainDescription = "指輪売りの少女と出会った。\n「指輪、指輪はいりませんかー？」";
        Options = new List<OptionData>
        {
            new OptionData
            {
                isEndless = true,
                description = "買う",
                resultDescription = "「お買い上げありがとうございます！」\n指輪を手に入れた",
                Action = () =>
                {
                    var rings = new List<RelicData>();
                    rings.Add(ContentProvider.Instance.GetRelicByClassName("FireRing"));
                    rings.Add(ContentProvider.Instance.GetRelicByClassName("IceRing"));
                    rings.Add(ContentProvider.Instance.GetRelicByClassName("ShockRing"));
                    rings.Print();
                    
                    var idx = GameManager.Instance.RandomRange(0, rings.Count);
                    RelicManager.Instance.AddRelic(rings[idx]);
                    GameManager.Instance.SubCoin(_price);
                    _count++;
                    _price += 15;
                },
                IsAvailable = () => GameManager.Instance.Coin.Value >= _price
            },
            new OptionData
            {
                description = "立ち去る",
                resultDescription = "...",
                Action = () => { }
            }
        };
    }
}
