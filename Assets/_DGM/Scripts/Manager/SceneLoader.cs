using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

public enum ESceneId
{
    Persistent,
    Loading,
    Title,
    Village,
    Forest,
    Temple
}

public class SceneLoader : Singleton<SceneLoader>
{
    private string _loadingKey = "Loading";
    private AsyncOperationHandle<SceneInstance> _loadingHandle;
    private AsyncOperationHandle<SceneInstance> _handle;

    private bool _isLoading = false;

    public bool IsLoading { get { return _isLoading; } }

    protected override void Awake()
    {
        base.Awake();

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        
    }

    public async UniTaskVoid LoadScene(ESceneId current, ESceneId target)
    {
        try
        {
            if (_isLoading)
                return;

            _isLoading = true;

            // 플레이어 이동 막기
            if (current != ESceneId.Title && GameManager.Instance != null)
            {
                GameManager.Instance.TogglePlayerMoveController(false);
            }

            _loadingHandle = Addressables.LoadSceneAsync(_loadingKey, LoadSceneMode.Additive);  // 로딩 씬 바로 띄움

            await _loadingHandle.Task;  // 로딩 씬 활성화

            // 기존 씬 언로드
            Scene currentScene = SceneManager.GetActiveScene();
            await SceneManager.UnloadSceneAsync(currentScene);

            string key = target.ToString();     // 타겟 씬

            _handle = Addressables.LoadSceneAsync(key, LoadSceneMode.Additive, false);  // 수동으로 제어

            await _handle.Task; // 씬 전환 준비 완료

            //// 캐시된 게 없으면 로드
            //string clipKey = key + "BGM";

            //if (!AudioManager.Instance.CheckClip(clipKey))
            //{
            //    Debug.Log($"딕셔너리에 없음");
            //    var bgmHandle = Addressables.LoadAssetAsync<AudioClip>(clipKey);
            //    Debug.Log("bgm 로드중");
            //    AudioClip bgmClip = await bgmHandle.Task;
            //    Debug.Log("bgm 로드완료");
            //    if (bgmHandle.Status == AsyncOperationStatus.Succeeded)
            //    {
            //        AudioManager.Instance.CacheClip(bgmClip);
            //    }

            //    else
            //    {
            //        Debug.Log("타겟 씬 BGM 로드 실패");
            //    }
            //}

            await _handle.Result.ActivateAsync();   // 씬 전환 직접 호출

            
            await UniTask.Yield(PlayerLoopTiming.Update);   // 한 프레임 대기

            if (current != ESceneId.Title)
            {
                GameManager.Instance.SetPlayerPosition(current, target);
            }
            
            await UniTask.Delay(2000);

            await Addressables.UnloadSceneAsync(_loadingHandle);    // 로딩 씬 언로드

            // 플레이어 이동 활성화
            if (current != ESceneId.Title && GameManager.Instance != null)
            {
                GameManager.Instance.TogglePlayerMoveController(true);
            }

            string clipKey = key + "BGM";
            AudioManager.Instance.PlayBGM(clipKey);     // 배경음악 재생
            _isLoading = false;
        }

        catch (Exception e)
        {
            Debug.LogError($"Error : {e.Message}");
        }
        
    }
}
