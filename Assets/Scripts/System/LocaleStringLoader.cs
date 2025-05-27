using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;
using UnityEngine.Localization.Tables;

public class LocalizeStringLoader : SingletonMonoBehaviour<LocalizeStringLoader>
{
    private readonly Dictionary<string, string> _cache = new();
    
    /// <summary>キャッシュからキーに対応する文字列を返す。見つからなければ [key] を返す。</summary>
    public string Get(string key) => _cache.TryGetValue(key, out var val) ? val : $"[{key}]";
    
    /// <summary>
    /// 非同期でLocalizationTableを取得する
    /// </summary>
    private static async UniTask<StringTable> GetLocalizationTable(LocalizationTableType tableType)
    {
        var task = tableType switch
        {
            LocalizationTableType.UI => LocalizationSettings.StringDatabase.GetTableAsync("UI"),
            LocalizationTableType.Ball => LocalizationSettings.StringDatabase.GetTableAsync("Ball"),
            LocalizationTableType.Relic => LocalizationSettings.StringDatabase.GetTableAsync("Relic"),
            _ => throw new System.ArgumentOutOfRangeException(nameof(tableType), $"Unknown LocalizationTableType: {tableType}")
        };
        return await task;
    }
    
    private async UniTask PreloadAsync()
    {
        _cache.Clear();

        // 必要なテーブルを全部読む
        await AddTable(LocalizationTableType.UI);
        await AddTable(LocalizationTableType.Ball);
        await AddTable(LocalizationTableType.Relic);
    }

    private async UniTask AddTable(LocalizationTableType tableType)
    {
        var t = await GetLocalizationTable(tableType);
        foreach (var entry in t.Values)
        {
            _cache[entry.Key] = entry.GetLocalizedString();
        }
    }

    private void Start()
    {
        DontDestroyOnLoad(this.gameObject);
        // ロケールが変更されたらキャッシュを作り直す
        LocalizationSettings.SelectedLocaleChanged += _ => PreloadAsync().Forget();
        PreloadAsync().Forget();
    }
}