using System;
using System.Linq;
using UnityEngine;

/// <summary>
/// 列挙型の選択肢から選ぶ設定項目
/// 解像度切り替えや品質設定などに使用
/// </summary>
[System.Serializable]
public class EnumSetting : SettingBase<string>
{
    [SerializeField] private string[] options;
    
    /// <summary>
    /// 現在選択されているインデックス
    /// </summary>
    public int CurrentIndex 
    { 
        get => Array.IndexOf(options ?? new string[0], CurrentValue);
        set 
        {
            if (options != null && value >= 0 && value < options.Length)
            {
                CurrentValue = options[value];
            }
        }
    }
    
    /// <summary>
    /// 選択肢の配列
    /// </summary>
    public string[] Options => options ?? new string[0];
    
    /// <summary>
    /// 表示名の配列（未設定の場合はOptionsを使用）
    /// </summary>
    public string[] DisplayNames 
    {
        get
        {
            if (!string.IsNullOrEmpty(localizationKey) && LocalizeStringLoader.Instance != null)
            {
                // ローカライゼーションキーから動的生成
                var localizedNames = new string[options?.Length ?? 0];
                for (int i = 0; i < localizedNames.Length; i++)
                {
                    localizedNames[i] = LocalizeStringLoader.Instance.Get($"{localizationKey}_{i}");
                }
                return localizedNames;
            }
            return options ?? new string[0];
        }
    }
    
    /// <summary>
    /// 現在の表示名
    /// </summary>
    public string CurrentDisplayName 
    {
        get
        {
            var index = CurrentIndex;
            return index >= 0 && index < DisplayNames.Length ? DisplayNames[index] : CurrentValue;
        }
    }
    
    /// <summary>
    /// ローカライゼーションキーベースのコンストラクタ
    /// </summary>
    public EnumSetting(string localizationKey, string[] opts, int defaultIndex = 0) 
        : base(localizationKey, (opts != null && defaultIndex >= 0 && defaultIndex < opts.Length) ? opts[defaultIndex] : (opts?.FirstOrDefault() ?? ""))
    {
        options = opts ?? new string[] { "Option1" };
    }
    
    public EnumSetting()
    {
        // シリアライゼーション用のデフォルトコンストラクタ
        options = new string[] { "Default" };
    }
    
    /// <summary>
    /// 次の選択肢に移動
    /// </summary>
    public void MoveNext()
    {
        if (options != null && options.Length > 0)
        {
            CurrentIndex = (CurrentIndex + 1) % options.Length;
        }
    }
    
    /// <summary>
    /// 前の選択肢に移動
    /// </summary>
    public void MovePrevious()
    {
        if (options != null && options.Length > 0)
        {
            var currentIdx = CurrentIndex;
            CurrentIndex = (currentIdx - 1 + options.Length) % options.Length;
        }
    }
    
    /// <summary>
    /// 指定したenumの値で設定を初期化
    /// </summary>
    public static EnumSetting CreateFromEnum<T>(string localizationKey, T defaultValue) 
        where T : Enum
    {
        var enumValues = Enum.GetValues(typeof(T)).Cast<T>().ToArray();
        var options = enumValues.Select(e => e.ToString()).ToArray();
        var defaultIndex = Array.IndexOf(options, defaultValue.ToString());
        
        return new EnumSetting(localizationKey, options, defaultIndex >= 0 ? defaultIndex : 0);
    }
    
    /// <summary>
    /// 現在の値をenumとして取得
    /// </summary>
    public T GetEnumValue<T>() where T : Enum
    {
        if (Enum.TryParse(typeof(T), CurrentValue, out object result))
        {
            return (T)result;
        }
        return default(T);
    }
    
    public override string GetSettingType()
    {
        return "Enum";
    }
}


/// <summary>
/// よく使用される解像度設定のための専用クラス
/// </summary>
[System.Serializable]
public class ResolutionSetting : EnumSetting
{
    public ResolutionSetting(string localizationKey = "RESOLUTION") 
        : base(localizationKey, GetResolutionOptions(), GetDefaultResolutionIndex())
    {
    }
    
    private static string[] GetResolutionOptions()
    {
        return new string[] 
        {
            "1920x1080",
            "1366x768", 
            "1280x720",
            "1024x768",
            "800x600"
        };
    }
    
    private static string[] GetResolutionDisplayNames()
    {
        return new string[]
        {
            "フルHD (1920×1080)",
            "HD+ (1366×768)",
            "HD (1280×720)", 
            "XGA (1024×768)",
            "SVGA (800×600)"
        };
    }
    
    private static int GetDefaultResolutionIndex()
    {
        // 現在の画面解像度に最も近いものをデフォルトに
        var currentWidth = Screen.currentResolution.width;
        var currentHeight = Screen.currentResolution.height;
        
        if (currentWidth >= 1920 && currentHeight >= 1080) return 0;
        if (currentWidth >= 1366 && currentHeight >= 768) return 1;
        if (currentWidth >= 1280 && currentHeight >= 720) return 2;
        if (currentWidth >= 1024 && currentHeight >= 768) return 3;
        return 4;
    }
    
    /// <summary>
    /// 選択された解像度を適用
    /// </summary>
    public void ApplyResolution()
    {
        var resolution = CurrentValue.Split('x');
        if (resolution.Length == 2 && 
            int.TryParse(resolution[0], out int width) && 
            int.TryParse(resolution[1], out int height))
        {
            Screen.SetResolution(width, height, Screen.fullScreen);
            Debug.Log($"解像度を {width}×{height} に変更しました");
        }
    }
}