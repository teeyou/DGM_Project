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
    [SerializeField] private float _sensitivity;
    [SerializeField] private CinemachineVirtualCamera _vcam;

    [SerializeField] private float _minPitch;
    [SerializeField] private float _maxPitch;

    [SerializeField] private float zoomSpeed;
    [SerializeField] private float minZoom;
    [SerializeField] private float maxZoom;

    private float _pitch, _yaw;
    private bool _isRightClicked = false;
    private Transform _target;

    private InputManager _inputManager;

    private CancellationTokenSource _cts;

    private CinemachineTransposer _transposer;
    private void Awake()
    {
        _transposer = _vcam.GetCinemachineComponent<CinemachineTransposer>();
    }

    private void OnEnable()
    {
        if (InputManager.Instance != null)
        {
            _inputManager = InputManager.Instance;
            _inputManager.OnZoom += HandleZoom;
            _inputManager.OnMouseRightClick += OnMouseRightClicked;
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

    void LateUpdate()
    {
        //if (_target == null)
        //    return;

        //if (_isRightClicked)
        //{
        //    Vector2 delta = Mouse.current.delta.ReadValue();
        //    _yaw += delta.x * _sensitivity * Time.deltaTime;
        //    _pitch -= delta.y * _sensitivity * Time.deltaTime;
        //    _pitch = Mathf.Clamp(_pitch, _minPitch, _maxPitch);
        //}


        //transform.LookAt(_target);
    }

    private void OnMouseRightClicked(bool clicked)
    {
        _isRightClicked = clicked;
    }

    private void HandleZoom(float value)
    {
        Vector3 offset = _transposer.m_FollowOffset;
        float scale = 1f - value * zoomSpeed;
        scale = Mathf.Clamp(scale, minZoom, maxZoom);

        offset *= scale;

        _transposer.m_FollowOffset = offset;
    }
}
