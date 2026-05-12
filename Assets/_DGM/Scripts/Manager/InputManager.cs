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
    [SerializeField] private InputActionReference _menuSelect;      // Enter
    [SerializeField] private InputActionReference _menuLeft;
    [SerializeField] private InputActionReference _menuRight;
    [SerializeField] private InputActionReference _menuInteract;    // F
    [SerializeField] private InputActionReference _menuEsc;        // ESC

    [SerializeField] private InputActionReference _esc;
    [SerializeField] private InputActionReference _questOpen;       // Q
    [SerializeField] private InputActionReference _interact;        // F


    [SerializeField] private InputActionReference _battleLeftArrow;
    [SerializeField] private InputActionReference _battleRightArrow;
    [SerializeField] private InputActionReference _battleA;         //Attack
    [SerializeField] private InputActionReference _battleS;         //Skill
    [SerializeField] private InputActionReference _battleD;         //Guard
    [SerializeField] private InputActionReference _battleF;         //Select
    [SerializeField] private InputActionReference _battleC;         //Cancel
    [SerializeField] private InputActionReference _battleR;         //Run

    [SerializeField] private InputActionAsset _inputActions;

    private InputActionMap _playerMap;
    private InputActionMap _menuMap;

    private bool _isBind = false;

    public event Action<Vector2> OnMove;

    public event Action<bool> OnMouseRightClick;
    public event Action<float> OnZoom;

    public event Action<Vector2> OnMenuMove;
    public event Action<bool> OnSelect;     //Enter

    public event Action<bool> OnQuestOpen;  //Q
    public event Action<bool> OnInteract;   //F

    public event Action<bool> OnEsc;        //ESC
    public event Action<bool> OnMenuEsc;        //ESC

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

        if (_menuLeft == null || _menuLeft.action == null)
            return;

        if (_menuRight == null || _menuRight.action == null)
            return;

        if (_menuInteract == null || _menuInteract.action == null)
            return;

        if (_menuEsc == null || _menuEsc.action == null)
            return;

        if (_questOpen == null || _questOpen.action == null)
            return;

        if (_interact == null || _interact.action == null)
            return;

        if (_esc == null || _esc.action == null)
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

        _menuLeft.action.started += OnMoveLeft;
        _menuLeft.action.canceled += OnMoveLeftCanceled;

        _menuRight.action.started += OnMoveRight;
        _menuRight.action.canceled += OnMoveRightCanceled;

        _menuInteract.action.started += OnInteractStarted;
        _menuInteract.action.canceled += OnInteractCanceled;

        _menuEsc.action.started += OnMenuEscStarted;
        _menuEsc.action.canceled += OnMenuEscCanceled;

        _questOpen.action.started += OnQuestOpenStarted;
        _questOpen.action.canceled += OnQuestOpenCanceled;

        _interact.action.started += OnInteractStarted;
        _interact.action.canceled += OnInteractCanceled;

        _esc.action.started += OnEscStarted;
        _esc.action.canceled += OnEscCanceled;
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
            _menuUp.action.canceled -= OnMoveUpCanceled;
        }

        if (_menuDown != null && _menuDown.action != null)
        {
            _menuDown.action.started -= OnMoveDown;
            _menuDown.action.canceled -= OnMoveDownCanceled;
        }

        if (_menuSelect != null && _menuSelect.action != null)
        {
            _menuSelect.action.started -= OnSelectStarted;
            _menuSelect.action.canceled -= OnSelectCanceled;
        }

        if (_menuLeft != null && _menuLeft.action != null)
        {
            _menuLeft.action.started -= OnMoveLeft;
            _menuLeft.action.canceled -= OnMoveLeftCanceled;
        }

        if (_menuRight != null && _menuRight.action != null)
        {
            _menuRight.action.started -= OnMoveRight;
            _menuRight.action.canceled -= OnMoveRightCanceled;
        }

        if (_menuInteract != null && _menuInteract.action != null)
        {
            _menuInteract.action.started -= OnInteractStarted;
            _menuInteract.action.canceled -= OnInteractCanceled;
        }

        if (_questOpen != null && _questOpen.action != null)
        {
            _questOpen.action.started -= OnQuestOpenStarted;
            _questOpen.action.canceled -= OnQuestOpenCanceled;
        }

        if (_interact != null && _interact.action != null)
        {
            _interact.action.started -= OnInteractStarted;
            _interact.action.canceled -= OnInteractCanceled;
        }

        if (_esc != null && _esc.action != null)
        {
            _esc.action.started -= OnEscStarted;
            _esc.action.canceled -= OnEscCanceled;
        }

        if (_menuEsc != null && _menuEsc.action != null)
        {
            _menuEsc.action.started -= OnMenuEscStarted;
            _menuEsc.action.canceled -= OnMenuEscCanceled;
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
            _menuLeft.action.Enable();
            _menuRight.action.Enable();
            _menuInteract.action.Enable();
            _menuEsc.action.Enable();
            _questOpen.action.Enable();
            _interact.action.Enable();
            _esc.action.Enable();
        }

        else
        {
            _move.action.Disable();
            _mouseRightClick.action.Disable();
            _zoom.action.Disable();
            _menuUp.action.Disable();
            _menuDown.action.Disable();
            _menuSelect.action.Disable();
            _menuLeft.action.Disable();
            _menuRight.action.Disable();
            _menuInteract.action.Disable();
            _menuEsc.action.Disable();
            _questOpen.action.Disable();
            _interact.action.Disable();
            _esc.action.Disable();
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
        OnMenuMove?.Invoke(new Vector2(0f, 1f));
    }

    private void OnMoveDown(InputAction.CallbackContext context)
    {
        OnMenuMove?.Invoke(new Vector2(0f, -1f));
    }

    private void OnMoveUpCanceled(InputAction.CallbackContext context)
    {
        OnMenuMove?.Invoke(Vector2.zero);
    }

    private void OnMoveDownCanceled(InputAction.CallbackContext context)
    {
        OnMenuMove?.Invoke(Vector2.zero);
    }

    private void OnMoveLeft(InputAction.CallbackContext context)
    {
        OnMenuMove?.Invoke(new Vector2(-1f, 0f));
    }

    private void OnMoveRight(InputAction.CallbackContext context)
    {
        OnMenuMove?.Invoke(new Vector2(1f, 0f));
    }

    private void OnMoveLeftCanceled(InputAction.CallbackContext context)
    {
        OnMenuMove?.Invoke(Vector2.zero);
    }

    private void OnMoveRightCanceled(InputAction.CallbackContext context)
    {
        OnMenuMove?.Invoke(Vector2.zero);
    }


    private void OnSelectStarted(InputAction.CallbackContext context)
    {
        OnSelect?.Invoke(true);
    }

    private void OnSelectCanceled(InputAction.CallbackContext context)
    {
        OnSelect?.Invoke(false);
    }

    private void OnQuestOpenStarted(InputAction.CallbackContext context)
    {
        //bool isOpen = context.ReadValueAsButton();
        OnQuestOpen?.Invoke(true);
    }

    private void OnQuestOpenCanceled(InputAction.CallbackContext context)
    {
        //bool isOpen = context.ReadValueAsButton();
        OnQuestOpen?.Invoke(false);
    }

    private void OnInteractStarted(InputAction.CallbackContext context)
    {
        OnInteract?.Invoke(true);
    }

    private void OnInteractCanceled(InputAction.CallbackContext context)
    {
        OnInteract?.Invoke(false);
    }

    private void OnEscStarted(InputAction.CallbackContext context)
    {
        OnEsc?.Invoke(true);
    }

    private void OnEscCanceled(InputAction.CallbackContext context)
    {
        OnEsc?.Invoke(false);
    }

    private void OnMenuEscStarted(InputAction.CallbackContext context)
    {
        OnMenuEsc?.Invoke(true);
    }

    private void OnMenuEscCanceled(InputAction.CallbackContext context)
    {
        OnMenuEsc?.Invoke(false);
    }

    public void SwitchToPlayerMap()
    {
        if (_menuMap.enabled)
        {
            _menuMap.Disable();
            _playerMap.Enable();
        }
    }

    public void SwitchToMenuUIMap()
    {
        if (_playerMap.enabled)
        {
            _playerMap.Disable();
            _menuMap.Enable();
        }
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
            _menuLeft.action.Enable();
            _menuRight.action.Enable();
            _menuInteract.action.Enable();
            _menuEsc.action.Enable();
        }

        else
        {
            _menuUp.action.Disable();
            _menuDown.action.Disable();
            _menuSelect.action.Disable();
            _menuLeft.action.Disable();
            _menuRight.action.Disable();
            _menuInteract.action.Disable();
            _menuEsc.action.Disable();
        }
    }

}
