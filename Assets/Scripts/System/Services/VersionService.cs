public class VersionService : IVersionService
{
    private readonly string _version;
    
    public VersionService(string version)
    {
        _version = version;
    }
    
    public string GetVersionText()
    {
        var text = $"Ver.{_version}";
        
        #if DEMO_PLAY
        text += " (demo)";
        #endif
        
        return text;
    }
}