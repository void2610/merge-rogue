using System.Collections.Generic;

public class CleaningService : StageEventBase
{
    public override void Init()
    {
        EventName = "CleaningService";
        MainDescription = "「掃除代行サービス、50コインで承っております!」";
        Options = new List<OptionData>
        {
            new OptionData
            {
                description = "お願いします。",
                resultDescription = "「かしこまりました」\n「またのご利用をお待ちしております!」\n(50コイン消費、マージエリアのボールが消滅した)",
                Action = () =>
                {
                    GameManager.Instance.SubCoin(50);
                    MergeManager.Instance.RemoveAllBalls();
                },
                IsAvailable = () => GameManager.Instance.Coin.Value >= 50
            },
            new OptionData
            {
                description = "結構です。",
                resultDescription = "「またのご利用をお待ちしております」",
                Action = () => { }
            }
        };
    }
}
