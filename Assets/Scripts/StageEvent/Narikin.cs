using System.Collections.Generic;

public class Narikin : StageEventBase
{
    private bool _isProcessorRegistered;
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
                    // ValueProcessorにEnemy→Restの変換を一度だけ実行する処理を登録
                    var oneTimeUse = false;
                    EventManager.OnStageTypeDecision.AddProcessor(this, stage =>
                    {
                        if (!oneTimeUse && stage == StageType.Enemy)
                        {
                            oneTimeUse = true;
                            return StageType.Rest;
                        }
                        return stage;
                    });
                    
                    _isProcessorRegistered = true;
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
    
    private void OnDestroy()
    {
        if (_isProcessorRegistered)
        {
            EventManager.OnStageTypeDecision.RemoveProcessorsFor(this);
        }
    }
}
