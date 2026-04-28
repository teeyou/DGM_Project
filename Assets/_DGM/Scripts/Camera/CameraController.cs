using Cinemachine;
using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.InputSystem;
using UnityEngine.ResourceManagement.AsyncOperations;

public class CameraController : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera _vcam;
    
    private EventChannel _eventChannel;

    private Transform _target;

    private CancellationTokenSource _cts;

    private AsyncOperationHandle<EventChannel> _handle;

    private void OnEnable()
    {
        GameObject go = GameObject.FindGameObjectWithTag("Player");

        if (go != null)
        {
            SetPlayer(go);
        }

        // 마을에서 스폰 됐을 때 발생
        if (_target == null)
        {
            _handle = Addressables.LoadAssetAsync<EventChannel>("EventChannel");
            _handle.Completed += OnEventChannelLoaded;
        }

        //_cts = new CancellationTokenSource();
        //FindPlayer(_cts.Token).Forget();
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

    private void OnEventChannelLoaded(AsyncOperationHandle<EventChannel> handle)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            _eventChannel = handle.Result;
            _eventChannel.OnPlayerSpawned += SetPlayer;
            Debug.Log("SetPlayer 완료");
        }

        else
        {
            Debug.Log($"Status : {handle.Status}");
        }
    }

    private void SetPlayer(GameObject player)
    {
        _target = player.transform;
        _vcam.m_Follow = _target;
    }

    private async UniTaskVoid FindPlayer(CancellationToken token)
    {
        try
        {
            await UniTask.WaitUntil(() => GameObject.FindGameObjectWithTag("Player") != null, 
                PlayerLoopTiming.Update, token);
            
            _target = GameObject.FindGameObjectWithTag("Player").transform;

            _vcam.m_Follow = _target;
        }

        catch (OperationCanceledException)
        {

        }
        catch (Exception e)
        {
            Debug.Log($"Error : {e.Message}");
        }
    }

    
}
