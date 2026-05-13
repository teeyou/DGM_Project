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
    [SerializeField] private float _groundStick;
    private float _gravity = -9.81f;
    //private float _verticalVel; // CC -> 리지드바디를 사용 안하기 때문에 직접 y속도 누적해서 사용
    private Vector3 _velocity;

    private Vector2 _inputVec;
    private Vector3 _moveDir;
    private CharacterController _cc;
    private Animator _animator;

    private float _idleTimer = 0f;
    private bool _isDancing = false;

    private CancellationTokenSource _cts;

    private InputManager _input;

    private IInteractable _interactable = null;
    private bool _isFPressed = false;
    private bool _isEscPressed = false;

    public float MoveSpeed => _moveSpeed;

    private void Awake()
    {
        _cc = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();

        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        _inputVec = Vector3.zero;

        _cts = new CancellationTokenSource();

        if (_input == null)
        {
            Debug.Log("input NULL - BindInputManagerAsync 실행");
            BindInputManagerAsync(_cts.Token).Forget();
        }
        else
        {
            Debug.Log("_input 존재");
            _input.SwitchToPlayerMap();
            _input.OnMove += HandleMove;
            _input.OnInteract += HandleInteract;
            _input.OnEsc += HandleEsc;
        }

        if (_input != null)
            _input.ShowCurrentMap();
    }

    private void OnDisable()
    {
        if (_cts != null)
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }

        _input.OnMove -= HandleMove;
        _input.OnInteract -= HandleInteract;
        _input.OnEsc -= HandleEsc;
    }

    private async UniTask BindInputManagerAsync(CancellationToken token)
    {
        try
        {
            await UniTask.WaitUntil(() => InputManager.Instance != null, PlayerLoopTiming.Update, token);

            _input = InputManager.Instance;
            _input.OnMove += HandleMove;
            _input.OnInteract += HandleInteract;
            _input.OnEsc += HandleEsc;
            _input.SwitchToPlayerMap();

            Debug.Log("BindInputManagerAsync 완료");
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
        TickGravity();

        if (_inputVec == null)
        {
            Debug.Log("_inputVec NULL");
            return;
        }

        _moveDir = new Vector3(_inputVec.x, 0f, _inputVec.y);

        if (_moveDir != Vector3.zero)
        {
            SetRotation();
            _idleTimer = 0f;

            Vector3 velocity = _moveDir * _moveSpeed + _velocity;
            _cc.Move(velocity * Time.deltaTime);
        }
        
        else
        {
            _idleTimer += Time.deltaTime;
            _cc.Move(_velocity * Time.deltaTime);
        }

        PlayAnimation();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_interactable == null)
        {
            _interactable = other.transform.GetComponent<IInteractable>();
        }

        //if (_interactable != null)
        //{
        //    if (other.tag == "NPC")
        //    {
        //        FieldUIController.Instance.ToggleInteractButton(true);
        //    }
        //    else if (other.tag == "Enemy")
        //    {
        //        FieldUIController.Instance.ToggleInteractCombatButton(true);
        //    }
        //}
    }

    private void OnTriggerStay(Collider other)
    {
        if (_interactable != null)
        {
            if (other.tag == "NPC")
            {
                if (GameManager.Instance.IsPlayerInteracting)
                {
                    FieldUIController.Instance.ToggleInteractButton(false);
                }
                else
                {
                    FieldUIController.Instance.ToggleInteractButton(true);
                }
            }
            else if (other.tag == "Enemy")
            {
                FieldUIController.Instance.ToggleInteractCombatButton(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (_interactable != null)
        {
            FieldUIController.Instance.ToggleDialoguePanel(false);

            if (other.tag == "NPC")
            {
                FieldUIController.Instance.ToggleInteractButton(false);
            }
            else if (other.tag == "Enemy")
            {
                FieldUIController.Instance.ToggleInteractCombatButton(false);
            }

            _interactable = null;
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

    private void SetRotation()
    {
        float t = 1f - Mathf.Exp(-_sharpness * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(_moveDir), t);
    }

    private void PlayAnimation()
    {
        //float speed = _moveDir.magnitude * _moveSpeed;
        //float speed01 = Mathf.InverseLerp(0f, _moveSpeed, speed); // 0 ~ 1 사이로 변환
        //_animator.SetFloat("Speed", speed01);

        //if (speed == 0f)
        //{
        //    if (_idleTimer >= _idleTime)
        //    {
        //        if (_isDancing)
        //            return;

        //        _isDancing = true;
        //        _animator.SetTrigger("Dance");
        //    }
        //}

        //else
        //{
        //    _isDancing = false;
        //}

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
        _inputVec = Vector2.ClampMagnitude(v, 1.0f);
    }

    public void OnDanceFinished()
    {
        _animator.SetTrigger("Dance");
    }

    public void ResetCC()
    {
        _cc.enabled = false;
        _cc.enabled = true;
    }

    private void HandleInteract(bool isPressed)
    {
        _isFPressed = isPressed;
        if (GameManager.Instance.IsBlockInteractionKey)
            return;

        if (!_isFPressed)
            return;

        _isFPressed = false;

        if (_interactable != null)
        {
            _interactable.Interact(gameObject);
        }
    }

    private void HandleEsc(bool isPressed)
    {
        if (GameManager.Instance.IsPlayerInteracting)
            return;

        if (!isPressed)
            return;

        FieldUIController.Instance.ToggleGameMenu(true);
    }
}
