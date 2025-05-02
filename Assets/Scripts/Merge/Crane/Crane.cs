using UnityEngine;
using DG.Tweening;
using Cysharp.Threading.Tasks;

public class Crane : MonoBehaviour
{
    [SerializeField] private CableObject cableObject;
    [SerializeField] private Transform leftArm;
    [SerializeField] private Transform rightArm;
    [SerializeField] private float armSpeed = 1f;

    private async UniTask OpenArm()
    {
        leftArm.DOLocalRotate(new Vector3(0, 0, -90), armSpeed).SetEase(Ease.OutBack).SetLink(gameObject).Forget();
        await rightArm.DOLocalRotate(new Vector3(0, 0, 90), armSpeed).SetEase(Ease.OutBack).SetLink(gameObject);
    }
    
    private async UniTask CloseArm()
    {
        leftArm.DOLocalRotate(new Vector3(0, 0, 0), armSpeed).SetEase(Ease.OutBack).SetLink(gameObject).Forget();
        await rightArm.DOLocalRotate(new Vector3(0, 0, 0), armSpeed).SetEase(Ease.OutBack).SetLink(gameObject);
    }
    
    private async UniTask DownArm()
    {
        for (var i = 0; i < 10; i++)
        {
            cableObject.AddLine();
            await UniTask.Delay(100);
        }
    }
    
    private async UniTask UpArm()
    {
        for (var i = 0; i < 10; i++)
        {
            cableObject.Reel();
            await UniTask.Delay(100);
        }
    }
    
    public async UniTask StartArmMove()
    {
        await DownArm();
        await OpenArm();
        await UniTask.Delay(1000);
        await CloseArm();
        await UpArm();
        await UniTask.Delay(1000);
    }
}
