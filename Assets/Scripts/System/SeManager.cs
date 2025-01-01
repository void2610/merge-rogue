using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

public class SeManager : MonoBehaviour
{
    [System.Serializable]
    public class SoundData
    {
        public string name;
        public AudioClip audioClip;
        public float volume = 1.0f;
    }

    [SerializeField] private AudioMixerGroup seMixerGroup;
    [SerializeField] private SoundData[] soundDatas;

    public static SeManager Instance;
    private readonly AudioSource[] _seAudioSourceList = new AudioSource[20];
    private float _seVolume = 0.5f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }

        for (var i = 0; i < _seAudioSourceList.Length; ++i)
        {
            _seAudioSourceList[i] = gameObject.AddComponent<AudioSource>();
            _seAudioSourceList[i].outputAudioMixerGroup = seMixerGroup;
        }
    }

    public float SeVolume
    {
        get => _seVolume;
        set
        {
            _seVolume = value;
            if (value <= 0.0f)
            {
                value = 0.0001f;
            }
            seMixerGroup.audioMixer.SetFloat("SeVolume", Mathf.Log10(value) * 20);
            PlayerPrefs.SetFloat("SeVolume", value);
        }
    }

    public void PlaySe(AudioClip clip, float volume = 1.0f, float pitch = 1.0f)
    {
        var audioSource = GetUnusedAudioSource();
        if (clip == null)
        {
            Debug.LogError("AudioClip could not be found.");
            return;
        }
        if (audioSource == null)
        {
            // Debug.LogWarning("There is no available AudioSource.");
            return;
        }

        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.pitch = pitch;
        audioSource.Play();
    }

    public void PlaySe(string seName, float volume = 1.0f, float pitch = 1.0f)
    {
        var soundData = soundDatas.FirstOrDefault(t => t.name == seName);
        var audioSource = GetUnusedAudioSource();
        if (soundData == null)
        {
            Debug.LogError("AudioClip could not be found: " + seName);
            return;
        }
        if (!audioSource)
        {
            // Debug.LogWarning("There is no available AudioSource");
            return;
        }

        audioSource.clip = soundData.audioClip;
        audioSource.volume = soundData.volume * volume;
        audioSource.pitch = pitch;
        audioSource.Play();
    }

    private AudioSource GetUnusedAudioSource() => _seAudioSourceList.FirstOrDefault(t => t.isPlaying == false);

    private void Start()
    {
        SeVolume = PlayerPrefs.GetFloat("SeVolume", 1.0f);
        seMixerGroup.audioMixer.SetFloat("SeVolume", Mathf.Log10(_seVolume) * 20);
    }
}
