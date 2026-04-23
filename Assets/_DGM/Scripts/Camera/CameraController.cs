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

    //private CinemachineTransposer _transposer;
    private void Awake()
    {
        //_transposer = _vcam.GetCinemachineComponent<CinemachineTransposer>();
    }

    private void OnEnable()
    {
        if (InputManager.Instance != null)
        {
            _inputManager = InputManager.Instance;
        }

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
