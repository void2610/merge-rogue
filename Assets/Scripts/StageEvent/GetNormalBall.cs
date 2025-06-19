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
                    InventoryService.AddBall(ball);
                },
                IsAvailable = () => InventoryService.IsFull == false
            },
            new OptionData
            {
                description = "ランダムなボールを捨てる",
                resultDescription = "(ボールを1つ捨てた)",
                Action = () =>
                {
                    var idx = RandomService.RandomRange(0, InventoryService.InventorySize);
                    InventoryService.RemoveAndShiftBall(idx);
                },
                IsAvailable = () => InventoryService.IsFull == false
            },
        };
    }
}
