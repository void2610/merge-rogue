using UnityEngine;
using UnityEngine.EventSystems;

public class CanvasGroupNavigationLimiter : MonoBehaviour
{
    // 前回選択されていたUI要素
    private GameObject previousSelected;

    // プログラムによる選択変更の場合はtrueにするフラグ
    private static bool allowProgrammaticChange = false;

    /// <summary>
    /// プログラム側でUI要素の選択変更を行うためのラッパーメソッド。
    /// このメソッド経由で変更する場合は、CanvasGroupの制限を無視して移動できます。
    /// </summary>
    /// <param name="go">選択したいGameObject</param>
    public static void SetSelectedGameObjectSafe(GameObject go)
    {
        allowProgrammaticChange = true;
        EventSystem.current.SetSelectedGameObject(go);
    }

    void Update()
    {
        GameObject currentSelected = EventSystem.current.currentSelectedGameObject;

        // 選択が無い場合は状態をリセット
        if (currentSelected == null)
        {
            previousSelected = null;
            return;
        }

        // 初回の選択時は現在の選択を記憶
        if (previousSelected == null)
        {
            previousSelected = currentSelected;
            return;
        }

        // 前回と異なるUI要素に移動した場合
        if (currentSelected != previousSelected)
        {
            // プログラムによる変更の場合はフラグをリセットして更新
            if (allowProgrammaticChange)
            {
                allowProgrammaticChange = false;
                previousSelected = currentSelected;
                return;
            }

            // ユーザー入力による変更の場合、CanvasGroupを比較してグループが異なるなら元に戻す
            CanvasGroup currentGroup = currentSelected.GetComponentInParent<CanvasGroup>();
            CanvasGroup previousGroup = previousSelected.GetComponentInParent<CanvasGroup>();

            // 両方にCanvasGroupが存在し、かつ異なる場合はユーザー入力による移動とみなし、前の選択に戻す
            if (currentGroup != null && previousGroup != null && currentGroup != previousGroup)
            {
                // ユーザー入力による移動はキャンセル
                EventSystem.current.SetSelectedGameObject(previousSelected);
            }
            else
            {
                // CanvasGroupが同じ、または片方がない場合は更新
                previousSelected = currentSelected;
            }
        }
    }
}
