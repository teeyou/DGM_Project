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

    public async UniTaskVoid LoadScene(ESceneId current, ESceneId target)
    {
        try
        {
            if (_isLoading)
                return;

            _isLoading = true;

            _loadingHandle = Addressables.LoadSceneAsync(_loadingKey, LoadSceneMode.Additive);  // 로딩 씬 바로 띄움

            await _loadingHandle.Task;  // 로딩 씬 활성화

            // 기존 씬 언로드
            Scene currentScene = SceneManager.GetActiveScene();
            await SceneManager.UnloadSceneAsync(currentScene);

            string key = target.ToString();
            _handle = Addressables.LoadSceneAsync(key, LoadSceneMode.Additive, false);  // 수동으로 제어

            await _handle.Task; // 씬 전환 준비 완료

            //await UniTask.Delay(2000);

            await _handle.Result.ActivateAsync();   // 씬 전환 직접 호출

            await UniTask.Yield(PlayerLoopTiming.Update);   // 한 프레임 대기

            if (current != ESceneId.Title)
            {
                GameManager.Instance.SetPlayerPosition(current, target);
            }
            
            await UniTask.Delay(2000);

            await Addressables.UnloadSceneAsync(_loadingHandle);    // 로딩 씬 언로드

            _isLoading = false;
        }

        catch (Exception e)
        {
            Debug.Log($"Error : {e.Message}");
        }
        
    }
}
