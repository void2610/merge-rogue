using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using VContainer;

public class BgmManager : MonoBehaviour
{
    [System.Serializable]
    public class SoundData
    {
        public AudioClip audioClip;
        public float volume = 1.0f;
        public BgmType bgmType = BgmType.Battle;
    }

    public static BgmManager Instance;
    [SerializeField]
    private bool playOnStart = true;
    [SerializeField]
    private AudioMixerGroup bmgMixerGroup;
    [SerializeField]
    private List<SoundData> bgmList = new List<SoundData>();

    private AudioSource AudioSource => this.GetComponent<AudioSource>();
    
    private const float FADE_TIME = 1.0f;
    
    private bool _isPlaying = false;
    private float _volume = 1.0f;
    private bool _isFading = false;
    private SoundData _currentBGM = null;
    private IGameSettingsService _gameSettingsService;

    public float BgmVolume
    {
        get => _volume;
        set
        {
            if (value <= 0.0f)
            {
                value = 0.0001f;
            }
            _volume = value;
            _gameSettingsService.SaveBgmVolume(value);
            bmgMixerGroup.audioMixer.SetFloat("BgmVolume", Mathf.Log10(value) * 20);
            AudioSource.volume = _currentBGM?.volume ?? 1;
        }
    }

    public void Resume()
    {
        if (_currentBGM == null) return;

        _isPlaying = true;
        AudioSource.Play();
        AudioSource.DOFade(_currentBGM.volume, FADE_TIME).SetUpdate(true).SetEase(Ease.InQuad).Forget();
    }

    public void Pause()
    {
        _isPlaying = false;
        AudioSource.DOFade(0, FADE_TIME).SetUpdate(true).SetEase(Ease.InQuad).OnComplete(() => AudioSource.Stop()).Forget();
    }
    
    public async UniTaskVoid Stop()
    {
        _isPlaying = false;
        await AudioSource.DOFade(0, FADE_TIME).SetUpdate(true).SetEase(Ease.InQuad).OnComplete(() => AudioSource.Stop());
        _currentBGM = null;
    }

    public async UniTaskVoid PlayRandomBGM(BgmType bgmType = BgmType.Battle)
    {
        if (bgmList.Count == 0) return;
        if (_currentBGM != null && _currentBGM.bgmType == bgmType) return;

        if (_currentBGM != null)
            await AudioSource.DOFade(0, FADE_TIME).SetUpdate(true).SetEase(Ease.InQuad).OnComplete(() => AudioSource.Stop());

        var targetBgmList = bgmList.FindAll(x => x.bgmType == bgmType);
        _currentBGM = targetBgmList[Random.Range(0, targetBgmList.Count)];
        AudioSource.clip = _currentBGM.audioClip;
        AudioSource.volume = 0;

        AudioSource.Play();
        AudioSource.DOFade(_currentBGM.volume, FADE_TIME).SetUpdate(true).SetEase(Ease.InQuad).OnComplete(() => _isFading = false).Forget();
    }

    private void Awake()
    {
        if (!Instance)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    [Inject]
    public void InjectDependencies(IGameSettingsService gameSettingsService)
    {
        _gameSettingsService = gameSettingsService;
    }
    
    private void Start()
    {
        _currentBGM = null;
        var audioSettings = _gameSettingsService.GetAudioSettings();
        _volume = audioSettings.bgmVolume;
        bmgMixerGroup.audioMixer.SetFloat("BgmVolume", Mathf.Log10(_volume) * 20);
        AudioSource.volume = 0;
        AudioSource.outputAudioMixerGroup = bmgMixerGroup;
        if (playOnStart)
        {
            _isPlaying = true;
            PlayRandomBGM().Forget();
        }
    }

    private void Update()
    {
        if (_isPlaying && AudioSource.clip)
        {
            var remainingTime = AudioSource.clip.length - AudioSource.time;
            if (!(remainingTime <= FADE_TIME) || _isFading) return;
            
            _isFading = true;
            AudioSource.DOFade(0, remainingTime).SetUpdate(true).SetEase(Ease.InQuad).OnComplete(() => PlayRandomBGM().Forget());
        }
    }
}
