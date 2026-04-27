using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class MiniMapCamera : Singleton<MiniMapCamera>
{
    [SerializeField] private PlayerSpawner _playerSpawner;

    private Transform _playerTr;
    private CancellationTokenSource _cts;
    private CancellationToken _token;

    private void OnEnable()
    {
        _playerSpawner.OnPlayerSpawned += SetPlayer;

        _cts = new CancellationTokenSource();
        _token = _cts.Token;
    }

    private void OnDisable()
    {
        _playerSpawner.OnPlayerSpawned -= SetPlayer;

        if (_cts != null)
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
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
            return;

        transform.position = _playerTr.position + Vector3.up * 30f;
    }
}
