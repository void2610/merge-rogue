using R3;
using UnityEngine;

public static class EventManager
{
    // コイン獲得時: コイン獲得量
    public static readonly GameEvent<int> OnCoinGain = new (0);
    // プレイヤーの攻撃時: プレイヤーの攻撃力
    public static readonly GameEvent<int> OnPlayerAttack = new (0);
}