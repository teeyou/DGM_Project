using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public enum EMenuType
{
    DigimonSelect,
    GameMenu,
}

public class CursorHandler : MonoBehaviour
{
    [SerializeField] private Transform _cursorTr;

    [SerializeField] private float _cursorPosAnimDuration;
    [SerializeField] private float _intervalH;
    [SerializeField] private float _intervalV;
    [SerializeField] private bool _isHorizontal;

    [SerializeField] private EMenuType _menuType;

    private InputManager _input;
    private Vector2 _inputValue;
    private bool _isSelect = false;
    private int _cursorIndex = -1;
    private bool _isEscPressed = false;
    private Coroutine _cursorAnimRoutine = null;

    private CancellationTokenSource _cts;

    private Vector3 _startCursorPos;
    private void OnEnable()
    {
        if (_input == null)
            _input = InputManager.Instance;

        if (_input != null)
        {
            _input.SwitchToMenuUIMap();

            _input.OnMenuMove += SetMove;
            _input.OnSelect += SetSelect;
            _input.OnMenuEsc += HandleMenuEsc;
        }

        else
        {
            Debug.Log("CursorHandler : input NULL");
            SetInputManager().Forget();
        }

        _cts = new CancellationTokenSource();

        _startCursorPos = _cursorTr.position;

        _cursorIndex = 0;
        _inputValue = Vector2.zero;
        _isEscPressed = false;
        _isSelect = false;
}

    private async UniTaskVoid SetInputManager()
    {
        try
        {
            await UniTask.WaitUntil(() => InputManager.Instance != null);
            _input = InputManager.Instance;

            _input.SwitchToMenuUIMap();
            
            _input.OnMenuMove -= SetMove;
            _input.OnSelect -= SetSelect;
            _input.OnMenuEsc -= HandleMenuEsc;

            _input.OnMenuMove += SetMove;
            _input.OnSelect += SetSelect;
            _input.OnMenuEsc += HandleMenuEsc;
        }

        catch (Exception e)
        {
            Debug.LogError($"Error : {e.Message}");
        }
        
    }

    private void OnDisable()
    {
        if (_input != null)
        {
            _input.OnMenuMove -= SetMove;
            _input.OnSelect -= SetSelect;
            _input.OnMenuEsc -= HandleMenuEsc;
        }

        if (_cts != null)
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }

        _cursorTr.position = _startCursorPos;

