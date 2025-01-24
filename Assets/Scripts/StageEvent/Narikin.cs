using System;
using System.Collections.Generic;
using R3;
using UnityEngine;

public class Narikin : StageEventBase
{
    private IDisposable _disposable;
    public override void Init()
    {
        EventName = "Narikin";
        MainDescription = "成金おじさん「何かお困りかね？」";
        Options = new List<OptionData>
        {
            new OptionData
            {
                description = "暗くてよく見えない。",
                resultDescription = "おじさんは懐から取り出した100コインに火をつけた\n「どうだ明るくなったろう」\n(次のイベントマスで戦闘が発生しなくなった!)",
                Action = () =>
                {
                    _disposable = EventManager.OnEventStageEnter.Subscribe(RewriteBattleStageToRestStage);
                    // シーン遷移時にDisposeされることを保証する
                    GameManager.Instance.SceneDisposables.Add(_disposable);
                }
            },
            new OptionData
            {
                description = "お金が欲しい。",
                resultDescription = "おじさんは懐から取り出した100コインを渡してくれた\n\u3000\u3000「すこしだけだが受け取りたまえ」\n(100コインを獲得した)",
                Action = () =>
                {
                    GameManager.Instance.AddCoin(100);
                }
            }
        };
    }
    
    private void RewriteBattleStageToRestStage(Unit _)
    {
        // 休憩ステージに変更して購読解除
        EventManager.OnEventStageEnter.SetValue(StageType.Rest);
        _disposable.Dispose();
    }
}
