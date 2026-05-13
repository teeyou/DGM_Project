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


    [SerializeField] private InputActionReference _battleLeft;
    [SerializeField] private InputActionReference _battleRight;
    [SerializeField] private InputActionReference _battleEvo;         //Evo
    [SerializeField] private InputActionReference _battleAttack;         //Attack
    [SerializeField] private InputActionReference _battleSkill;         //Skill
    [SerializeField] private InputActionReference _battleGuard;         //Guard
    [SerializeField] private InputActionReference _battleSelect;         //Select
    [SerializeField] private InputActionReference _battleRun;         //Run

    [SerializeField] private InputActionAsset _inputActions;

    private InputActionMap _playerMap;
    private InputActionMap _menuMap;
    private InputActionMap _battleMap;

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

    public event Action<bool> OnEvo;
    public event Action<bool> OnAttack;
    public event Action<bool> OnSkill;
    public event Action<bool> OnGuard;
    public event Action<bool> OnRun;

    public void ShowCurrentMap()
    {
        foreach (var map in _inputActions.actionMaps)
        {
            if (map.enabled)
            {
                Debug.Log("현재 활성화된 Action Map: " + map.name);
            }
        }
    }
    protected override void Awake()
    {
        base.Awake();

        DontDestroyOnLoad(gameObject);

        TryBind();

        _playerMap = _inputActions.FindActionMap("Player");
        _menuMap = _inputActions.FindActionMap("Menu UI");
        _battleMap = _inputActions.FindActionMap("Battle");
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

        if (_battleLeft == null || _battleLeft.action == null)
            return;

        if (_battleRight == null || _battleRight.action == null)
            return;

        if (_battleEvo == null || _battleEvo.action == null)
            return;

        if (_battleAttack == null || _battleAttack.action == null)
            return;

        if (_battleSkill == null || _battleSkill.action == null)
            return;

        if (_battleGuard == null || _battleGuard.action == null)
            return;

        if (_battleSelect == null || _battleSelect.action == null)
            return;

        if (_battleRun == null || _battleRun.action == null)
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

        _battleLeft.action.started += OnMoveLeft;
        _battleRight.action.started += OnMoveRight;
        _battleEvo.action.started += OnEvoStarted;
        _battleAttack.action.started += OnAttackStarted;
        _battleSkill.action.started += OnSkillStarted;
        _battleGuard.action.started += OnGuardStarted;
        _battleSelect.action.started += OnSelectStarted;
        _battleRun.action.started += OnRunStarted;

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

        if (_battleLeft != null && _battleLeft.action != null)
        {
            _battleLeft.action.started -= OnMoveLeft;
        }

        if (_battleRight != null && _battleRight.action != null)
        {
            _battleRight.action.started -= OnMoveRight;
        }

        if (_battleEvo != null && _battleEvo.action != null)
        {
            _battleEvo.action.started -= OnEvoStarted;
        }

        if (_battleAttack != null && _battleAttack.action != null)
        {
            _battleAttack.action.started -= OnAttackStarted;
        }

        if (_battleSkill != null && _battleSkill.action != null)
        {
            _battleSkill.action.started -= OnSkillStarted;
        }

        if (_battleGuard != null && _battleGuard.action != null)
        {
            _battleGuard.action.started -= OnGuardStarted;
        }

        if (_battleSelect != null && _battleSelect.action != null)
        {
            _battleSelect.action.started -= OnSelectStarted;
        }

        if (_battleRun != null && _battleRun.action != null)
        {
            _battleRun.action.started -= OnRunStarted;
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
            _questOpen.action.Enable();
            _interact.action.Enable();
            _esc.action.Enable();
            EnableMenuUI(true);
            EnableBattle(true);
        }

        else
        {
            _move.action.Disable();
            _mouseRightClick.action.Disable();
            _zoom.action.Disable();
            _questOpen.action.Disable();
            _interact.action.Disable();
            _esc.action.Disable();
            EnableMenuUI(false);
            EnableBattle(false);
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

    private void OnEvoStarted(InputAction.CallbackContext context)
    {
        OnEvo?.Invoke(true);
    }

    private void OnAttackStarted(InputAction.CallbackContext context)
    {
        OnAttack?.Invoke(true);
    }

    private void OnSkillStarted(InputAction.CallbackContext context)
    {
        OnSkill?.Invoke(true);
    }

    private void OnGuardStarted(InputAction.CallbackContext context)
    {
        OnGuard?.Invoke(true);
    }

    private void OnRunStarted(InputAction.CallbackContext context)
    {
        OnRun?.Invoke(true);
    }
   
    public void SwitchToPlayerMap()
    {
        _playerMap.Enable();
        _menuMap.Disable();
        _battleMap.Disable();
    }

    public void SwitchToMenuUIMap()
    {
        _playerMap.Disable();
        _menuMap.Enable();
        _battleMap.Disable();
    }

    public void SwitchToBattleMap()
    {
        _playerMap.Disable();
        _menuMap.Disable();
        _battleMap.Enable();
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

    public void EnableBattle(bool enabled)
    {
        if (!_isBind)
            return;

        if (enabled)
        {
            _battleLeft.action.Enable();
            _battleRight.action.Enable();
            _battleEvo.action.Enable();
            _battleAttack.action.Enable();
            _battleSkill.action.Enable();
            _battleGuard.action.Enable();
            _battleSelect.action.Enable();
            _battleRun.action.Enable();
        }

        else
        {
            _battleLeft.action.Disable();
            _battleRight.action.Disable();
            _battleEvo.action.Disable();
            _battleAttack.action.Disable();
            _battleSkill.action.Disable();
            _battleGuard.action.Disable();
            _battleSelect.action.Disable();
            _battleRun.action.Disable();
        }
    }

}
