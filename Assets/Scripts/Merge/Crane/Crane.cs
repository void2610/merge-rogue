using UnityEngine;
using DG.Tweening;
using Cysharp.Threading.Tasks;

public class Crane : MonoBehaviour
{
    [SerializeField] private CableObject cableObject;
    [SerializeField] private Transform leftArm;
    [SerializeField] private Transform rightArm;
    [SerializeField] private float armSpeed = 1f;
    [SerializeField] private int cableLength = 10;

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
        for (var i = 0; i < cableLength; i++)
        {
            cableObject.AddLine();
            await UniTask.Delay(150);
        }
    }
    
    private async UniTask UpArm()
    {
        for (var i = 0; i < cableLength; i++)
        {
            cableObject.Reel();
            await UniTask.Delay(150);
        }
    }
    
    public async UniTask StartArmMove()
    {
        await OpenArm();
        await DownArm();
        await UniTask.Delay(700);
        await CloseArm();
        await UpArm();
        await UniTask.Delay(500);
    }
}
