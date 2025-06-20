using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class SeedText : MonoBehaviour
{
    private string _seedText;
    
    [Inject]
    public void InjectDependencies(IRandomService randomService)
    {
       Debug.Log("SeedText: Injecting dependencies");
        var t = this.GetComponent<TextMeshProUGUI>();
        _seedText = randomService.SeedText;
        t.text = $"Seed: {_seedText}";
    }

    private void Start()
    {
        // クリックしたらシードをコピーする
        this.GetComponent<Button>().onClick.AddListener(() =>
        {
            GUIUtility.systemCopyBuffer = _seedText;
            NotifyWindow.Instance.Notify(NotifyWindow.NotifyType.SeedCopied);
        });
    }
}
