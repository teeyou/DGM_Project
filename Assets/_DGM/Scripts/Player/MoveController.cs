using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

public class MoveController : MonoBehaviour
{
    [SerializeField] private float _moveSpeed;
    [SerializeField] private float _sharpness;
    [SerializeField] private float _idleTime;

    private Vector2 _inputVec;
    private Vector3 _moveDir;
    private CharacterController _cc;
    private Animator _animator;

    private float _idleTimer = 0f;
    private bool _isDancing = false;

    private CancellationTokenSource _cts;

    private InputManager _inputManager;
    private void Awake()
    {
        _cc = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();
    }

    //public void OnMove(InputValue value)
    //{
    //    _inputVec = value.Get<Vector2>();
    //}

    private void OnEnable()
    {
        _cts = new CancellationTokenSource();

        BindInputManagerAsync(_cts.Token).Forget();
    }

    private void OnDisable()
    {
        if (_cts != null)
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }

        _inputManager.OnMove -= HandleMove;
    }

    private async UniTask BindInputManagerAsync(CancellationToken token)
    {
        try
        {
            await UniTask.WaitUntil(() => InputManager.Instance != null, PlayerLoopTiming.Update, token);

            _inputManager = InputManager.Instance;
            _inputManager.OnMove += HandleMove;
        }
        catch (OperationCanceledException)
        {

        }
        catch (Exception e)
        {
            Debug.Log($"Error : {e.Message}");
        }
    }

    void Update()
    {
        if (_inputVec == null)
        {
            Debug.Log("_inputVec NULL");
            return;
        }

        _moveDir = new Vector3(_inputVec.x, 0f, _inputVec.y);

        if (_moveDir != Vector3.zero)
        {
            SetDirection();
            _idleTimer = 0f;
        }
        
        else
        {
            _idleTimer += Time.deltaTime;
        }
        
        _cc.Move(_moveDir * _moveSpeed * Time.deltaTime);

        if (transform.position.y > 0)
        {
            Vector3 pos = transform.position;
            pos.y = 0f;
            transform.position = pos;
        }
        
        PlayAnimation();
    }

    private void SetDirection()
    {
        float t = 1f - Mathf.Exp(-_sharpness * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(_moveDir), t);
    }

    private void PlayAnimation()
    {
        if (_moveDir == Vector3.zero)
        {
            if (_idleTimer >= _idleTime)
            {
                if (_isDancing)
                    return;

                _isDancing = true;
                _animator.SetTrigger("Dance");
            }

            else
            {
                _animator.SetBool("Move", false);
                _isDancing = false;
            }
            
        }

        else
        {
            _animator.SetBool("Move", true);
            _isDancing = false;
        }
    }

    private void HandleMove(Vector2 v)
    {
        //_inputVec = v;
        _inputVec = Vector2.ClampMagnitude(v, 1.0f);
    }

    public void OnDanceFinished()
    {
        _animator.SetTrigger("Dance");
    }
}
