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
                    // ContentServiceが利用可能な場合はそれを使用、そうでなければフォールバック
                    BallData ball = null;
                    var lifetimeScope = VContainer.Unity.LifetimeScope.Find<VContainer.Unity.LifetimeScope>();
                    if (lifetimeScope != null && lifetimeScope.Container.TryResolve(typeof(IContentService), out var service))
                    {
                        var contentService = service as IContentService;
                        ball = contentService?.GetBallDataFromClassName("MagiciansOrb");
                    }
                    else
                    {
                        throw new System.Exception("ContentService not found in the current scope");
                    }
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
