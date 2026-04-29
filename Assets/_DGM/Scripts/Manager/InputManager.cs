using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : Singleton<InputManager>
{
    [SerializeField] private InputActionReference _move;
    [SerializeField] private InputActionReference _mouseRightClick;
    [SerializeField] private InputActionReference _zoom;

    [SerializeField] private InputActionReference _menuUp;
    [SerializeField] private InputActionReference _menuDown;
    [SerializeField] private InputActionReference _menuSelect;

    [SerializeField] private InputActionAsset _inputActions;

    private InputActionMap _playerMap;
    private InputActionMap _menuMap;

    private bool _isBind = false;

    public event Action<Vector2> OnMove;

    public event Action<bool> OnMouseRightClick;
    public event Action<float> OnZoom;

    public event Action<int> OnMenuMove;
    public event Action<bool> OnSelect;

    protected override void Awake()
    {
        base.Awake();

        DontDestroyOnLoad(gameObject);

        TryBind();

        _playerMap = _inputActions.FindActionMap("Player");
        _menuMap = _inputActions.FindActionMap("Menu UI");
    }

    private void OnDestroy()
    {
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

        if (_menuUp == null || _menuUp.action == null)
            return;

        if (_menuDown == null || _menuDown.action == null)
            return;

        if (_menuSelect == null || _menuSelect.action == null)
            return;

        _move.action.performed += OnMovePerformed;
        _move.action.canceled += OnMoveCanceled;

        _mouseRightClick.action.performed += OnMouseRightClickPerformed;
        _mouseRightClick.action.canceled += OnMouseRightClickCanceled;

        _zoom.action.performed += OnZoomPerformed;

        _menuUp.action.started += OnMoveUp;
        _menuUp.action.canceled += OnMoveUpCanceled;

        _menuDown.action.started += OnMoveDown;
        _menuDown.action.canceled += OnMoveDownCanceled;

        _menuSelect.action.started += OnSelectStarted;


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

        if (_menuUp != null && _menuUp.action != null)
        {
            _menuUp.action.started -= OnMoveUp;
            _menuUp.action.canceled += OnMoveUpCanceled;
        }

        if (_menuDown != null && _menuDown.action != null)
        {
            _menuDown.action.started -= OnMoveDown;
            _menuDown.action.canceled += OnMoveDownCanceled;
        }

        if (_menuSelect != null && _menuSelect.action != null)
        {
            _menuSelect.action.started -= OnSelectStarted;
            _menuSelect.action.canceled -= OnSelectCanceled;
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
            _menuUp.action.Enable();
            _menuDown.action.Enable();
            _menuSelect.action.Enable();
        }

        else
        {
            _move.action.Disable();
            _mouseRightClick.action.Disable();
            _zoom.action.Disable();
            _menuUp.action.Disable();
            _menuDown.action.Disable();
            _menuSelect.action.Disable();
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

    private void OnMoveUp(InputAction.CallbackContext context)
    {
        OnMenuMove?.Invoke(1);
    }

    private void OnMoveDown(InputAction.CallbackContext context)
    {
        OnMenuMove?.Invoke(-1);
    }

    private void OnMoveUpCanceled(InputAction.CallbackContext context)
    {
        OnMenuMove?.Invoke(0);
    }

    private void OnMoveDownCanceled(InputAction.CallbackContext context)
    {
        OnMenuMove?.Invoke(0);
    }


    private void OnSelectStarted(InputAction.CallbackContext context)
    {
        OnSelect?.Invoke(true);
    }

    private void OnSelectCanceled(InputAction.CallbackContext context)
    {
        OnSelect?.Invoke(false);
    }
    
    public void SwitchToPlayerMap()
    {
        _menuMap.Disable();
        _playerMap.Enable();
    }

    public void SwitchToMenuUIMap()
    {
        _playerMap.Disable();
        _menuMap.Enable();
    }

    public void EnableMenuUI(bool enabled)
    {
        if (!_isBind)
            return;

        if (enabled)
        {
            _menuUp.action.Enable();
            _menuDown.action.Enable();
            _menuSelect.action.Enable();
        }

        else
        {
            _menuUp.action.Disable();
            _menuDown.action.Disable();
            _menuSelect.action.Disable();
        }
    }

}
