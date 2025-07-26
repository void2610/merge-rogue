using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Threading;

/// <summary>
/// 確認ダイアログの表示を担当するViewクラス
/// </summary>
public class ConfirmationDialogView : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Image dialogBackground;
    [SerializeField] private GameObject dialogPanel;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private TextMeshProUGUI confirmButtonText;
    [SerializeField] private TextMeshProUGUI cancelButtonText;
    
    private CancellationTokenSource _currentDialogCts;
    private UniTaskCompletionSource<bool> _dialogResult;

    /// <summary>
    /// 確認ダイアログを表示し、ユーザーの選択を待つ
    /// </summary>
    /// <param name="message">表示するメッセージ</param>
    /// <param name="confirmText">確認ボタンのテキスト（デフォルト: "OK"）</param>
    /// <param name="cancelText">キャンセルボタンのテキスト（デフォルト: "キャンセル"）</param>
    /// <returns>true: 確認, false: キャンセル</returns>
    public async UniTask<bool> ShowDialog(string message, string confirmText = "OK", string cancelText = "キャンセル")
    {
        // 現在実行中のダイアログをキャンセル
        _currentDialogCts?.Cancel();
        _currentDialogCts?.Dispose();
        
        // 新しいキャンセレーショントークンを作成
        _currentDialogCts = new CancellationTokenSource();
        
        // アプリケーション終了時にもキャンセルされるようにする
        var cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(
            _currentDialogCts.Token,
            this.GetCancellationTokenOnDestroy(),
            Application.exitCancellationToken
        ).Token;
        
        try
        {
            // ダイアログの結果を管理するCompletionSourceを作成
            _dialogResult = new UniTaskCompletionSource<bool>();
            
            // メッセージとボタンテキストを設定
            messageText.text = message;
            confirmButtonText.text = confirmText;
            cancelButtonText.text = cancelText;
            
            // ダイアログを表示
            dialogPanel.SetActive(true);
            
            // ボタンのインタラクションを有効化
            confirmButton.interactable = true;
            cancelButton.interactable = true;
            
            // ユーザーの選択を待つ
            var result = await _dialogResult.Task;
            
            // ダイアログを非表示
            HideDialog();
            
            return result;
        }
        catch (System.OperationCanceledException)
        {
            // キャンセルされた場合のクリーンアップ
            HideDialog();
            return false;
        }
    }
    
    /// <summary>
    /// ダイアログを非表示にする
    /// </summary>
    private void HideDialog()
    {
        dialogPanel.SetActive(false);
        
        // ボタンのインタラクションを無効化
        confirmButton.interactable = false;
        cancelButton.interactable = false;
    }
    
    /// <summary>
    /// 確認ボタンがクリックされた時の処理
    /// </summary>
    private void OnConfirmClicked()
    {
        _dialogResult?.TrySetResult(true);
    }
    
    /// <summary>
    /// キャンセルボタンがクリックされた時の処理
    /// </summary>
    private void OnCancelClicked()
    {
        _dialogResult?.TrySetResult(false);
    }
    
    private void Awake()
    {
        // 初期状態の設定 - ダイアログパネルを非表示
        dialogPanel.SetActive(false);
        
        // ボタンイベントの設定
        confirmButton.onClick.AddListener(OnConfirmClicked);
        cancelButton.onClick.AddListener(OnCancelClicked);
        
        // ボタンを無効化
        confirmButton.interactable = false;
        cancelButton.interactable = false;
    }
    
    private void OnDestroy()
    {
        // ダイアログのキャンセレーショントークンをクリーンアップ
        _currentDialogCts?.Cancel();
        _currentDialogCts?.Dispose();
        
        // 未完了のCompletionSourceをキャンセル
        _dialogResult?.TrySetCanceled();
    }
}