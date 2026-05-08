using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UIElements;

public class DigimonFollow : MonoBehaviour
{
    [SerializeField] private float _rotationSharpness = 10f;

    private const float MUST_STOP_OFFSET = 2f;
    private const float STOP_OFFSET = 3f;
    private const float MOVE_OFFSET = 4f;
    private const float TELEPORT_OFFSET = 10f;
    private Transform _playerTr;

    private float _groundStick = -2f;
    private CharacterController _cc;
    private float _gravity = -9.81f;

    private Vector3 _velocity;

    private CancellationTokenSource _cts;
    private CancellationToken _token;

    private Animator _animator;
    private float _speed;

    private bool _isMoving = false;
    private Coroutine _stopRoutine = null;

    private void Awake()
    {
        _animator = GetComponent<Animator>();

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

    void Start()
    {
        SetPlayer().Forget();

        _cc = GetComponent<CharacterController>();
        _cc.skinWidth = 0.01f;
        _cc.height = 2f;
        _cc.center = new Vector3(0f, 1f, 0f);

        if (_cc == null)
        {
            Debug.LogError("cc NULL");
        }
    }

    void Update()
    {
        if (_playerTr == null)
            return;

        TickGravity();

        //transform.position = new Vector3(transform.position.x, _playerTr.position.y, transform.position.z);

        float dist = Vector3.Distance(transform.position, _playerTr.position);

        if (dist < MUST_STOP_OFFSET)
        {
            if (_stopRoutine != null)
            {
                StopCoroutine(_stopRoutine);
                _stopRoutine = null;
            }

            _isMoving = false;
            _animator.SetBool("Move", false);
        }

        if (!_isMoving && dist > MOVE_OFFSET)
        {
            _isMoving = true;
            _animator.SetBool("Move", true);
        }
        else if (_isMoving && dist < STOP_OFFSET)
        {
            //_isMoving = false;
            //_animator.SetBool("Move", false);

            if (_stopRoutine == null)
            {
                _stopRoutine = StartCoroutine(CoDelayStop());
            }

            else
            {
                StopCoroutine(_stopRoutine);
                _stopRoutine = null;

                _stopRoutine = StartCoroutine(CoDelayStop());
            }
        }

        if (_isMoving)
        {
            if (dist > TELEPORT_OFFSET)
            {
                _isMoving = false;
                _animator.SetBool("Move", false);
                transform.rotation = _playerTr.rotation;
                transform.position = _playerTr.position + transform.forward * -STOP_OFFSET;
                return;
            }

            Vector3 dir = _playerTr.position - transform.position;
            dir.y = 0f;
            dir.Normalize();

            //transform.forward = Vector3.Lerp(transform.forward, dir, Time.deltaTime * 10f);
            float t = 1f - Mathf.Exp(-_rotationSharpness * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), t);

            Vector3 velocity = dir * _speed + _velocity;
            _cc.Move(velocity * Time.deltaTime);
            //transform.position += dir * _speed * Time.deltaTime;
        }

        else
        {
            _cc.Move(_velocity * Time.deltaTime);
        }
    }

    private void TickGravity()
    {
        if (_cc.isGrounded)
        {
            if (_velocity.y < 0f)
            {
                _velocity.y = _groundStick;
            }
        }

        _velocity.y += _gravity * Time.deltaTime;
    }

    private IEnumerator CoDelayStop()
    {
        yield return new WaitForSeconds(0.5f);
        _isMoving = false;
        _animator.SetBool("Move", false);

        _stopRoutine = null;
    }

    private async UniTaskVoid SetPlayer()
    {
        await UniTask.WaitUntil(() => GameManager.Instance.IsExistsPlayer(), PlayerLoopTiming.Update, _token);

        _playerTr = GameManager.Instance.GetPlayer().transform;
        MoveController playerMoveController = _playerTr.GetComponent<MoveController>();

        _speed = playerMoveController != null ? playerMoveController.MoveSpeed : 3f;
        _speed *= 0.9f;
    }
}
