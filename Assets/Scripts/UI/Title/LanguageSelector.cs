using UnityEngine;
using UnityEngine.Localization.Settings;
using TMPro;
using Cysharp.Threading.Tasks;

public class LanguageSelector : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown languageDropdown;

    private async void Start()
    {
        // ローカライズシステムの初期化を待つ
        await LocalizationSettings.InitializationOperation.Task;

        // UIを構築
        languageDropdown.ClearOptions();
        var options = new System.Collections.Generic.List<string>();

        foreach (var locale in LocalizationSettings.AvailableLocales.Locales)
            options.Add(locale.Identifier.CultureInfo.NativeName);

        languageDropdown.AddOptions(options);
        languageDropdown.value = LocalizationSettings.AvailableLocales.Locales.IndexOf(LocalizationSettings.SelectedLocale);
        languageDropdown.onValueChanged.AddListener(OnChanged);
    }

    private void OnChanged(int idx)
    {
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[idx];
    }
}