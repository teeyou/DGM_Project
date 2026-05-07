using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum ENPC
{
    CherubimonGood
}

public class FieldUIController : Singleton<FieldUIController>
{
    [SerializeField] private GameObject _menuGo;
    //[SerializeField] private RectTransform _cursorRt;

    [SerializeField] private GameObject _questDetailGo;
    [SerializeField] private SlideAnim _slideAnim;

    [SerializeField] private GameObject _questGo;
    [SerializeField] private GameObject _questCloseGo;
    [SerializeField] private TMP_Text _questTitle;
    [SerializeField] private TMP_Text _questDescription;

    [SerializeField] private GameObject _interactOuter;
    [SerializeField] private GameObject _interactDialogue;
    [SerializeField] private GameObject _interactCombat;

    [SerializeField] private GameObject _dialoguePanelGo;
    [SerializeField] private TMP_Text _dialogueText;
    [SerializeField] private GameObject _dialogueFKeyOuter;

    public int _dialogueIndex = 0;

    private InputManager _input;

    private bool _isQPressed = false;

    private bool _isShowQuestDetail = false;

    private Quest currentQuest = null;
    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);

        _questGo.SetActive(true);
        _questCloseGo.SetActive(false);
    }

    void Start()
    {
        _input = InputManager.Instance;

        if (_input == null)
        {
            Debug.LogError("_input NULL");
            return;
        }

        _input.OnQuestOpen += SetQPressed;
    }

    private void OnDestroy()
    {
        if (_input != null)
        {
            _input.OnQuestOpen -= SetQPressed;
        }
    }

    private void SetQPressed(bool isPressed)
    {
        _isQPressed = isPressed;

        HandleQuestInput(_isQPressed);
    }

    public void HandleQuestInput(bool isPressed)
    {
        if (!isPressed)
            return;

        _isQPressed = false;

        ShowCurrentQuest();

        if (_isShowQuestDetail)
        {
            _questGo.SetActive(true);
            _questCloseGo.SetActive(false);
            
            _slideAnim.SlideOut();
            _isShowQuestDetail = false;
        }
        else
        {
            _questGo.SetActive(false);
            _questCloseGo.SetActive(true);

            _slideAnim.SlideIn();
            _isShowQuestDetail = true;
        }
    }

    public void ToggleInteractButton(bool enabled)
    {
        _interactDialogue.SetActive(enabled);
        _interactOuter.SetActive(enabled);
    }

    public void ToggleInteractCombatButton(bool enabled)
    {
        _interactCombat.SetActive(enabled);
        _interactOuter.SetActive(enabled);
    }

    public void ShowDialogue(List<string> dialogueList, string _npcName)
    {
        _interactOuter.SetActive(false);

        if (!_dialoguePanelGo.activeSelf)
        {
            ToggleDialoguePanel(true);
        }

        if (_dialogueIndex < dialogueList.Count)
        {
            _dialogueText.text = dialogueList[_dialogueIndex++];
        }
        else
        {
            _dialoguePanelGo.SetActive(false);
            _dialogueIndex = 0;

            if (_npcName == ENPC.CherubimonGood.ToString())
            {
                if (GameManager.Instance.HasDigimon)
                    return;
                
                ToggleMenu(true);
            }

            else
            {

            }

        }
    }

    public void ToggleMenu(bool enabled)
    {
        _menuGo.SetActive(enabled);
    }

    public void ToggleDialoguePanel(bool enabled)
    {
        _dialogueIndex = 0;
        _dialoguePanelGo.SetActive(enabled);
    }

    //public void InitCursorPos()
    //{
    //    _cursorRt.anchoredPosition = new Vector2(-600f, -300f);
    //}

    public void ShowMessage(string msg)
    {
        ShowMessageAsync(msg).Forget();
    }

    private async UniTaskVoid ShowMessageAsync(string msg)
    {
        _dialogueFKeyOuter.SetActive(false);
        _dialoguePanelGo.SetActive(true);
        _dialogueText.text = msg;
        
        await UniTask.Delay(2000);

        _dialoguePanelGo.SetActive(false);
        _dialogueFKeyOuter.SetActive(true);
    }

    public void ShowCurrentQuest()
    {
        Quest currentQuest = QuestManager.Instance.GetCurrentQuest();
        if (currentQuest == null)
        {
            _questTitle.text = "";
            _questDescription.text = "Äù½ºÆ® ¾øÀ½";
        }
        else
        {
            _questTitle.text = currentQuest.Title;
            _questDescription.text = currentQuest.Description;
        }
    }
}
