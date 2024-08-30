using UnityEngine;
using System.Collections.Generic;

public class BallCreater : MonoBehaviour
{
    [SerializeField]
    private GameObject ballPrefab;
    // ボールのレアリティを定義するenum
    public enum BallRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }

    // レアリティごとの色を定義する辞書
    private static readonly Dictionary<BallRarity, Color> RarityColors = new Dictionary<BallRarity, Color>
    {
        { BallRarity.Common, Color.gray },
        { BallRarity.Uncommon, Color.green },
        { BallRarity.Rare, Color.blue },
        { BallRarity.Epic, new Color(0.5f, 0f, 0.5f) },
        { BallRarity.Legendary, new Color(1f, 0.5f, 0f) }
    };

    // レアリティごとの攻撃力範囲を定義する辞書
    private static readonly Dictionary<BallRarity, Vector2Int> AttackRanges = new Dictionary<BallRarity, Vector2Int>
    {
        { BallRarity.Common, new Vector2Int(1, 5) },
        { BallRarity.Uncommon, new Vector2Int(3, 8) },
        { BallRarity.Rare, new Vector2Int(6, 12) },
        { BallRarity.Epic, new Vector2Int(10, 18) },
        { BallRarity.Legendary, new Vector2Int(15, 25) }
    };

    // ボールを作成するメソッド
    public void CreateBall(BallRarity rarity)
    {
        GameObject ballObject = Instantiate(ballPrefab, InventoryManager.instance.transform);
        BallBase b = ballObject.GetComponent<BallBase>();
        Vector2Int attackRange = AttackRanges[rarity];

        // 攻撃力をランダムに設定
        int a = GameManager.instance.RandomRange(attackRange.x, attackRange.y + 1);

        b.color = new Color(GameManager.instance.RandomRange(0, 256) / 255f, GameManager.instance.RandomRange(0, 256) / 255f, GameManager.instance.RandomRange(0, 256) / 255f);
        b.attack = a;
        b.size = GameManager.instance.RandomRange(0.1f, 3f);
        b.level = GameManager.instance.RandomRange(1, 6);
        b.probability = 0.1f;
        InventoryManager.instance.AddBall(ballObject);
    }

    // レアリティに応じた色を取得するメソッド
    public static Color GetRarityColor(BallRarity rarity)
    {
        return RarityColors[rarity];
    }

    // レアリティに応じた攻撃力範囲を取得するメソッド
    public static Vector2Int GetAttackRange(BallRarity rarity)
    {
        return AttackRanges[rarity];
    }
}
