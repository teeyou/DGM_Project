using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class DigimonFollow : MonoBehaviour
{
    private const float OFFSET = 2f;

    private Transform _playerTr;

    private CancellationTokenSource _cts;
    private CancellationToken _token;

    private Animator _animator;
    private float _speed;

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
    }

    void Update()
    {
        if (_playerTr == null)
            return;

        transform.position = new Vector3(transform.position.x, 0f, transform.position.z);

        float dist = Vector3.Distance(transform.position, _playerTr.position);

        if (dist < OFFSET)
        {
            _animator.SetBool("Move", false);
            return;
        }

        else
        {
            // 너무 멀리 떨어져있으면 플레이어 뒤로 이동
            if (dist > 10f)
            {
                _animator.SetBool("Move", false);
                transform.rotation = _playerTr.rotation;
                transform.position = _playerTr.position + transform.forward * -OFFSET;
                return;
            }

            Vector3 dir = _playerTr.position - transform.position;
            dir.y = 0f;
            dir.Normalize();

            transform.forward = Vector3.Lerp(transform.forward, dir, Time.deltaTime * 10f);
            
            transform.position += dir * _speed * Time.deltaTime;
            
            _animator.SetBool("Move", true);
        }
    }

    private async UniTaskVoid SetPlayer()
    {
        await UniTask.WaitUntil(() => GameManager.Instance.IsExistsPlayer(), PlayerLoopTiming.Update, _token);

        _playerTr = GameManager.Instance.GetPlayer().transform;
        MoveController playerMoveController = _playerTr.GetComponent<MoveController>();

        _speed = playerMoveController != null ? playerMoveController.MoveSpeed : 3f;

        Debug.Log("DigimonFollow : 플레이어 세팅 완료");
    }
}
