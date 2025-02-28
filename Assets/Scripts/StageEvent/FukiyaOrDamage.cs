using System.Collections.Generic;
using UnityEngine;

public class FukiyaOrDamage : StageEventBase
{
    public override void Init()
    {
        EventName = "FukiyaOrDamage";
        MainDescription = "前方から吹矢が飛んできた！\nどうする？";
        Options = new List<OptionData>
        {
            new OptionData
            {
                description = "バッグで受け止める",
                resultDescription = "吹矢を受け止めた！\n(レリックを獲得した)",
                Action = () =>
                {
                    var r = ContentProvider.Instance.GetRelicByClassName("Fukiya");
                    RelicManager.Instance.AddRelic(r);
                }
            },
            new OptionData
            {
                description = "体で受け止める",
                resultDescription = "膝に矢を受けてしまった...!\n(20ダメージ!)",
                Action = () =>
                {
                    GameManager.Instance.Player.Damage(20);
                }
            }
        };
    }
}
