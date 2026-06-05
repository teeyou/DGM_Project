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
    [SerializeField] private List<RectTransform> _menuButtons; // 버튼 RectTransform 리스트

    [SerializeField] private RectTransform _cursorTr;

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

        // 시작 시 커서를 첫 번째 버튼 위치에 붙이기
        if (_menuButtons.Count > 0)
        {
            _cursorIndex = 0;
            _cursorTr.anchoredPosition = _menuButtons[_cursorIndex].anchoredPosition;
        }

        _startCursorPos = _cursorTr.anchoredPosition;

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

        if (_inputValue != Vector2.zero)
        {
            HandleMoveHorizontally();
            HandleMoveVertically();

            // 입력을 한 번 처리했으면 초기화
            _inputValue = Vector2.zero;
        }
    }

    private void HandleMenuEsc(bool isPressed)
    {
        if (!isPressed)
            return;

        if (_menuType == EMenuType.GameMenu)
        {
            Debug.Log("CursorHandler - HandleMenuEsc");
            FieldUIController.Instance.ToggleSettingsPanel(false);
            FieldUIController.Instance.ToggleGameMenu(false);

            FadeInOut.Instance.TogglePanel(true);
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

        QuestManager.Instance.QuestCheckList[0] = true; // 첫번째 퀘스트 완료
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
                FadeInOut.Instance.TogglePanel(false);
                FieldUIController.Instance.ToggleSettingsPanel(true);
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
                        _cursorAnimRoutine = StartCoroutine(CoAnimatePositionHorizontally(_cursorPosAnimDuration, currentX, currentX + _inputValue.x * 600));
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
                        _cursorAnimRoutine = StartCoroutine(CoAnimatePositionHorizontally(_cursorPosAnimDuration, currentX, currentX - _inputValue.x * 1200));
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
                        _cursorAnimRoutine = StartCoroutine(CoAnimatePositionHorizontally(_cursorPosAnimDuration, currentX, currentX + _inputValue.x * 600));
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
                        _cursorAnimRoutine = StartCoroutine(CoAnimatePositionHorizontally(_cursorPosAnimDuration, currentX, currentX - _inputValue.x * 1200));
                        _cursorIndex = 2;
                    }

                }
            }
        }
    }

    //public void HandleMoveVertically()
    //{
    //    if (_isHorizontal)
    //        return;

    //    if (_cursorIndex == -1)
    //        return;

    //    float currentY = _cursorTr.localPosition.y;
    //    if (_inputValue.y > 0)
    //    {
    //        if (_cursorIndex != 0)
    //        {
    //            if (_cursorAnimRoutine == null)
    //            {

    //                AudioManager.Instance.PlaySFX("CursorMoveSFX");
    //                _cursorAnimRoutine = StartCoroutine(CoAnimatePositionVertically(_cursorPosAnimDuration, currentY, currentY + _inputValue.y * 100));
    //                _cursorIndex--;
    //            }
    //            else
    //            {
    //                Debug.Log("Cursor animation in progress. Ignoring input.");
    //            }
    //        }
    //        else
    //        {
    //            if (_cursorAnimRoutine == null)
    //            {
    //                AudioManager.Instance.PlaySFX("CursorMoveSFX");
    //                _cursorAnimRoutine = StartCoroutine(CoAnimatePositionVertically(_cursorPosAnimDuration, currentY, currentY - _inputValue.y * 200));
    //                _cursorIndex = 2;
    //            }
    //            else
    //            {
    //                Debug.Log("Cursor animation in progress. Ignoring input.");
    //            }
    //        }
    //    }

    //    else if (_inputValue.y < 0)
    //    {
    //        if (_cursorIndex != 2)
    //        {
    //            if (_cursorAnimRoutine == null)
    //            {
    //                AudioManager.Instance.PlaySFX("CursorMoveSFX");
    //                _cursorAnimRoutine = StartCoroutine(CoAnimatePositionVertically(_cursorPosAnimDuration, currentY, currentY + _inputValue.y * 100));
    //                _cursorIndex++;
    //            }

    //        }
    //        else
    //        {
    //            if (_cursorAnimRoutine == null)
    //            {
    //                AudioManager.Instance.PlaySFX("CursorMoveSFX");
    //                _cursorAnimRoutine = StartCoroutine(CoAnimatePositionVertically(_cursorPosAnimDuration, currentY, currentY - _inputValue.y * 200));
    //                _cursorIndex = 0;
    //            }
    //        }
    //    }
    //}

    public void HandleMoveVertically()
    {
        if (FieldUIController.Instance.SettingsPanel == null)
        {
            AudioManager.Instance.RegisterSettingsPanel();
        }

        if (FieldUIController.Instance.SettingsPanel.activeSelf)
            return;

        if (_isHorizontal || _cursorIndex == -1) return;

        if (_inputValue.y > 0) // 위로 이동
        {
            int newIndex = (_cursorIndex == 0) ? _menuButtons.Count - 1 : _cursorIndex - 1;
            MoveCursor(newIndex);
        }
        else if (_inputValue.y < 0) // 아래로 이동
        {
            int newIndex = (_cursorIndex == _menuButtons.Count - 1) ? 0 : _cursorIndex + 1;
            MoveCursor(newIndex);
        }
    }

    private void MoveCursor(int newIndex)
    {
        if (newIndex < 0 || newIndex >= _menuButtons.Count) return;

        AudioManager.Instance.PlaySFX("CursorMoveSFX");

        if (_cursorAnimRoutine != null)
            StopCoroutine(_cursorAnimRoutine);

        _cursorAnimRoutine = StartCoroutine(CoAnimatePosition(
            _cursorPosAnimDuration,
            _cursorTr.anchoredPosition,
            _menuButtons[newIndex].anchoredPosition
        ));

        _cursorIndex = newIndex;
    }

    private IEnumerator CoAnimatePosition(float duration, Vector2 start, Vector2 end)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            Vector2 pos = Vector2.Lerp(start, end, t);
            _cursorTr.anchoredPosition = pos;
            yield return null;
        }

        _cursorTr.anchoredPosition = end;
        _cursorAnimRoutine = null;
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
