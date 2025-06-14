using System;
using R3;
using UnityEngine;
using DG.Tweening;

public class RedBombBall : BallBase
{
    private GameObject _fireParticle;
    private IDisposable _disposable;
    private int _nearbyMergeCount = 0;
    private Vector3 _originalScale;
    
    public override void InitBall(BallData d, int rank, int level = 0)
    {
        isMergable = false; // 通常ではマージ不可
        base.InitBall(d, rank, level);

        _fireParticle = ParticleManager.Instance.GetBombFireParticle();
        _fireParticle.transform.SetParent(this.transform);
        _fireParticle.transform.localPosition = new Vector3(0.2f, 0.8f, 0);
        
        _disposable = EventManager.OnBallMerged.Subscribe(CheckNearMerge).AddTo(this);
    }
    
    private void Start()
    {
        // Tweenアニメーション前に元のスケールを保存
        _originalScale = this.transform.localScale;
    }
    
    protected override void Effect(BallBase other)
    {
        base.Effect(other);
        DefaultMergeParticle();
        // マージさせると全体攻撃できる
        MergeManager.Instance.Attack(AttackType.All, Attack * Rank, this.transform.position);
    }
    
    protected override void TurnEndEffect()
    {
        base.TurnEndEffect();
        
        // ターンが経過するごとに少しずつ大きくなる
        var scaleIncrease = 1f + (elapsedTurns * 0.25f);
        var targetScale = _originalScale * scaleIncrease;
        this.transform.DOScale(targetScale, 0.3f).SetEase(Ease.OutBack).SetLink(gameObject);
        
        if (elapsedTurns < 3) return;
        
        // 周りのボールを消して、プレイヤーにダメージ
        var hitBalls = Utils.GetNearbyBalls(this.gameObject, 0.5f);
        foreach (var ball in hitBalls)
        {
            ball.DestroyWithNoEffect();
        }
        SeManager.Instance.PlaySe("bomb");
        
        // プレイヤーにダメージ
        GameManager.Instance.Player.Damage(AttackType.Normal, (int)(Attack * Rank));
        // 自分を消す
        isDestroyed = true;
        this.DestroyWithNoEffect();
    }
    
    private void CheckNearMerge((BallBase ball1, BallBase ball2) mergeData)
    {
        // 周りのボールがマージされたら回数をカウント
        var (b1, b2) = mergeData;
        var mergePosition = (b1.transform.position + b2.transform.position) / 2f;
        var distance = Vector3.Distance(this.transform.position, mergePosition);
        if (distance < 1f)
        {
            _nearbyMergeCount++;
            ParticleManager.Instance.MergeBallIconParticle(this.transform.position, this.Size, this.Data.sprite);
            if (_nearbyMergeCount >= 3)
            {
                this.EffectAndDestroy(null);
            }
        }
    }
    
    private void OnDestroy()
    {
        _disposable?.Dispose();
    }
}