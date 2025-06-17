public interface IVersionService
{
    string GetVersionText(string version = null);
    string GetFormattedVersionText(string version);
}