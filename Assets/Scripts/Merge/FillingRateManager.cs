using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
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
    [SerializeField] private TextMeshProUGUI fillingRateText;
    [SerializeField] private ParticleSystem fillingRateParticle;
    
    public FillingRateType fillingRate;
    private Material _gaugeMaterial;
    private float _currentIntensity;
    private Color _baseColor = Color.red;
    
    public float CalcFillingGauge()
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
        fillingRateParticle.emissionRate = fillingRate switch
        {
            FillingRateType.Higher => 12.5f,
            FillingRateType.Middle => 2.5f,
            FillingRateType.Lower => 0f,
            _ => 0f
        };

        var res = fillingRate switch
        {
            FillingRateType.Higher => 2f,
            FillingRateType.Middle => 1.5f,
            FillingRateType.Lower => 1f,
            _ => 1f
        };
            
        fillingRateText.text = "x" + res.ToString("F1");
        fillingRateText.color = fillingRate.GetColor();
        
        return res;
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
        
		_gaugeMaterial = fillImage.material;
        _currentIntensity = 0.5f;
        _gaugeMaterial.SetColor("_EmissionColor", _baseColor * _currentIntensity);
        DOTween.To(() => _currentIntensity, x => {
                _currentIntensity = x;
                _gaugeMaterial.SetColor("_EmissionColor", _baseColor * _currentIntensity);
            }, 1.0f, 3.0f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
        
        fillingRateParticle.emissionRate = 0f;
    }
}
