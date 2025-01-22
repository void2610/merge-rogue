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
                description = "バッグで受け止める(レリック獲得)",
                Action = () =>
                {
                    //TODO: fukiyaレリック取得処理
                    RelicData relic = null;
                    RelicManager.Instance.AddRelic(relic);
                }
            },
            new OptionData
            {
                description = "体で受け止める(20ダメージ!)",
                Action = () =>
                {
                    GameManager.Instance.Player.Damage(20);
                }
            }
        };
    }
}
