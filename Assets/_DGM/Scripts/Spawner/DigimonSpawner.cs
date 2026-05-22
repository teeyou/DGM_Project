using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.InputSystem;
using UnityEngine.ResourceManagement.AsyncOperations;

public class DigimonSpawner : Singleton<DigimonSpawner>
{
    //private const string STATUS_SUFFIX = "_Status";
    private const float FRIEND_OFFSET = 2f;

    private CancellationTokenSource _cts;
    private CancellationToken _token;

    private DigimonFactory _factory;

    private Dictionary<string, GameObject> _keyToPrefab = new Dictionary<string, GameObject>();
    private Dictionary<string, DigimonStatusSO> _keyToStatusSO = new Dictionary<string, DigimonStatusSO>();

    private List<AsyncOperationHandle> _handleList = new List<AsyncOperationHandle>();

    public event Action<GameObject, DigimonStatus> OnFriendDigimonSpawned;

    // 배틀 씬에서 사용
    public event Action<GameObject, DigimonStatus> OnPlayerDigimonSpawned;
    public event Action<GameObject, DigimonStatus> OnEnemyDigimonSpawned;

    protected override void Awake()
    {
        base.Awake();

        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        _cts = new CancellationTokenSource();
        _token = _cts.Token;
    }

    private void OnDisable()
    {
        if (_cts != null)
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }
    }

    private void Start()
    {
        _factory = DigimonFactory.Instance;
    }

    public void UnloadAll()
    {
        for (int i = 0; i < _handleList.Count; i++)
        {
            Addressables.Release(_handleList[i]);
        }

        _handleList.Clear();

        foreach (var kvp in _keyToPrefab)
        {
            if (kvp.Value != null)
                Destroy(kvp.Value);
        }

        _keyToPrefab.Clear();
    }

    public async UniTaskVoid SpawnFriendDigimon(int level, string key, CancellationToken token)
    {
        try
        {
            await UniTask.WaitUntil(() => GameManager.Instance.IsExistsPlayer(), PlayerLoopTiming.Update);
            Transform playerTr = GameManager.Instance.GetPlayer().transform;

            Vector3 pos = playerTr.position + playerTr.forward * -FRIEND_OFFSET;
            pos.y = 0f;
            Quaternion rot = playerTr.rotation;

            // 어드레서블에서 로드해서 스폰
            LoadFriendDigimon(level, key, pos, rot, token).Forget();

        }
        catch (OperationCanceledException)
        {

        }
        catch (Exception e)
        {
            Debug.LogError($"Error : {e.Message}");
        }
    }

    public void SpawnPlayerDigimon(int idx, string key, Vector3 pos, Quaternion rot)
    {
        if (_keyToPrefab.ContainsKey(key))
        {
            SpawnDigimon(idx, key, pos, rot);
            return;
        }

        LoadAndSpawnPlayerDigimon(idx, key, pos, rot).Forget();
    }

    private void SpawnDigimon(int idx, string key, Vector3 pos, Quaternion rot)
    {
        GameObject go = _factory.CreateDigimon(_keyToPrefab[key], pos, rot);
        Debug.Log(go.scene.name);

        DigimonStatus status = go.AddComponent<DigimonStatus>();

        status.Init(GameManager.Instance.GetDigimonStatus(idx));

        string typeName = key + "Attack";
        System.Type type = System.Type.GetType(typeName);

        if (type != null)
        {
            go.AddComponent(type);
        }
        else
        {
            Debug.LogError($"AddComponent {typeName} 실패 ");
        }

        OnPlayerDigimonSpawned?.Invoke(go, status);

    }

    private async UniTaskVoid LoadAndSpawnPlayerDigimon(int idx, string prefabKey, Vector3 pos, Quaternion rot)
    {
        try
        {
            AsyncOperationHandle<GameObject> prefabHandle = Addressables.LoadAssetAsync<GameObject>(prefabKey);

            await prefabHandle.Task;
            GameObject digimonPrefab = null;
            if (prefabHandle.Status == AsyncOperationStatus.Succeeded)
            {
                digimonPrefab = prefabHandle.Result;
            }

            if (digimonPrefab == null)
            {
                Debug.Log("디지몬 프리팹 로드 실패");
                return;
            }

            // 프리팹 캐싱
            _keyToPrefab[prefabKey] = digimonPrefab;
            _handleList.Add(prefabHandle);

            SpawnDigimon(idx, prefabKey, pos, rot);

            //GameObject digimonGo = _factory.CreateDigimon(digimonPrefab, pos, rot);

            //DigimonStatus status = digimonGo.AddComponent<DigimonStatus>();

            //status.Init(GameManager.Instance.GetDigimonStatus(idx));

            //OnPlayerDigimonSpawned?.Invoke(digimonGo, status);
        }

        catch (OperationCanceledException)
        {

        }
        catch (Exception e)
        {
            Debug.LogError($"Error! {e.Message}");
        }
    }

    public async UniTask SpawnEnemyDigimon(int id, string key, Vector3 pos, Quaternion rot, CancellationToken token = default)
    {
        if (_keyToPrefab.ContainsKey(key))
        {
            SpawnEnemy(id, key, pos, rot);
            return;
        }

        await LoadAndSpawnEnemyDigimon(id, key, pos, rot, token);
    }

    private async UniTask LoadFriendDigimon(int level, string prefabKey, Vector3 pos, Quaternion rot, CancellationToken token)
    {
        try
        {
            AsyncOperationHandle<GameObject> prefabHandle = Addressables.LoadAssetAsync<GameObject>(prefabKey);

            await prefabHandle.Task;

            GameObject digimonPrefab = null;
            if (prefabHandle.Status == AsyncOperationStatus.Succeeded)
            {
                digimonPrefab = prefabHandle.Result;
            }

            if (digimonPrefab == null)
            {
                Debug.Log("디지몬 프리팹 로드 실패");
                return;
            }

            _keyToPrefab[prefabKey] = digimonPrefab;
            _handleList.Add(prefabHandle);

            // 최초 스폰
            DigimonDB db = DigimonDB.Instance;
            StatusData data = db.GetStatusDataByName(prefabKey);

            GameObject digimonGo = _factory.CreateDigimon(digimonPrefab, pos, rot);

            DigimonStatus status = digimonGo.AddComponent<DigimonStatus>();
            status.Init(data, db.GetGrowthType(data.GrowthType));           // string -> GrowthType 객체로 변환해서 초기화

            DigimonFollow follow = digimonGo.AddComponent<DigimonFollow>();
            CharacterController cc = digimonGo.AddComponent<CharacterController>();
            GameManager.Instance.FollowDigimon = digimonGo;

            OnFriendDigimonSpawned?.Invoke(digimonGo, status);  //GameManager에서 AddDigimon
        }

        catch (OperationCanceledException)
        {

        }
        catch (Exception e)
        {
            Debug.LogError($"Error! {e.Message}");
        }
    }

    private void SpawnEnemy(int id, string prefabKey, Vector3 pos, Quaternion rot)
    {
        DigimonDB db = DigimonDB.Instance;
        EnemyStatusData data = db.GetEnemyStatusDataById(id);

        GameObject digimonGo = _factory.CreateDigimon(_keyToPrefab[prefabKey], pos, rot);

        DigimonStatus status = digimonGo.AddComponent<DigimonStatus>();
        status.Init(data, db.GetGrowthType(data.GrowthType));               // string -> GrowthType 객체로 변환해서 초기화

        OnEnemyDigimonSpawned?.Invoke(digimonGo, status);
    }

    private async UniTask LoadAndSpawnEnemyDigimon(int id, string prefabKey, Vector3 pos, Quaternion rot, CancellationToken token)
    {
        try
        {
            AsyncOperationHandle<GameObject> prefabHandle = Addressables.LoadAssetAsync<GameObject>(prefabKey);

            await prefabHandle.Task;
            GameObject digimonPrefab = null;
            if (prefabHandle.Status == AsyncOperationStatus.Succeeded)
            {
                digimonPrefab = prefabHandle.Result;
            }

            if (digimonPrefab == null)
            {
                Debug.Log("디지몬 프리팹 로드 실패");
                return;
            }

            // 프리팹 캐싱
            _keyToPrefab[prefabKey] = digimonPrefab;
            _handleList.Add(prefabHandle);

            DigimonDB db = DigimonDB.Instance;
            EnemyStatusData data = db.GetEnemyStatusDataById(id);

            GameObject digimonGo = _factory.CreateDigimon(digimonPrefab, pos, rot);

            DigimonStatus status = digimonGo.AddComponent<DigimonStatus>();
            status.Init(data, db.GetGrowthType(data.GrowthType));                // string -> GrowthType 객체로 변환해서 초기화

            OnEnemyDigimonSpawned?.Invoke(digimonGo, status);
        }

        catch (OperationCanceledException)
        {

        }
        catch (Exception e)
        {
            Debug.LogError($"Error! {e.Message}");
        }
    }

}
