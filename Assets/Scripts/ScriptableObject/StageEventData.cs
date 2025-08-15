using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ステージイベントのデータを定義するScriptableObject
/// </summary>
[CreateAssetMenu(fileName = "StageEventData", menuName = "ScriptableObjects/StageEventData")]
public class StageEventData : ScriptableObject
{
    [Serializable]
    public class EventOptionData
    {
        /// <summary>
        /// 実行するアクションリスト（SerializeReference対応）
        /// </summary>
        [SerializeReference, SubclassSelector]
        public List<StageEventActionBase> actions = new List<StageEventActionBase>();
        
        /// <summary>
        /// エンドレスオプションかどうか（イベント終了しない）
        /// </summary>
        public bool isEndless;
        
        /// <summary>
        /// オプションの説明文を取得
        /// ローカライゼーションキー: {eventId}_{index}_D (indexは1から開始)
        /// </summary>
        public string GetDescriptionKey(string eventId, int arrayIndex) => $"{eventId}_{arrayIndex + 1}_D";
        
        /// <summary>
        /// オプションの効果説明文を取得
        /// ローカライゼーションキー: {eventId}_{index}_E (indexは1から開始)
        /// </summary>
        public string GetEffectDescriptionKey(string eventId, int arrayIndex) => $"{eventId}_{arrayIndex + 1}_E";
        
        /// <summary>
        /// 結果の説明文を取得
        /// ローカライゼーションキー: {eventId}_{index}_R (indexは1から開始)
        /// </summary>
        public string GetResultDescriptionKey(string eventId, int arrayIndex) => $"{eventId}_{arrayIndex + 1}_R";
    }
    
    [Header("基本情報")]
    /// <summary>
    /// イベントのID（ローカライゼーションキーのベースとしても使用）
    /// </summary>
    public string eventId;
    
    /// <summary>
    /// イベントの発生重み（重み付き確率計算で使用）
    /// 値が大きいほど発生しやすい
    /// </summary>
    public int weight = 1;
    
    
    [Header("選択肢")]
    /// <summary>
    /// イベントの選択肢
    /// </summary>
    public List<EventOptionData> options = new List<EventOptionData>();
    
    /// <summary>
    /// メイン説明文を取得
    /// ローカライゼーションキー: {eventId}_D
    /// </summary>
    public string GetMainDescriptionKey() => $"{eventId}_D";
}