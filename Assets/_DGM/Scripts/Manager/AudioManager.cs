using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Audio;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AudioManager : Singleton<AudioManager>
{
    private const string MASTER_VOLUME_PARAM = "Master";
    private const string BGM_VOLUME_PARAM = "BGM";
    private const string SFX_VOLUME_PARAM = "SFX";

    private AudioSource _masterSource;
    [SerializeField] private AudioSource _bgmSource;
    [SerializeField] private AudioSource _sfxSource;

    [SerializeField] private AudioMixer _audioMixer;

    [SerializeField] private Slider _sliderMaster;
    [SerializeField] private Slider _sliderBGM;
    [SerializeField] private Slider _sliderSFX;

    [SerializeField] private GameObject _settingsPanel;
    [SerializeField] private Button _fullScreenButton;
    [SerializeField] private Button _windowedScreenButton;

    private Coroutine _fadeOutRoutine = null;

    private Dictionary<string, AudioClip> _keyToClip;

    public bool SFXLoaded { get; set; } = false;
    public bool BGMLoaded { get; set; } = false;

    private bool _isPlaying = false;

    protected override void Awake()
    {
        base.Awake();
        
        DontDestroyOnLoad(gameObject);

        _masterSource = GetComponent<AudioSource>();

        _keyToClip = new Dictionary<string, AudioClip>();
    }

    private void Start()
    {
        BindSlider();
        LoadSFXAssets();
        LoadBGMAssets();
    }

    private void OnDestroy()
    {
        UnBindSlider();
    }

    private void Update()
    {
        if (!BGMLoaded)
            return;

        if (!SFXLoaded)
            return;

        if (!_isPlaying)
        {
            _isPlaying = true;
            string sceneName = SceneLoader.Instance.GetCurrentSceneName();
            Debug.Log(sceneName);
            PlayBGM(sceneName + "BGM");
        }
    }

    private void BindSlider()
    {
        Debug.Log("BindSlider");
        if (_sliderMaster != null)
            _sliderMaster.onValueChanged.AddListener(SetMasterVolume);
        if (_sliderBGM != null)
            _sliderBGM.onValueChanged.AddListener(SetBGMVolume);
        if (_sliderSFX != null)
            _sliderSFX.onValueChanged.AddListener(SetSFXVolume);
    }

    private void UnBindSlider()
    {
        Debug.Log("UnBindSlider");
        if (_sliderMaster != null)
            _sliderMaster.onValueChanged.RemoveListener(SetMasterVolume);
        if (_sliderBGM != null)
            _sliderBGM.onValueChanged.RemoveListener(SetBGMVolume);
        if (_sliderSFX != null)
            _sliderSFX.onValueChanged.RemoveListener(SetSFXVolume);
    }

    private void SetMasterVolume(float value)
    {
        //_masterSource.volume = value;
        Debug.Log($"SetMasterVolume: {value}");
        _audioMixer.SetFloat(MASTER_VOLUME_PARAM, Mathf.Log10(value) * 20);
    }

    private void SetBGMVolume(float value)
    {
        //_bgmSource.volume = value;
        Debug.Log($"SetBGMVolume: {value}");
        _audioMixer.SetFloat(BGM_VOLUME_PARAM, Mathf.Log10(value) * 20);
    }

    private void SetSFXVolume(float value)
    {
        //_sfxSource.volume = value;
        Debug.Log($"SetSFXVolume: {value}");
        _audioMixer.SetFloat(SFX_VOLUME_PARAM, Mathf.Log10(value) * 20);
    }

    private void LoadBGMAssets()
    {
        var handle = Addressables.LoadAssetsAsync<AudioClip>("BGM", clip =>
        {
            _keyToClip[clip.name] = clip;
        });

        handle.Completed += op =>
        {
            if (op.Status == AsyncOperationStatus.Succeeded)
            {
                BGMLoaded = true;
                Debug.Log("BGM 로드 완료");
            }
            else
            {
                Debug.Log("BGM 로드 실패");
            }
        };
    }


    private void LoadSFXAssets()
    {
        var handle = Addressables.LoadAssetsAsync<AudioClip>("SFX", clip =>
        {
            _keyToClip[clip.name] = clip;
        });

        handle.Completed += op =>
        {
            if (op.Status == AsyncOperationStatus.Succeeded)
            {
                SFXLoaded = true;
                Debug.Log("SFX 로드 완료");
            }
            else
            {
                Debug.Log("SFX 로드 실패");
            }
        };
    }

    public void CacheClip(AudioClip clip)
    {
        _keyToClip[clip.name] = clip;
    }

    public bool CheckClip(string name)
    {
        return _keyToClip.ContainsKey(name);
    }

    public void PlayBGM(AudioClip clip)
    {
        _bgmSource.clip = clip;
        _bgmSource.loop = true;
        _bgmSource.Play();
    }

    public void PlayBGM(string key)
    {
        if (_keyToClip.TryGetValue(key, out AudioClip clip))
        {
            _bgmSource.clip = clip;
            _bgmSource.loop = true;
            _bgmSource.Play();
        }
    }

    public void StopBGM()
    {
        //_bgmSource.Stop();

        if (_fadeOutRoutine == null)
            _fadeOutRoutine = StartCoroutine(FadeOutBGM(2f));
    }

    public IEnumerator FadeOutBGM(float duration)
    {
        float startVolume = _bgmSource.volume;
        while (_bgmSource.volume > 0)
        {
            _bgmSource.volume -= startVolume * Time.deltaTime / duration;
            yield return null;
        }
        _bgmSource.Stop();
        _bgmSource.volume = startVolume;

        _fadeOutRoutine = null;
    }

    public void PlaySFX(AudioClip clip)
    {
        _sfxSource.PlayOneShot(clip);
    }

    public void PlaySFX(string key)
    {
        if (_keyToClip.TryGetValue(key, out AudioClip clip))
        {
            if (_sfxSource.isPlaying)
                return;

            _sfxSource.PlayOneShot(clip);
        }
    }

    public void RegisterSettingsPanel()
    {
        FieldUIController.Instance.SettingsPanel = _settingsPanel;
    }

    public void RegisterScreenModeButton()
    {
        FieldUIController.Instance.FullScreenButton = _fullScreenButton;
        FieldUIController.Instance.WindowedScreenButton = _windowedScreenButton;
    }
}
