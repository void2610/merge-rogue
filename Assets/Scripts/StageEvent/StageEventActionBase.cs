using System;
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
    public abstract void Execute();
    
    /// <summary>
    /// アクションが実行可能かチェック
    /// </summary>
    /// <returns>実行可能な場合true</returns>
    public virtual bool CanExecute()
    {
        return true;
    }
}