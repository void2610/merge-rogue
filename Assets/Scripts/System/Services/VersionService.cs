public class VersionService : IVersionService
{
    private readonly string configuredVersion;
    
    public VersionService(string version = "0.0.0")
    {
        configuredVersion = string.IsNullOrEmpty(version) ? "0.0.0" : version;
    }
    
    /// <summary>
    /// バージョンテキストを取得します。引数が指定されない場合は設定されたバージョンを使用します。
    /// </summary>
    /// <param name="version">使用するバージョン文字列（nullの場合は設定値を使用）</param>
    /// <returns>フォーマットされたバージョンテキスト</returns>
    public string GetVersionText(string version = null)
    {
        var versionToUse = string.IsNullOrEmpty(version) ? configuredVersion : version;
        return GetFormattedVersionText(versionToUse);
    }

    /// <summary>
    /// バージョン文字列をフォーマットします。
    /// </summary>
    /// <param name="version">バージョン文字列</param>
    /// <returns>フォーマットされたバージョンテキスト</returns>
    public string GetFormattedVersionText(string version)
    {
        var text = $"Ver.{version}";
        
        #if DEMO_PLAY
        text += " (demo)";
        #endif
        
        return text;
    }
}