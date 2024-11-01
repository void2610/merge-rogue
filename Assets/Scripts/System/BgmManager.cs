using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class BgmManager : MonoBehaviour
{
    [System.Serializable]
    public class SoundData
    {
        public AudioClip audioClip;
        public float volume = 1.0f;
    }

    public static BgmManager Instance;
    [SerializeField]
    private bool playOnStart = true;
    [SerializeField]
    private AudioMixerGroup bmgMixerGroup;
    [SerializeField]
    private List<SoundData> bgmList = new List<SoundData>();

    private AudioSource AudioSource => this.GetComponent<AudioSource>();
    private bool isPlaying = false;
    private SoundData currentBGM;
    private float volume = 1.0f;
    private const float FadeTime = 1.5f;
    private bool isFading = false;

    public float BgmVolume
    {
        get
        {
            return volume;
        }
        set
        {
            if (value <= 0.0f)
            {
                value = 0.0001f;
            }
            volume = value;
            PlayerPrefs.SetFloat("BgmVolume", value);
            bmgMixerGroup.audioMixer.SetFloat("BgmVolume", Mathf.Log10(value) * 20);
            AudioSource.volume = currentBGM != null ? currentBGM.volume : 1;
        }
    }

    public void Play()
    {
        if (currentBGM == null) return;

        isPlaying = true;
        AudioSource.Play();
        AudioSource.DOFade(currentBGM.volume, FadeTime).SetUpdate(true).SetEase(Ease.InQuad);
    }

    public void Stop()
    {
        isPlaying = false;
        AudioSource.DOFade(0, FadeTime).SetUpdate(true).SetEase(Ease.InQuad).OnComplete(() => AudioSource.Stop());
    }

    private void PlayRandomBGM()
    {
        if (bgmList.Count == 0) return;

        AudioSource.Stop();

        var bgm = bgmList[Random.Range(0, bgmList.Count)];
        currentBGM = bgm;
        AudioSource.clip = currentBGM.audioClip;
        AudioSource.volume = 0;

        AudioSource.Play();
        AudioSource.DOFade(currentBGM.volume, FadeTime).SetUpdate(true).SetEase(Ease.InQuad).OnComplete(() => isFading = false);
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
        volume = PlayerPrefs.GetFloat("BgmVolume", 1.0f);
        bmgMixerGroup.audioMixer.SetFloat("BgmVolume", Mathf.Log10(volume) * 20);
        AudioSource.volume = 0;
        AudioSource.outputAudioMixerGroup = bmgMixerGroup;
        if (playOnStart)
        {
            isPlaying = true;
            PlayRandomBGM();
        }
    }

    private void Update()
    {
        if (isPlaying && AudioSource.clip)
        {
            var remainingTime = AudioSource.clip.length - AudioSource.time;
            if (!(remainingTime <= FadeTime) || isFading) return;
            
            isFading = true;
            AudioSource.DOFade(0, remainingTime).SetUpdate(true).SetEase(Ease.InQuad).OnComplete(() => PlayRandomBGM());
        }
    }
}
