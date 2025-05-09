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
                description = "取引する。",
                resultDescription = "(25ゴールドを差し出し、HPを999回復した)",
                Action = () =>
                {
                    GameManager.Instance.SubCoin(25);
                    GameManager.Instance.Player.Heal(999);
                },
                IsAvailable = () => GameManager.Instance.Coin.CurrentValue >= 25
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
