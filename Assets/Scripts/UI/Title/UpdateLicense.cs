using UnityEngine;
using TMPro;
using SyskenTLib.LicenseMaster;

public class UpdateLicense : MonoBehaviour
{
    [SerializeField] private LicenseManager licenseManager;
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private RectTransform content;
    
    private void Start()
    {
        var licenses = licenseManager.GetLicenseConfigsTxt();
        licenses = "\n\n\n\n" + licenses;
        text.text = licenses;
        
        // テキストのPreferred Valuesを取得
        var preferredHeight = text.GetPreferredValues().y;

        // Contentの高さをPreferred Heightに合わせる
        content.sizeDelta = new Vector2(content.sizeDelta.x, preferredHeight);
    }
}
