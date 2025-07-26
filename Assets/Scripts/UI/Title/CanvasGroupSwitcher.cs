using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class CanvasGroupSwitcher
{
    private readonly List<CanvasGroup> _canvasGroups;
    private readonly Dictionary<string, Sequence> _canvasGroupTween = new ();
    
    public CanvasGroupSwitcher(List<CanvasGroup> canvasGroups)
    {
        _canvasGroups = canvasGroups;
        
        foreach (var cg in _canvasGroups)
        {
            _canvasGroupTween[cg.name] = null;
            EnableCanvasGroupAsync(cg.name, false).Forget();
        }
    }
    
    public GameObject GetTopCanvasGroup() => _canvasGroups.Find(c => c.alpha > 0)?.gameObject;
    
    public void EnableCanvasGroup(string canvasName, bool e) => EnableCanvasGroupAsync(canvasName, e).Forget();
    
    public async UniTask EnableCanvasGroupAsync(string canvasName, bool e)
    {
        var cg = _canvasGroups.Find(c => c.name == canvasName);
        if (!cg) return;
        if (_canvasGroupTween[canvasName].IsActive()) return;
        
        // アニメーション中は操作をブロック
        cg.interactable = false;
        cg.blocksRaycasts = false;
        
        var seq = DOTween.Sequence();
        seq.SetUpdate(true).Forget();
        if (e)
        {
            seq.Join(cg.transform.DOMoveY(-0.45f, 0).SetRelative(true)).Forget();
            seq.Join(cg.transform.DOMoveY(0.45f, 0.2f).SetRelative(true).SetEase(Ease.OutBack)).Forget();
            seq.Join(cg.DOFade(1, 0.2f)).Forget();
        }
        else
        {
            seq.Join(cg.DOFade(0, 0.2f)).Forget();
        }
        
        _canvasGroupTween[canvasName] = seq;
        SelectionCursor.SetSelectedGameObjectSafe(null);
        
        await seq.AsyncWaitForCompletion();
        
        _canvasGroupTween[canvasName] = null;
        cg.interactable = e;
        cg.blocksRaycasts = e;
    }
}