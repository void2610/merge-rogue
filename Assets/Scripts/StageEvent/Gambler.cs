using System;
using System.Collections.Generic;
using R3;
using UnityEngine;

public class Gambler : StageEventBase
{
    private const string GAMBLER_COIN_KEY = "gamblerCoin";
    
    public override void Init()
    {
        // 投資する選択肢
        var investOption = new OptionData
        {
            description = "投資する",
            resultDescription = "ありがとよ！(コインを全て渡した)",
            Action = () =>
            {
                var coin = (int)GameManager.Instance.Coin.Value;
                if(Register.TryGetInt(GAMBLER_COIN_KEY, out var gamblerCoin)) Register.RegisterInt(GAMBLER_COIN_KEY, gamblerCoin + coin);
                else Register.RegisterInt(GAMBLER_COIN_KEY, coin);
                GameManager.Instance.SubCoin(coin);
            },
            IsAvailable = () => GameManager.Instance.Coin.Value > 0
        };
        
        EventName = "Gambler";
        if (Register.GetInt(GAMBLER_COIN_KEY) == null)
        {
            MainDescription = "ボロボロの男に出会った。\n「俺に投資してみないか...?」";
            Options = new List<OptionData>
            {
                investOption,
                new OptionData
                {
                    description = "立ち去る",
                    resultDescription = "...",
                    Action = () => { }
                }
            };
        }
        else
        {
            MainDescription = "ボロボロの男に出会った。\n「おお、お前だな...」";
            var r = RandomService.RandomRange(0f, 1f) < 0.5f;
            if(r){
                Options = new List<OptionData>
                {
                    new OptionData
                    {
                        description = "返してもらう",
                        resultDescription = "「お前のおかげで稼げたよ！」(コインを全て受け取った)",
                        Action = () =>
                        {
                            var coin = Register.GetInt(GAMBLER_COIN_KEY).Value;
                            GameManager.Instance.AddCoin(coin * 2);
                            Register.RemoveInt(GAMBLER_COIN_KEY);
                        }
                    },
                    investOption
                }; 
            }
            else
            {
                Options = new List<OptionData>
                {
                    new OptionData
                    {
                        description = "返してもらう",
                        resultDescription = "「すまん、失敗した...」",
                        Action = () =>
                        {
                            Register.RemoveInt(GAMBLER_COIN_KEY);
                        }
                    },
                    investOption
                }; 
            }
        }
    }
}
