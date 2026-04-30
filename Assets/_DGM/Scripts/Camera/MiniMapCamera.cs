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
    [SerializeField] private float _height;

    private Transform _playerTr;
    private CancellationTokenSource _cts;
    private CancellationToken _token;

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
        FindPlayerAsync(_token).Forget();
    }

    private void SetPlayer(GameObject go)
    {
        _playerTr = go.transform;
    }

    private async UniTaskVoid FindPlayerAsync(CancellationToken token)
    {
        try
        {
            await UniTask.WaitUntil(() => GameManager.Instance.IsExistsPlayer(),PlayerLoopTiming.Update, _token);
            _playerTr = GameManager.Instance.GetPlayer().transform;

            //await UniTask.WaitUntil(() =>
            //{
            //    GameObject go = GameObject.FindGameObjectWithTag("Player");

            //    if (go != null)
            //    {
            //        _playerTr = go.transform;
            //    }
            //    return go != null;
            //},
            //PlayerLoopTiming.Update, token);

            //_playerTr = GameObject.FindGameObjectWithTag("Player").transform;
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

        transform.position = _playerTr.position + Vector3.up * _height;
    }
}