        if (_cursorAnimRoutine != null)
        {
            StopCoroutine(_cursorAnimRoutine);
            _cursorAnimRoutine = null;
        }
    }

    private void Update()
    {
        if (_input == null)
            return;

        HandleMenuSelect();

        HandleMoveHorizontally();

        HandleMoveVertically();
    }

    private void SetSelect(bool isSelect)
    {
        _isSelect = isSelect;
    }

    private void SetMove(Vector2 v)
    {
        _inputValue = v;
        Debug.Log($"Cursor Move: {_inputValue}");
    }

    private void HandleMenuEsc(bool isPressed)
    {
        if (!isPressed)
            return;

        if (_menuType == EMenuType.GameMenu)
        {
            FieldUIController.Instance.ToggleGameMenu(false);
        }
    }

    public void HandleMenuSelect()
    {
        if (!_isSelect)
            return;

        _isSelect = false;

        switch (_menuType)
        {
            case EMenuType.DigimonSelect:
                HandleDigimonSelect();
                break;
            case EMenuType.GameMenu:
                HandleGameMenuSelect();
                break;
        }
    }

    public void HandleDigimonSelect()
    {
        // 0 : Vaccine  Agumon
        // 1 : Free     Veemon
        // 2 : Virus    Guilmon

        CancellationTokenSource linked = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, this.GetCancellationTokenOnDestroy());
        switch (_cursorIndex)
        {
            case 0:
                DigimonSpawner.Instance.SpawnFriendDigimon(1, "Agumon", linked.Token).Forget();
                break;
            case 1:
                DigimonSpawner.Instance.SpawnFriendDigimon(1, "Veemon", linked.Token).Forget();
                break;
            case 2:
                DigimonSpawner.Instance.SpawnFriendDigimon(1, "Guilmon", linked.Token).Forget();
                break;
        }

        GameManager.Instance.HasDigimon = true;

        FieldUIController.Instance.ToggleDigimonSelectMenu(false);
        GameManager.Instance.IsBlockInteractionKey = false;

        _input.SwitchToPlayerMap();

        GameManager.Instance.IsPlayerInteracting = false;
        FieldUIController.Instance.ToggleMenuButton(true);
        FieldUIController.Instance.ToggleQuestButton(true);
    }

    public void HandleGameMenuSelect()
    {
        switch (_cursorIndex)
        {
            case 0:
                ExitGame();
                break;
            case 1:
                ExitGame();
                break;
            case 2:
                ExitGame();
                break;
        }
    }

    private void ExitGame()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void HandleMoveHorizontally()
    {
        if (!_isHorizontal)
            return;

        if (_cursorIndex == -1)
            return;

        float currentX = _cursorTr.localPosition.x;
        if (_inputValue.x > 0)
        {
            if (_cursorIndex != 2)
            {
                if (_cursorAnimRoutine == null)
                {
                    if (gameObject.activeInHierarchy)
                    {
                        AudioManager.Instance.PlaySFX("CursorMoveSFX");
                        _cursorAnimRoutine = StartCoroutine(CoAnimatePositionHorizontally(_cursorPosAnimDuration, currentX, currentX + _inputValue.x * _intervalH));
                        _cursorIndex++;
                    }

                }
            }
            else
            {
                if (_cursorAnimRoutine == null)
                {
                    if (gameObject.activeInHierarchy)
                    {
                        AudioManager.Instance.PlaySFX("CursorMoveSFX");
                        _cursorAnimRoutine = StartCoroutine(CoAnimatePositionHorizontally(_cursorPosAnimDuration, currentX, currentX - _inputValue.x * _intervalH * 2));
                        _cursorIndex = 0;
                    }

                }
            }
        }

        else if (_inputValue.x < 0)
        {
            if (_cursorIndex != 0)
            {
                if (_cursorAnimRoutine == null)
                {

                    if (gameObject.activeInHierarchy)
                    {
                        AudioManager.Instance.PlaySFX("CursorMoveSFX");
                        _cursorAnimRoutine = StartCoroutine(CoAnimatePositionHorizontally(_cursorPosAnimDuration, currentX, currentX + _inputValue.x * _intervalH));
                        _cursorIndex--;
                    }

                }

            }
            else
            {
                if (_cursorAnimRoutine == null)
                {
                    if (gameObject.activeInHierarchy)
                    {
                        AudioManager.Instance.PlaySFX("CursorMoveSFX");
                        _cursorAnimRoutine = StartCoroutine(CoAnimatePositionHorizontally(_cursorPosAnimDuration, currentX, currentX - _inputValue.x * _intervalH * 2));
                        _cursorIndex = 2;
                    }

                }
            }
        }
    }

    public void HandleMoveVertically()
    {
        if (_isHorizontal)
            return;

        if (_cursorIndex == -1)
            return;

        float currentY = _cursorTr.localPosition.y;
        if (_inputValue.y > 0)
        {
            if (_cursorIndex != 0)
            {
                if (_cursorAnimRoutine == null)
                {

                    AudioManager.Instance.PlaySFX("CursorMoveSFX");
                    _cursorAnimRoutine = StartCoroutine(CoAnimatePositionVertically(_cursorPosAnimDuration, currentY, currentY + _inputValue.y * _intervalV));
                    _cursorIndex--;
                }
                else
                {
                    Debug.Log("Cursor animation in progress. Ignoring input.");
                }
            }
            else
            {
                if (_cursorAnimRoutine == null)
                {
                    AudioManager.Instance.PlaySFX("CursorMoveSFX");
                    _cursorAnimRoutine = StartCoroutine(CoAnimatePositionVertically(_cursorPosAnimDuration, currentY, currentY - _inputValue.y * _intervalV * 2));
                    _cursorIndex = 2;
                }
                else
                {
                    Debug.Log("Cursor animation in progress. Ignoring input.");
                }
            }
        }

        else if (_inputValue.y < 0)
        {
            if (_cursorIndex != 2)
            {
                if (_cursorAnimRoutine == null)
                {
                    AudioManager.Instance.PlaySFX("CursorMoveSFX");
                    _cursorAnimRoutine = StartCoroutine(CoAnimatePositionVertically(_cursorPosAnimDuration, currentY, currentY + _inputValue.y * _intervalV));
                    _cursorIndex++;
                }

            }
            else
            {
                if (_cursorAnimRoutine == null)
                {
                    AudioManager.Instance.PlaySFX("CursorMoveSFX");
                    _cursorAnimRoutine = StartCoroutine(CoAnimatePositionVertically(_cursorPosAnimDuration, currentY, currentY - _inputValue.y * _intervalV * 2));
                    _cursorIndex = 0;
                }
            }
        }
    }

    private IEnumerator CoAnimatePositionVertically(float duration, float start, float end)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float y = Mathf.Lerp(start, end, t);
            _cursorTr.transform.localPosition = new Vector3(_cursorTr.transform.localPosition.x, y, _cursorTr.transform.localPosition.z);
            yield return null;
        }

        _cursorTr.transform.localPosition = new Vector3(_cursorTr.transform.localPosition.x, end, _cursorTr.transform.localPosition.z);

        _cursorAnimRoutine = null;
    }

    private IEnumerator CoAnimatePositionHorizontally(float duration, float start, float end)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float x = Mathf.Lerp(start, end, t);
            _cursorTr.transform.localPosition = new Vector3(x, _cursorTr.transform.localPosition.y, _cursorTr.transform.localPosition.z);
            yield return null;
        }

        _cursorTr.transform.localPosition = new Vector3(end, _cursorTr.transform.localPosition.y, _cursorTr.transform.localPosition.z);

        _cursorAnimRoutine = null;
    }
}
