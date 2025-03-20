using UnityEngine;
using UnityEngine.EventSystems;

public class CanvasGroupNavigationLimiter : MonoBehaviour
{
    // 前回選択されていたUI要素
    private GameObject _previousSelected;

    // プログラムによる選択変更の場合はtrueにするフラグ
    private static bool _allowProgrammaticChange = false;

    /// <summary>
    /// プログラム側でUI要素の選択変更を行うためのラッパーメソッド。
    /// このメソッド経由で変更する場合は、CanvasGroupの制限を無視して移動できます。
    /// </summary>
    /// <param name="go">選択したいGameObject</param>
    public static void SetSelectedGameObjectSafe(GameObject go)
    {
        _allowProgrammaticChange = true;
        EventSystem.current.SetSelectedGameObject(go);
    }

    private void Update()
    {
        var currentSelected = EventSystem.current.currentSelectedGameObject;

        // 選択が無い場合は状態をリセット
        if (!currentSelected)
        {
            _previousSelected = null;
            return;
        }

        // 初回の選択時は現在の選択を記憶
        if (!_previousSelected)
        {
            _previousSelected = currentSelected;
            return;
        }

        // 前回と異なるUI要素に移動した場合
        if (currentSelected != _previousSelected)
        {
            // プログラムによる変更の場合はフラグをリセットして更新
            if (_allowProgrammaticChange)
            {
                _allowProgrammaticChange = false;
                _previousSelected = currentSelected;
                return;
            }

            // ユーザー入力による変更の場合、CanvasGroupを比較してグループが異なるなら元に戻す
            var currentGroup = currentSelected.GetComponentInParent<CanvasGroup>();
            var previousGroup = _previousSelected.GetComponentInParent<CanvasGroup>();

            // 両方にCanvasGroupが存在し、かつ異なる場合はユーザー入力による移動とみなし、前の選択に戻す
            if (currentGroup && previousGroup && currentGroup != previousGroup)
            {
                // ユーザー入力による移動はキャンセル
                EventSystem.current.SetSelectedGameObject(_previousSelected);
            }
            else
            {
                // CanvasGroupが同じ、または片方がない場合は更新
                _previousSelected = currentSelected;
            }
        }
    }
}
