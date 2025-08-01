using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization.Settings;
using Cysharp.Threading.Tasks;
using UnityEngine.Localization.Tables;
using R3;

public class LocalizeStringLoader : SingletonMonoBehaviour<LocalizeStringLoader>
{
    private readonly Dictionary<string, string> _cache = new();
    private bool _isInitialized;
    private readonly Subject<Unit> _onLocalizationUpdated = new();
    
    /// <summary>初期化が完了しているかどうか</summary>
    public bool IsInitialized => _isInitialized;
    
    /// <summary>ローカライゼーションが更新された時のイベント</summary>
    public Observable<Unit> OnLocalizationUpdated => _onLocalizationUpdated;
    
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
            LocalizationTableType.WordDictionary => LocalizationSettings.StringDatabase.GetTableAsync("WordDictionary"),
            LocalizationTableType.Tutorial => LocalizationSettings.StringDatabase.GetTableAsync("Tutorial"),
            LocalizationTableType.Setting => LocalizationSettings.StringDatabase.GetTableAsync("Setting"),
            _ => throw new System.ArgumentOutOfRangeException(nameof(tableType), $"Unknown LocalizationTableType: {tableType}")
        };
        return await task;
    }
    
    private async UniTask PreloadAsync()
    {
        _isInitialized = false;
        _cache.Clear();

        // 必要なテーブルを全部読む
        await AddTable(LocalizationTableType.UI);
        await AddTable(LocalizationTableType.Ball);
        await AddTable(LocalizationTableType.Relic);
        await AddTable(LocalizationTableType.WordDictionary);
        await AddTable(LocalizationTableType.Tutorial);
        await AddTable(LocalizationTableType.Setting);
        
        _isInitialized = true;
        _onLocalizationUpdated.OnNext(Unit.Default);
    }
    
    /// <summary>
    /// 初期化が完了するまで待機
    /// </summary>
    public async UniTask WaitForInitialization()
    {
        while (!_isInitialized)
        {
            await UniTask.Yield();
        }
    }

    private async UniTask AddTable(LocalizationTableType tableType)
    {
        var t = await GetLocalizationTable(tableType);
        // StringDatabase を介せば WaitForCompletion を回避できる
        var db = LocalizationSettings.StringDatabase;
        foreach (var entry in t.Values)
        {
            // 非同期で評価
            var str = await db.GetLocalizedStringAsync(t.TableCollectionName, entry.Key).Task;
            _cache[entry.Key] = string.IsNullOrEmpty(str) ? $"[{entry.Key}]" : str;
        }
    }

    private void Start()
    {
        DontDestroyOnLoad(this.gameObject);
        // ロケールが変更されたらキャッシュを作り直す
        LocalizationSettings.SelectedLocaleChanged += _ => PreloadAsync().Forget();
        PreloadAsync().Forget();
    }
    
    private void OnDestroy()
    {
        _onLocalizationUpdated?.Dispose();
    }
}