using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

public class AudioManager : Singleton<AudioManager>
{
    private AudioSource _masterSource;
    [SerializeField] private AudioSource _bgmSource;
    [SerializeField] private AudioSource _sfxSource;

    private Coroutine _fadeOutRoutine = null;

    private Dictionary<string, AudioClip> _keyToClip;

    public bool SFXLoaded { get; set; } = false;
    public bool BGMLoaded { get; set; } = false;
    protected override void Awake()
    {
        base.Awake();
        
        DontDestroyOnLoad(gameObject);

        _masterSource = GetComponent<AudioSource>();

        _keyToClip = new Dictionary<string, AudioClip>();
    }

    private void Start()
    {
        LoadSFXAssets();
        LoadBGMAssets();
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
            _sfxSource.PlayOneShot(clip);
    }
}
