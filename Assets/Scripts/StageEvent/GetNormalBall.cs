using System.Collections.Generic;
using UnityEngine;

public class GetNormalBall : StageEventBase
{
    public override void Init()
    {
        EventName = "GetNormalBall";
        MainDescription = "ただのボールがたくさん転がっている！\nどうする？";
        Options = new List<OptionData>
        {
            new OptionData
            {
                description = "バッグに加える",
                resultDescription = "(ただのボールを手に入れた)",
                Action = () =>
                {
                    var ball = ContentService.GetNormalBallData();
                    InventoryManager.Instance.AddBall(ball);
                },
                IsAvailable = () => InventoryManager.Instance.IsFull == false
            },
            new OptionData
            {
                description = "ランダムなボールを捨てる",
                resultDescription = "(ボールを1つ捨てた)",
                Action = () =>
                {
                    var idx = GameManager.Instance.RandomRange(0, InventoryManager.Instance.InventorySize);
                    InventoryManager.Instance.RemoveAndShiftBall(idx);
                },
                IsAvailable = () => InventoryManager.Instance.IsFull == false
            },
        };
    }
}
