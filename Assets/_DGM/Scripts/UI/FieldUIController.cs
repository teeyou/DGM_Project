using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum ENPC
{
    CherubimonGood
}

public class FieldUIController : Singleton<FieldUIController>
{
    [SerializeField] private GameObject _HUDCanvas;
    [SerializeField] private GameObject _MiniMapCanvas;

    [SerializeField] private GameObject _digimonSelectMenuGo;
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

    [SerializeField] private GameObject _gameMenuGo;
    [SerializeField] private GameObject _gameMenuButtonGo;
    [SerializeField] private GameObject _questButtonGo;

    [SerializeField] private GameObject _digimonStatusPanel;
    [SerializeField] private List<GameObject> _digimonStatusList;
    [SerializeField] private SlideAnim _digimonStatusSlideAnim;
    private bool _isShowDigimonStatus = false;
    
    private bool _isShowMessage = false;

    public int _dialogueIndex = 0;

    private InputManager _input;

    private bool _isQPressed = false;

    private bool _isShowQuestDetail = false;

    public bool IsShowDigimonStatus => _isShowDigimonStatus;

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
                {
                    InputManager.Instance.SwitchToPlayerMap();
                    GameManager.Instance.IsPlayerInteracting = false;
                    ToggleMenuButton(true);
                    ToggleQuestButton(true);
                    return;
                }

