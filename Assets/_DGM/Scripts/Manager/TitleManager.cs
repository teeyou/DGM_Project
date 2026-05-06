using Cysharp.Threading.Tasks;
using System.Collections;
using UnityEditor;
using UnityEngine;
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
    private Vector2 _inputValue;
    private bool _isSelect = false;

    private int _cursorIndex = -1;
    private Coroutine _cursorAnimRoutine = null;

    private void Awake()
    {
        _pressAnyKeyGo.SetActive(false);
        _menuGo.SetActive(false);

        AddressablesHelper.UpdateCatalogsAsync().Forget();
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

    private void SetMove(Vector2 v)
    {
        _inputValue = v;
    }

    private void ShowPressAnyKeyText()
    {
        WaitSFXLoad();

        _pressAnyKeyGo.SetActive(true);
        _isShowAnyKey = true;
    }

    private void WaitSFXLoad()
    {
        Debug.Log("BGM, SFX 로드 기다리는 중");
        UniTask.WaitUntil(() => AudioManager.Instance.SFXLoaded);
        UniTask.WaitUntil(() => AudioManager.Instance.BGMLoaded);
        Debug.Log("BGM SFX 로드 완료 Press Any Key 활성화");
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
            AudioManager.Instance.PlaySFX("SelectSFX");
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

    public void StartGame()
    {
        SceneLoader.Instance.LoadScene(ESceneId.Title, ESceneId.Village).Forget();
    }

    public void ShowOptions()
    {

    }

    public void Exit()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif

    }

    private void HandleMenuMove()
    {
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
                    _cursorAnimRoutine = StartCoroutine(CoAnimatePosition(_cursorPosAnimDuration, currentY, currentY + _inputValue.y * 100));
                    _cursorIndex--;
                }
            }
            else
            {
                if (_cursorAnimRoutine == null)
                {
                    AudioManager.Instance.PlaySFX("CursorMoveSFX");
                    _cursorAnimRoutine = StartCoroutine(CoAnimatePosition(_cursorPosAnimDuration, currentY, currentY - _inputValue.y * 200));
                    _cursorIndex = 2;
                }
            }

            //_inputValue = 0;
        }

        else if (_inputValue.y < 0)
        {
            if (_cursorIndex != 2)
            {
                if (_cursorAnimRoutine == null)
                {
                    AudioManager.Instance.PlaySFX("CursorMoveSFX");
                    _cursorAnimRoutine = StartCoroutine(CoAnimatePosition(_cursorPosAnimDuration, currentY, currentY + _inputValue.y * 100));
                    _cursorIndex++;
                }

            }
            else
            {
                if (_cursorAnimRoutine == null)
                {
                    AudioManager.Instance.PlaySFX("CursorMoveSFX");
                    _cursorAnimRoutine = StartCoroutine(CoAnimatePosition(_cursorPosAnimDuration, currentY, currentY - _inputValue.y * 200));
                    _cursorIndex = 0;
                }
            }

            //_inputValue = 0;
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
        if (!_pressed && Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
        {
            _pressed = true;
            OnAnyKeyPressed();
        }
    }
    private void OnAnyKeyPressed()
    {
        // Press Any Key 비활성화
        // 메뉴 선택 창 활성화
        // 사용자 입력 활성화
        AudioManager.Instance.PlaySFX("SelectSFX");
        _pressAnyKeyGo.SetActive(false);
        _menuGo.SetActive(true);
        _input.EnableMenuUI(true);
        _cursorIndex = 0;
    }
}
