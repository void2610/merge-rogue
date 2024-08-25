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

    public static BgmManager instance;
    [SerializeField]
    private bool playOnStart = true;
    [SerializeField]
    private AudioMixer mixer;
    [SerializeField]
    private List<SoundData> bgmList = new List<SoundData>();

    private AudioSource audioSource => this.GetComponent<AudioSource>();
    private bool isPlaying = false;
    private SoundData currentBGM;
    private float volume = 1.0f;
    private float fadeTime = 1.5f;
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
            mixer.SetFloat("BgmVolume", Mathf.Log10(value) * 20);
            audioSource.volume = currentBGM != null ? currentBGM.volume : 1;
        }
    }

    public void Play()
    {
        if (currentBGM == null) return;

        isPlaying = true;
        audioSource.Play();
        audioSource.DOFade(currentBGM.volume, fadeTime).SetUpdate(true).SetEase(Ease.InQuad);
    }

    public void Stop()
    {
        isPlaying = false;
        audioSource.DOFade(0, fadeTime).SetUpdate(true).SetEase(Ease.InQuad).OnComplete(() => audioSource.Stop());
    }

    private void PlayRandomBGM()
    {
        if (bgmList.Count == 0) return;

        audioSource.Stop();

        var bgm = bgmList[Random.Range(0, bgmList.Count)];
        currentBGM = bgm;
        audioSource.clip = currentBGM.audioClip;
        audioSource.volume = 0;

        audioSource.Play();
        audioSource.DOFade(currentBGM.volume, fadeTime).SetUpdate(true).SetEase(Ease.InQuad).OnComplete(() => isFading = false);
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    private void Start()
    {
        volume = PlayerPrefs.GetFloat("BgmVolume", 1.0f);
        mixer.SetFloat("BgmVolume", Mathf.Log10(volume) * 20);
        audioSource.volume = 0;
        if (playOnStart)
        {
            isPlaying = true;
            PlayRandomBGM();
        }
    }

    private void Update()
    {
        if (isPlaying && audioSource.clip != null)
        {
            float remainingTime = audioSource.clip.length - audioSource.time;
            if (remainingTime <= fadeTime && !isFading)
            {
                isFading = true;
                audioSource.DOFade(0, remainingTime).SetUpdate(true).SetEase(Ease.InQuad).OnComplete(() => PlayRandomBGM());
            }
        }
    }
}
