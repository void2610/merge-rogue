using System;
using System.Collections.Generic;
using R3;
using UnityEngine;
using SafeEventSystem;

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
                    _disposable = SafeEventManager.OnEventStageEnterSimple.Subscribe(RewriteBattleStageToRestStage);
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
    
    private void RewriteBattleStageToRestStage(StageType stageType)
    {
        // TODO: この機能は新しいEventSystemでの実装が必要
        // 現在は一時的にコメントアウト
        // Note: Stage rewriting functionality removed with old EventManager
        _disposable.Dispose();
    }
}
