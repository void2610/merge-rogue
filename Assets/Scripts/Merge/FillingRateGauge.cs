using UnityEngine;
using UnityEngine.Serialization;

public class FillingRateGauge : MonoBehaviour
{
    public enum FillingRateType
    {
        Lower,
        Middle,
        Higher
    }
    
    [SerializeField] private FillingRateTrigger lowerTrigger;
    [SerializeField] private FillingRateTrigger higherTrigger;
    [SerializeField] private FillingRateType fillingRate;
    public FillingRateType GetFillingRate() => fillingRate;

    private void CalcFillingGauge()
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
    }

    private void Start()
    {
        
    }

    private void Update()
    {
        CalcFillingGauge();
    }
}
