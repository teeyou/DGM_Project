using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class PlayerSpawner : MonoBehaviour, ISpawner
{
    //public event Action<GameObject> OnPlayerSpawned;

    [SerializeField] private EventChannel _eventChannel;
    [SerializeField] private Transform _spawnPoint;

    private string _key = "Player";
    private GameObject _playerGo;

    private CancellationTokenSource _cts;
    private CancellationToken _token;

    private bool _isSpawning = false;

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

    void Start()
    {
        if (!GameManager.Instance.IsExistsPlayer())
            Spawn(_spawnPoint.position, _spawnPoint.rotation);
    }

    public void Spawn(Vector3 pos, Quaternion rot)
    {
        // 콜백 사용
        //Addressables.InstantiateAsync(_key, pos, rot).Completed += InstantiatePlayer;

        if (_isSpawning)
            return;

        SpawnAsync(pos, rot, _token).Forget();
    }

    private async UniTaskVoid SpawnAsync(Vector3 pos, Quaternion rot, CancellationToken token)
    {
        _isSpawning = true;

        AsyncOperationHandle<GameObject> handle;
        try
        {
            handle = Addressables.InstantiateAsync(_key, pos, rot);

            await handle.Task.AsUniTask().AttachExternalCancellation(token);

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                _playerGo = handle.Result;
                _eventChannel.SpawnedPlayer(_playerGo);
                //OnPlayerSpawned?.Invoke(_playerGo);
            }

            else
            {
                Debug.Log("플레이어 생성 실패");
            }
        }

        catch (OperationCanceledException)
        {
            
        }

        catch (Exception e)
        {
            Debug.Log($"에러 : {e.Message} ");
        }

        _isSpawning = false;
    }

    private void OnDestroy()
    {
        //if (_playerGo != null)
        //    Addressables.ReleaseInstance( _playerGo );
    }
}
