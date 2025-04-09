using UnityEngine;
using UnityEngine.Serialization;

public class FillingRateManager : MonoBehaviour
{
    public enum FillingRateType
    {
        Lower,
        Middle,
        Higher
    }
    
    [SerializeField] private FillingRateTrigger lowerTrigger;
    [SerializeField] private FillingRateTrigger higherTrigger;
    public static FillingRateType FillingRate;

    private void CalcFillingGauge()
    {
        if (higherTrigger.IsCollideWithBall())
        {
            FillingRate = FillingRateType.Higher;
        }
        else if (lowerTrigger.IsCollideWithBall())
        {
            FillingRate = FillingRateType.Middle;
        }
        else 
        {
            FillingRate = FillingRateType.Lower;
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
