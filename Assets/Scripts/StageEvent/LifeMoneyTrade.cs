using System.Collections.Generic;
using UnityEngine;

public class LifeMoneyTrade : StageEventBase
{
    public override void Init()
    {
        EventName = "LifeMoneyTrade";
        MainDescription = "怪しい男と出会った。\n「取引をしよう。」";
        Options = new List<OptionData>
        {
            new OptionData
            {
                description = "取引する。(50ゴールド消費, HPを999回復)",
                Action = () =>
                {
                    GameManager.Instance.SubCoin(50);
                    GameManager.Instance.Player.Heal(999);
                },
                IsAvailable = () => GameManager.Instance.Coin.Value >= 50
            },
            new OptionData
            {
                description = "立ち去る",
                Action = () => { }
            }
        };
    }
}
