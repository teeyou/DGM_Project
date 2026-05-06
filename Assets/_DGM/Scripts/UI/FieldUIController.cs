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

    [SerializeField] private GameObject _questDetailGo;
    [SerializeField] private SlideAnim _slideAnim;

    [SerializeField] private GameObject _questGo;
    [SerializeField] private GameObject _questCloseGo;

    [SerializeField] private GameObject _interactOuter;
    [SerializeField] private GameObject _interactDialogue;
    [SerializeField] private GameObject _interactCombat;

    [SerializeField] private GameObject _dialoguePanelGo;
    [SerializeField] private TMP_Text _dialogueText;

    public int _dialogueIndex = 0;

    private InputManager _input;

    private bool _isQPressed = false;

    private bool _isShowQuestDetail = false;

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
        _input.OnQuestOpen -= SetQPressed;
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
}