                ToggleDigimonSelectMenu(true);
            }

            else
            {

            }

        }
    }

    public void ToggleDigimonSelectMenu(bool enabled)
    {
        if (enabled)
        {
            GameManager.Instance.IsBlockInteractionKey = true;
        }

        _digimonSelectMenuGo.SetActive(enabled);
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
        if (_isShowMessage)
            return;

        ShowMessageAsync(msg).Forget();
    }

    private async UniTaskVoid ShowMessageAsync(string msg)
    {
        _isShowMessage = true;

        _dialogueFKeyOuter.SetActive(false);
        _dialoguePanelGo.SetActive(true);
        _dialogueText.text = msg;
        
        await UniTask.Delay(2000);

        _dialoguePanelGo.SetActive(false);
        _dialogueFKeyOuter.SetActive(true);

        _isShowMessage = false;
    }

    public void ShowCurrentQuest()
    {
        Quest currentQuest = QuestManager.Instance.GetCurrentQuest();
        if (currentQuest == null)
        {
            _questTitle.text = "";
            _questDescription.text = "퀘스트 없음";
        }
        else
        {
            _questTitle.text = currentQuest.Title;
            _questDescription.text = currentQuest.Description;
        }
    }

    public void ToggleGameMenu(bool enabled)
    {
        Debug.Log($"ToggleGameMenu enabled : {enabled}");
        Debug.Log($"_gameMenuGo.activeSelf : {_gameMenuGo.activeSelf}");
        if (!_gameMenuGo.activeSelf)
        {
            _gameMenuGo.SetActive(true);
            _input.SwitchToMenuUIMap();
        }
        else
        {
            _gameMenuGo.SetActive(false);
            _input.SwitchToPlayerMap();
        }
    }

    public void ToggleMenuButton(bool enabled)
    {
        _gameMenuButtonGo.SetActive(enabled);
    }

    public void ToggleQuestButton(bool enabled)
    {
        _questButtonGo.SetActive(enabled);
    }

    public void ToggleHUDCanvas(bool enabled)
    {
        _HUDCanvas.SetActive(enabled);
        Debug.Log($"_HUDCanvas : {enabled}");
    }

    public void ToggleMiniMapCanvas(bool enabled)
    {
        _MiniMapCanvas.SetActive(enabled);
        Debug.Log($"_MiniMapCanvas : {enabled}");
    }

    public void ToggleFieldCanvas(bool enabled)
    {
        Debug.Log($"ToggleFieldCanvas : {enabled}");
        ToggleHUDCanvas(enabled);
        ToggleMiniMapCanvas(enabled);
    }

    public void ToggleDigimonStatus()
    {
        IReadOnlyList<DigimonStatus> list = GameManager.Instance.GetDigimonStatusList();

        if (list == null || list.Count <= 0)
        {
            ShowMessage("보유한 디지몬이 없습니다.\n퀘스트를 완료하세요.");
            return;
        }

        if (_isShowDigimonStatus)
        {
            _digimonStatusSlideAnim.SlideOut();
            _isShowDigimonStatus = false;
            //_digimonStatusPanel.SetActive(false);
        }
        else
        {
            _isShowDigimonStatus = true;
            for (int i = 0; i < _digimonStatusList.Count; i++)
            {
                if (i < list.Count)
                {
                    SetContents(_digimonStatusList[i], i);
                    _digimonStatusList[i].SetActive(true);

                    Debug.Log($"{i}번째 리스트 패널 활성화");
                }
                else
                {
                    _digimonStatusList[i].SetActive(false);

                    Debug.Log($"{i}번째 리스트 패널 비활성화");
                }
            }

            //_digimonStatusPanel.SetActive(true);
            _digimonStatusSlideAnim.SlideIn();
        }
    }

    public void UpdateStatus()
    {
        // 씬로더에서 isReturn일 때 호출
        // 화면에 보여지는 상태일때만 업데이트
        if (!_isShowDigimonStatus)
            return;

        IReadOnlyList<DigimonStatus> list = GameManager.Instance.GetDigimonStatusList();

        for (int i = 0; i < _digimonStatusList.Count; i++)
        {
            if (i < list.Count)
            {
                SetContents(_digimonStatusList[i], i);
            }
        }
    }

    private void SetContents(GameObject statusPanel, int idx)
    {
        TMP_Text[] texts = statusPanel.GetComponentsInChildren<TMP_Text>();
        Image[] imgs = statusPanel.GetComponentsInChildren<Image>();
        DigimonStatus status = GameManager.Instance.GetDigimonStatus(idx);

        // 텍스트 세팅
        for (int i = 0; i < texts.Length; i++)
        {
            if (texts[i].name == "Name")
            {
                texts[i].text = $"{status.DigimonNameKor}";
            }

            else if (texts[i].name == "Level")
            {
                texts[i].text = $"레벨 : {status.Level.ToString()}";
            }

            else if (texts[i].name == "Grade")
            {
                texts[i].text = $"등급 : {status.GradeKor}";
            }

            else if (texts[i].name == "Attr")
            {
                string attr = status.Attr.ToString().Substring(0, 2);
                string colorCode = ColorTable.GetColor(attr);
                texts[i].text = $"속성 : <color={colorCode}>{attr}</color>";
            }

            else if (texts[i].name == "Type")
            {
                texts[i].text = $"타입 : {status.TypeKor}";
            }

            else if (texts[i].name == "Growth")
            {
                texts[i].text = $"성장 : {status.GrowthTypeKor}";
            }

            else if (texts[i].name == "HP")
            {
                texts[i].text = $"HP : {status.HP}";
            }

            else if (texts[i].name == "ATK")
            {
                texts[i].text = $"ATK : {status.ATK}";
            }

            else if (texts[i].name == "DEF")
            {
                texts[i].text = $"DEF : {status.DEF}";
            }

            else if (texts[i].name == "INT")
            {
                texts[i].text = $"INT : {status.INT}";
            }

            else if (texts[i].name == "SPD")
            {
                texts[i].text = $"SPD : {status.SPD}";
            }

            else if (texts[i].name == "EXP")
            {
                texts[i].text = $"{status.EXP} / {status.RequiredEXP}";
            }
        }

        // 이미지 세팅
        for (int i = 0; i < imgs.Length; i++)
        {
            if (imgs[i].name == "Sprite")
            {
                imgs[i].sprite = DigimonDB.Instance.GetDigimonSprite(status.DigimonName);
            }

            else if (imgs[i].name == "ExpFront")
            {
                imgs[i].fillAmount = (float)status.EXP / (float)status.RequiredEXP;
            }
        }
    }
}
