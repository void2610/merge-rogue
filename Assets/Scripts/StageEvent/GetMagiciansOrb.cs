using System.Collections.Generic;
using UnityEngine;

public class GetMagiciansOrb : StageEventBase
{
    public override void Init()
    {
        EventName = "GetMagiciansOrb";
        MainDescription = "魔術師と出会った。\n「ワシではこれを使いこなせないみたいだ...\nこれを貰ってくれんか？」";
        Options = new List<OptionData>
        {
            new OptionData
            {
                description = "受け取る",
                resultDescription = "魔術師のオーブを手に入れた",
                Action = () =>
                {
                    var ball = ContentProvider.Instance.GetBallDataFromClassName("MagiciansOrb");
                    InventoryManager.Instance.AddBall(ball);
                }
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
