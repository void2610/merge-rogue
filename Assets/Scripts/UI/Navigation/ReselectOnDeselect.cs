using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Rendering;

public class ReselectOnDeselect : MonoBehaviour, IDeselectHandler
{
    // 再選択するまでの遅延時間（必要に応じて調整）
    [SerializeField]
    private float reselectDelay = 0.1f;

    public void OnDeselect(BaseEventData eventData)
    {
        if (TryGetComponent<Button>(out var b) && b.interactable) return;
        if (EventSystem.current.currentSelectedGameObject != null)
        {
            Debug.Log(EventSystem.current.currentSelectedGameObject.name);
            return;
        } 
        Debug.Log("OnDeselect");
        StartCoroutine(Reselect());
    }

    private IEnumerator Reselect()
    {
        // 少し待機して、他の処理が終わるのを待つ
        yield return new WaitForSeconds(reselectDelay);
        EventSystem.current.SetSelectedGameObject(gameObject);
    }
}