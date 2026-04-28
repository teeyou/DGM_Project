using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Device;
using UnityEngine.InputSystem;

public class TitleManager : MonoBehaviour
{
    [SerializeField] private GameObject _pressAnyKeyGo;
    [SerializeField] private GameObject _menuGo;
    [SerializeField] private Transform _cursorTr;
    [SerializeField] private TextSpacingAnim _textSpacingAnim;

    [SerializeField] private float _cursorPosAnimDuration;

    private bool _pressed = false;
    private bool _isShowAnyKey = false;

    private InputManager _input;
    private int _inputValue;
    private bool _isSelect = false;

    private int _cursorIndex = -1;
    private Coroutine _cursorAnimRoutine = null;

    private void Awake()
    {
        _pressAnyKeyGo.SetActive(false);
        _menuGo.SetActive(false);
    }

    private void OnEnable()
    {
        _textSpacingAnim.OnAnimationFinished += ShowPressAnyKeyText;
    }

    private void OnDisable()
    {
        _textSpacingAnim.OnAnimationFinished -= ShowPressAnyKeyText;

        _input.OnMenuMove -= SetMove;
        _input.OnSelect -= SetSelect;
    }

    private void Start()
    {
        _input = InputManager.Instance;
        _input.SwitchToMenuUIMap();
        
        _input.OnMenuMove += SetMove;
        _input.OnSelect += SetSelect;

        _input.EnableMenuUI(false); // 꺼두고, 메뉴 창 뜨면 켜기
    }

    private void SetSelect(bool isSelect)
    {
        _isSelect = isSelect;
    }

    private void SetMove(int v)
    {
        _inputValue = v;
    }

    private void ShowPressAnyKeyText()
    {
        _pressAnyKeyGo.SetActive(true);
        _isShowAnyKey = true;
    }

    private void Update()
    {
        HandleMenuSelect();

        HandleMenuMove();

        // AnyKey가 아직 안 떴으면 처리 안 함
        if (!_isShowAnyKey)
            return;

        // Any Key 이미 누른 상태면 처리 안 함
        if (_pressed)
            return;
        
        HandleAnyKey();

    }

    private void HandleMenuSelect()
    {
        if (_cursorIndex == -1)
            return;

        if (_isSelect)
        {
            _isSelect = false;

            switch ( _cursorIndex )
            {
                case 0:
                    StartGame();
                    break;
                case 1:
                    ShowOptions();
                    break;
                case 2:
                    Exit();
                    break;
            }
        }
    }

    private void StartGame()
    {
        SceneLoader.Instance.LoadScene(ESceneId.Title, ESceneId.Village).Forget();
    }

    private void ShowOptions()
    {

    }

    private void Exit()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif

    }

    private void HandleMenuMove()
    {
        float currentY = _cursorTr.localPosition.y;
        if (_inputValue > 0)
        {
            if (_cursorIndex != 0)
            {
                if (_cursorAnimRoutine == null)
                {
                    _cursorAnimRoutine = StartCoroutine(CoAnimatePosition(_cursorPosAnimDuration, currentY, currentY + _inputValue * 100));
                    _cursorIndex--;
                }
            }
            else
            {
                if (_cursorAnimRoutine == null)
                {
                    _cursorAnimRoutine = StartCoroutine(CoAnimatePosition(_cursorPosAnimDuration, currentY, currentY - _inputValue * 200));
                    _cursorIndex = 2;
                }
            }

            _inputValue = 0;
        }

        else if (_inputValue < 0)
        {
            if (_cursorIndex != 2)
            {
                if (_cursorAnimRoutine == null)
                {
                    _cursorAnimRoutine = StartCoroutine(CoAnimatePosition(_cursorPosAnimDuration, currentY, currentY + _inputValue * 100));
                    _cursorIndex++;
                }

            }
            else
            {
                if (_cursorAnimRoutine == null)
                {
                    _cursorAnimRoutine = StartCoroutine(CoAnimatePosition(_cursorPosAnimDuration, currentY, currentY - _inputValue * 200));
                    _cursorIndex = 0;
                }
            }

            _inputValue = 0;
        }
    }

    private IEnumerator CoAnimatePosition(float duration, float start, float end)
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

    private void HandleAnyKey()
    {
        if (Keyboard.current != null)
        {
            foreach (var key in Keyboard.current.allKeys)
            {
                if (key == null)
                    return;

                if (key.wasPressedThisFrame)
                {
                    _pressed = true;
                    OnAnyKeyPressed();
                    return;
                }
            }
        }
    }
    private void OnAnyKeyPressed()
    {
        // Press Any Key 비활성화
        // 메뉴 선택 창 활성화
        // 사용자 입력 활성화

        _pressAnyKeyGo.SetActive(false);
        _menuGo.SetActive(true);
        _input.EnableMenuUI(true);
        _cursorIndex = 0;
    }
}
