using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// ステージイベントアクションのベースクラス
/// </summary>
[Serializable]
public abstract class StageEventActionBase
{
    /// <summary>
    /// アクションを実行
    /// </summary>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>実行結果</returns>
    public abstract UniTask Execute(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// アクションが実行可能かチェック
    /// </summary>
    /// <returns>実行可能な場合true</returns>
    public virtual bool CanExecute()
    {
        return true;
    }
}