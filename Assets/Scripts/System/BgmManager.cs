using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class BgmManager : MonoBehaviour
{
    public enum BgmType
    {
        Battle,
        AfterBattle,
        Shop,
        Event,
        Boss,
    }
    
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
    private bool _isPlaying = false;
    private SoundData _currentBGM;
    private float _volume = 1.0f;
    private const float FADE_TIME = 1.5f;
    private bool _isFading = false;

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
            PlayerPrefs.SetFloat("BgmVolume", value);
            bmgMixerGroup.audioMixer.SetFloat("BgmVolume", Mathf.Log10(value) * 20);
            AudioSource.volume = _currentBGM?.volume ?? 1;
        }
    }

    public void Play()
    {
        if (_currentBGM == null) return;

        _isPlaying = true;
        AudioSource.Play();
        AudioSource.DOFade(_currentBGM.volume, FADE_TIME).SetUpdate(true).SetEase(Ease.InQuad);
    }

    public void Stop()
    {
        _isPlaying = false;
        AudioSource.DOFade(0, FADE_TIME).SetUpdate(true).SetEase(Ease.InQuad).OnComplete(() => AudioSource.Stop());
    }

    private void PlayRandomBGM(BgmType bgmType = BgmType.Battle)
    {
        if (bgmList.Count == 0) return;

        AudioSource.Stop();

        var targetBgmList = bgmList.FindAll(x => x.bgmType == bgmType);
        _currentBGM = targetBgmList[Random.Range(0, targetBgmList.Count)];
        AudioSource.clip = _currentBGM.audioClip;
        AudioSource.volume = 0;

        AudioSource.Play();
        AudioSource.DOFade(_currentBGM.volume, FADE_TIME).SetUpdate(true).SetEase(Ease.InQuad).OnComplete(() => _isFading = false);
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    private void Start()
    {
        _volume = PlayerPrefs.GetFloat("BgmVolume", 1.0f);
        bmgMixerGroup.audioMixer.SetFloat("BgmVolume", Mathf.Log10(_volume) * 20);
        AudioSource.volume = 0;
        AudioSource.outputAudioMixerGroup = bmgMixerGroup;
        if (playOnStart)
        {
            _isPlaying = true;
            PlayRandomBGM();
        }
    }

    private void Update()
    {
        if (_isPlaying && AudioSource.clip)
        {
            var remainingTime = AudioSource.clip.length - AudioSource.time;
            if (!(remainingTime <= FADE_TIME) || _isFading) return;
            
            _isFading = true;
            AudioSource.DOFade(0, remainingTime).SetUpdate(true).SetEase(Ease.InQuad).OnComplete(() => PlayRandomBGM());
        }
    }
}
