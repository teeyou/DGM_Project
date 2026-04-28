using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class MiniMapCamera : Singleton<MiniMapCamera>
{
    [SerializeField] private PlayerSpawner _playerSpawner;

    private EventChannel _eventChannel;

    private Transform _playerTr;
    private CancellationTokenSource _cts;
    private CancellationToken _token;

    AsyncOperationHandle<EventChannel> _handle;

    private void OnEnable()
    {
        if (_playerTr == null)
        {
            _handle = Addressables.LoadAssetAsync<EventChannel>("EventChannel");
            _handle.Completed += OnEventChannelLoaded;
        }

        //_cts = new CancellationTokenSource();
        //_token = _cts.Token;
    }

    private void OnEventChannelLoaded(AsyncOperationHandle<EventChannel> handle)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            _eventChannel = handle.Result;
            _eventChannel.OnPlayerSpawned += SetPlayer;
        }
    }

    private void OnDisable()
    {
        if (_cts != null)
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }

        if (_eventChannel != null)
        {
            _eventChannel.OnPlayerSpawned -= SetPlayer;
        }

        if (_handle.IsValid())
        {
            Addressables.Release(_handle);
        }
    }

    void Start()
    {
        //FindPlayerAsync(_token).Forget();
    }

    private void SetPlayer(GameObject go)
    {
        _playerTr = go.transform;
    }

    private async UniTaskVoid FindPlayerAsync(CancellationToken token)
    {
        try
        {
            await UniTask.WaitUntil(() => GameObject.FindGameObjectWithTag("Player"),
                PlayerLoopTiming.Update, token);

            _playerTr = GameObject.FindGameObjectWithTag("Player").transform;
        }

        catch (OperationCanceledException)
        {

        }
        catch (Exception e)
        {
            Debug.Log($"Error : {e.Message}");
        }
        
    }
    private void LateUpdate()
    {
        if (_playerTr == null)
        {
            return;
        }

        transform.position = _playerTr.position + Vector3.up * 30f;
    }
}
