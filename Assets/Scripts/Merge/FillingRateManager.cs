using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using DG.Tweening;

public class FillingRateManager : MonoBehaviour
{
    public enum FillingRateType
    {
        Higher,
        Middle,
        Lower,
    }
    
    public static FillingRateManager Instance;
    
    [SerializeField] private FillingRateTrigger lowerTrigger;
    [SerializeField] private FillingRateTrigger higherTrigger;
    [SerializeField] private Image fillImage;
    public FillingRateType fillingRate;

    public FillingRateType CalcFillingGauge()
    {
        if (higherTrigger.IsCollideWithBall())
        {
            fillingRate = FillingRateType.Higher;
        }
        else if (lowerTrigger.IsCollideWithBall())
        {
            fillingRate = FillingRateType.Middle;
        }
        else 
        {
            fillingRate = FillingRateType.Lower;
        }
        
        var fill = fillingRate switch
        {
            FillingRateType.Higher => 1f,
            FillingRateType.Middle => 0.5f,
            FillingRateType.Lower => 0f,
            _ => 0f
        };
        fillImage.DOFillAmount(fill, 0.5f);
        return fillingRate;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        
        fillImage.fillAmount = 0f;
        fillingRate = FillingRateType.Lower;
    }
}
