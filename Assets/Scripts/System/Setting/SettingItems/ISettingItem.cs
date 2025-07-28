using System.Collections.Generic;
using R3;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 設定項目の基本インターフェース
/// 各設定タイプはこのインターフェースを実装する
/// </summary>
public interface ISettingItem
{
    /// <summary>
    /// 設定項目名
    /// </summary>
    string SettingName { get; }
    
    /// <summary>
    /// UIゲームオブジェクト
    /// </summary>
    GameObject GameObject { get; }
    
    /// <summary>
    /// ナビゲーション可能な要素を取得
    /// </summary>
    /// <returns>ナビゲーション可能なSelectable要素のリスト</returns>
    List<Selectable> GetSelectables();
    
    /// <summary>
    /// 設定値を更新（フォーカス維持のため再生成せず値のみ更新）
    /// </summary>
    /// <param name="settingData">新しい設定データ</param>
    void UpdateValue(SettingsView.SettingDisplayData settingData);
    
    /// <summary>
    /// リソースを解放
    /// </summary>
    void Dispose();
}