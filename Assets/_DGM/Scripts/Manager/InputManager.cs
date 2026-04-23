using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : Singleton<InputManager>
{
    [SerializeField] private InputActionReference _move;
    [SerializeField] private InputActionReference _mouseRightClick;
    [SerializeField] private InputActionReference _zoom;

    private bool _isBind = false;

    public event Action<Vector2> OnMove;
    public event Action<bool> OnMouseRightClick;
    public event Action<float> OnZoom;

    protected override void Awake()
    {
        base.Awake();

        DontDestroyOnLoad(gameObject);

        TryBind();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        Unbind();
    }

    private void OnEnable()
    {
        // Awake는 최초 한 번만 실행되기 때문에 여기서도 Bind()
        TryBind();

        EnableActions(true);
    }

    private void OnDisable()
    {
        // 여기에서 UnBind() 하면 피바람이 불 수 있다.
        // 오브젝트가 꺼지면 입력도 끈다.
        EnableActions(false);
    }

    private void TryBind()
    {
        if (_isBind)
            return;

        if (_move == null || _move.action == null)
            return;

        if (_mouseRightClick == null || _mouseRightClick.action == null)
            return;

        if (_zoom == null || _zoom.action == null)
            return;

        _move.action.performed += OnMovePerformed;
        _move.action.canceled += OnMoveCanceled;

        _mouseRightClick.action.performed += OnMouseRightClickPerformed;
        _mouseRightClick.action.canceled += OnMouseRightClickCanceled;

        _zoom.action.performed += OnZoomPerformed;
        _isBind = true;
    }

    private void Unbind()
    {
        if (!_isBind)
            return;
        
        if (_move != null && _move.action != null)
        {
            _move.action.performed -= OnMovePerformed;
            _move.action.canceled -= OnMoveCanceled;
        }

        if (_mouseRightClick != null && _mouseRightClick.action != null)
        {
            _mouseRightClick.action.performed -= OnMouseRightClickPerformed;
            _mouseRightClick.action.canceled -= OnMouseRightClickCanceled;
        }

        if (_zoom != null && _zoom.action != null)
        {
            _zoom.action.performed -= OnZoomPerformed;
        }

        _isBind = false;
    }

    private void EnableActions(bool enabled)
    {
        if (!_isBind)
            return;

        if (enabled)
        {
            _move.action.Enable();
            _mouseRightClick.action.Enable();
            _zoom.action.Enable();
        }

        else
        {
            _move.action.Disable();
            _mouseRightClick.action.Disable();
            _zoom.action.Disable();
        }
    }

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        Vector2 moveVec = context.ReadValue<Vector2>();

        OnMove?.Invoke(moveVec);
    }

    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        OnMove?.Invoke(Vector2.zero);
    }

    private void OnMouseRightClickPerformed(InputAction.CallbackContext context)
    {
        OnMouseRightClick?.Invoke(true);
    }

    private void OnMouseRightClickCanceled(InputAction.CallbackContext context)
    {
        OnMouseRightClick?.Invoke(false);
    }

    private void OnZoomPerformed(InputAction.CallbackContext context)
    {
        Vector2 scroll = context.ReadValue<Vector2>();
        OnZoom?.Invoke(scroll.y);
    }
}
