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

    public event Action<GameObject,bool> OnDigimonSpawned;

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

        //SpawnDigimon("Koromon", new Vector3(-7f, 0f, -13f), Quaternion.identity, _token, this.GetCancellationTokenOnDestroy());
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

    private void Spawn(int level, string key, Vector3 pos, Quaternion rot, bool isEnemy)
    {
        // 최초 스폰
        DigimonDB db = DigimonDB.Instance;
        StatusData data = db.GetStatusDataByName(key);

        GameObject digimonGo = _factory.CreateDigimon(_keyToPrefab[key], pos, rot);
        DigimonStatus status = digimonGo.GetComponent<DigimonStatus>();
        status.Init(data, db.GetGrowthType(data.GrowthType), db.GetEvoTreeById(data.ID));
        //status.Init(_keyToStatusSO[key]);
        status.Level = level;

        if (!isEnemy)
        {
            DigimonFollow follow = digimonGo.AddComponent<DigimonFollow>();
        }

        OnDigimonSpawned?.Invoke(digimonGo, isEnemy);
    }

    public async UniTaskVoid SpawnFriendDigimon(int level, string key, CancellationToken token)
    {
        try
        {
            Debug.Log("SpawnFriendDigimon Start");
            await UniTask.WaitUntil(() => GameManager.Instance.IsExistsPlayer(), PlayerLoopTiming.Update);
            Transform playerTr = GameManager.Instance.GetPlayer().transform;

            Debug.Log("playerTr 로드 완료");

            Vector3 pos = playerTr.position + playerTr.forward * -FRIEND_OFFSET;
            pos.y = 0f;
            Quaternion rot = playerTr.rotation;

            if (_keyToPrefab.ContainsKey(key) && _keyToStatusSO.ContainsKey(key))
            {
                Debug.Log("딕셔너리에서 로드");
                Spawn(level, key, pos, rot, false);
                return;
            }

            // 캐시된게 없으면 어드레서블에서 로드해서 스폰
            await LoadDigimon(level, key, pos, rot, token, false);

        }
        catch (OperationCanceledException)
        {

        }
        catch (Exception e)
        {
            Debug.LogError($"Error : {e.Message}");
        }
    }

    public void SpawnDigimon(int level, string key, Vector3 pos, Quaternion rot, CancellationToken token)
    {
        if (_keyToPrefab.ContainsKey(key) && _keyToStatusSO.ContainsKey(key))
        {
            Spawn(level, key, pos, rot, true);
            return;
        }

        LoadDigimon(level, key, pos, rot, token, true).Forget();
    }

    private async UniTask LoadDigimon(int level, string digimonKey, Vector3 pos, Quaternion rot, CancellationToken token, bool isEnemy)
    {
        try
        {
            AsyncOperationHandle<GameObject> prefabHandle = Addressables.LoadAssetAsync<GameObject>(digimonKey);

            //await prefabHandle.Task.AsUniTask().AttachExternalCancellation(token);
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

            _keyToPrefab[digimonKey] = digimonPrefab;
            _handleList.Add(prefabHandle);

            // 최초 스폰
            DigimonDB db = DigimonDB.Instance;
            StatusData data = db.GetStatusDataByName(digimonKey);

            Debug.Log("LoadDigimon StatusData 로드 완료");
            Debug.Log($"data : {data.KorName}");

            GameObject digimonGo = _factory.CreateDigimon(digimonPrefab, pos, rot);

            DigimonStatus status = digimonGo.GetComponent<DigimonStatus>();

            if (status == null)
                status = digimonGo.AddComponent<DigimonStatus>();

            if (status != null)
            {
                status.Init(data, db.GetGrowthType(data.GrowthType), db.GetEvoTreeById(data.ID));
                status.Level = level;
            }

            if (!isEnemy)
            {
                DigimonFollow follow = digimonGo.AddComponent<DigimonFollow>();
                CharacterController cc = digimonGo.AddComponent<CharacterController>();
            }

            OnDigimonSpawned?.Invoke(digimonGo, isEnemy);
            Debug.Log("디지몬 최초 스폰 완료");
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
