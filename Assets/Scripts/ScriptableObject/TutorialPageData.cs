using UnityEngine;

[CreateAssetMenu(fileName = "New Tutorial Page", menuName = "Tutorial/Tutorial Page Data")]
public class TutorialPageData : ScriptableObject
{
    [Header("基本情報")]
    [SerializeField] private string pageId;
    
    [Header("表示設定")]
    [SerializeField] private Sprite tutorialImage;
    
    public Sprite TutorialImage => tutorialImage;
    
    // ローカライズされたタイトルを取得
    public string GetTitle() => LocalizeStringLoader.Instance.Get($"{pageId}_T");
    
    // ローカライズされた説明を取得
    public string GetDescription() => LocalizeStringLoader.Instance.Get($"{pageId}_D");
}