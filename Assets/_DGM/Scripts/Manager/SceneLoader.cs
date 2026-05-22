using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Playables;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

public enum ESceneId
{
    Loading,
    Title,
    Village,
    VillageEast,
    Forest,
    Temple,
    VillageEastBattle,
    ForestBattle,
    TempleBattle,
}

public class SceneLoader : Singleton<SceneLoader>
{
    [SerializeField] private int _delay;

    public event Action OnBattleSceneLoaded;

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

    public ESceneId ConvertStringToESceneId(string sceneName)
    {
        if (Enum.TryParse(sceneName, out ESceneId targetScene))
        {
            return targetScene;
        }
        else
        {
            Debug.Log("GetCurrentESceneId 에러 발생");
            return ESceneId.Title;
        }
    }

    public string GetCurrentSceneName()
    {
        return SceneManager.GetActiveScene().name;
    }

    public Scene GetCurrentScene()
    {
        return SceneManager.GetActiveScene();
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

    private async UniTaskVoid LoadTargetSceneAsync(ESceneId target, bool isReturn)
    {
        try
        {
            if (_isLoading)
                return;

            _isLoading = true;

            if (!isReturn)
            {
                GameManager.Instance.TogglePlayerMoveController(false);
            }
            
            await FadeInOut.Instance.FadeInAsync();

            _loadingHandle = Addressables.LoadSceneAsync(_loadingKey, LoadSceneMode.Additive);  // 로딩 씬 바로 띄움

            await _loadingHandle.Task;  // 로딩 씬 활성화

            // 플레이어 비활성화
            if (!isReturn)
            {
                GameManager.Instance.SetPlayerActive(false);
            }

            // 기존 씬 언로드
            Scene currentScene = SceneManager.GetActiveScene();
            await SceneManager.UnloadSceneAsync(currentScene);

            string key = target.ToString();     // 타겟 씬

            _handle = Addressables.LoadSceneAsync(key, LoadSceneMode.Additive, false);  // 수동으로 제어

            await _handle.Task; // 씬 전환 준비 완료

            await _handle.Result.ActivateAsync();   // 씬 전환 직접 호출

            //await UniTask.Delay(2000);

            await Addressables.UnloadSceneAsync(_loadingHandle);    // 로딩 씬 언로드

            if (isReturn)
            {
                FieldUIController.Instance.ToggleFieldCanvas(true);
                InputManager.Instance.SwitchToPlayerMap();

                GameManager.Instance.SetPlayerActive(true);
                GameManager.Instance.TogglePlayerMoveController(true);

                FieldUIController.Instance.UpdateStatus();

                await UniTask.Delay(_delay);

                await FadeInOut.Instance.FadeOutAsync();
            }

            string clipKey = key + "BGM";
            AudioManager.Instance.PlayBGM(clipKey);     // 배경음악 재생
            _isLoading = false;

            if (!isReturn)
            {
                OnBattleSceneLoaded?.Invoke();

            }
        }

        catch (Exception e)
        {
            Debug.LogError($"Error : {e.Message}");
        }
    }

    public void LoadTargetScene(string target, bool isReturn)
    {
        LoadTargetSceneAsync(ConvertStringToESceneId(target), isReturn).Forget();
    }
}
