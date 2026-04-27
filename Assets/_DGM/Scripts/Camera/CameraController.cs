using Cinemachine;
using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera _vcam;

    private Transform _target;

    private InputManager _inputManager;

    private CancellationTokenSource _cts;

    private void OnEnable()
    {
        _cts = new CancellationTokenSource();
        FindPlayer(_cts.Token).Forget();
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

    private void OnDisable()
    {
        if (_cts != null)
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }
    }
}
