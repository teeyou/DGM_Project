using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
using static UnityEngine.ParticleSystem;

public class BattleUIManager : Singleton<BattleUIManager>
{
    [SerializeField] private float _heightOffset;   // 공격모드에서 타겟 넘버 오프셋

    [SerializeField] private List<GameObject> _numberList;
    
    [SerializeField] private GameObject _attackPanel;
    [SerializeField] private GameObject _statusPanel;
    [SerializeField] private GameObject _enemyStatusPanel;
    [SerializeField] private GameObject _turnPanel;

    [SerializeField] private Image _profileImage;
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private TMP_Text _levelText;
    [SerializeField] private TMP_Text _hpText;
    [SerializeField] private TMP_Text _attrText;
    [SerializeField] private TMP_Text _typeText;
    [SerializeField] private TMP_Text _actionText;

    [SerializeField] private Transform _enemyStatusParent;

    [SerializeField] private GameObject _dialogueBox;
    [SerializeField] private TMP_Text _battleMsgText;

    [SerializeField] private RectTransform _turnParent;

    private Dictionary<string, Sprite> _nameToSprite = new Dictionary<string, Sprite>();

    private AsyncOperationHandle<IList<Sprite>> _spriteHandle;
    private AsyncOperationHandle<GameObject> _enemyStatusHandle;

    private GameObject _enemyStatusPrefab;
    private List<GameObject> _enemyStatusUIList = new List<GameObject>();
    //private List<Image> _enemyHPList = new List<Image>();
    private Dictionary<int, Image> _uidToEnemyHP = new Dictionary<int, Image>();

    private GameObject _turnPrefab;
    private Queue<GameObject> _turnQueue = new Queue<GameObject>();    //이걸 활성화 비활성화
    private Queue<Image> _turnImageQueue = new Queue<Image>();         //이걸 애니메이션 처리 x좌표 0 -> 250 , 250 -> 0
    private Queue<DigimonStatus> _statusQueue = new Queue<DigimonStatus>();    //아군 + 적군 SPD순서로 정렬
    void Start()
    {
        SetProfileAsync().Forget();
    }

    public async UniTask RemoveDead(float duration = 0.5f)
    {
        if (_statusQueue.Count == 0)
            return;

        int count = _statusQueue.Count;
        
        for (int i = 0; i < count; i++)
        {
            var status = _statusQueue.Dequeue();
            var turn = _turnQueue.Dequeue();
            var img = _turnImageQueue.Dequeue();

            if (status.CurrentHP <= 0)
            {
                await AnimateImage(img.rectTransform, 0f, 250f, duration);
                turn.SetActive(false);
                LayoutRebuilder.ForceRebuildLayoutImmediate(_turnParent);

                continue;
            }

            _statusQueue.Enqueue(status);
            _turnQueue.Enqueue(turn);
            _turnImageQueue.Enqueue(img);
        }
    }

    // 현재 턴 이동: 큐에서 맨 위 하나만 꺼내서 처리
    public async UniTask UpdateTurn(float duration = 0.5f)
    {
        if (_statusQueue.Count == 0) 
            return;

        var status = _statusQueue.Dequeue();
        var turn = _turnQueue.Dequeue();
        var img = _turnImageQueue.Dequeue();

        if (status.CurrentHP > 0)
        {
            Debug.Log("맨 밑으로 이동");
            await AnimateImage(img.rectTransform, 0f, 250f, duration);

            turn.SetActive(false);
            turn.transform.SetAsLastSibling();
            turn.SetActive(true);

            LayoutRebuilder.ForceRebuildLayoutImmediate(_turnParent);

            await AnimateImage(img.rectTransform, 250f, 0f, duration);

            _statusQueue.Enqueue(status);
            _turnQueue.Enqueue(turn);
            _turnImageQueue.Enqueue(img);
        }
        else
        {
            Debug.LogError("첫번째 턴이 죽음");
            await AnimateImage(img.rectTransform, 0f, 250f, duration);
            turn.SetActive(false);
            LayoutRebuilder.ForceRebuildLayoutImmediate(_turnParent);
        }
    }

    private async UniTask AnimateImage(RectTransform rect, float start, float end, float duration)
    {
        float elapsed = 0f;
        
        Vector3 pos = rect.localPosition;
        pos.x = start;

        while (elapsed < duration)
        {
            float t = Mathf.Clamp01(elapsed / duration);
            pos.x = Mathf.Lerp(start, end, t);
            rect.localPosition = pos;

            elapsed += Time.deltaTime;
            await UniTask.Yield();
        }

        pos.x = end;
        rect.localPosition = pos;
    }

    public void SetTurnImage()
    {
        int count = _statusQueue.Count;
        for (int i = 0; i < count; i++)
        {
            var status = _statusQueue.Dequeue();
            var turn = _turnQueue.Dequeue();
            var img = _turnImageQueue.Dequeue();

            img.sprite = _nameToSprite[status.DigimonName + "Sprite"];
       
            _statusQueue.Enqueue(status);
            _turnQueue.Enqueue(turn);
            _turnImageQueue.Enqueue(img);
        }
    }

    private async UniTask SetTurnAsync()
    {
        try
        {
            var handle = Addressables.LoadAssetAsync<GameObject>("TurnPanel");
            await handle.Task;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                _turnPrefab = handle.Result;

                List<DigimonStatus> allStatus = new List<DigimonStatus>();
                allStatus.AddRange(BattleManager.Instance.PlayerStatusList);
                allStatus.AddRange(BattleManager.Instance.EnemyStatusList);

                allStatus.Sort((a,b) => b.SPD.CompareTo(a.SPD));

                for (int i = 0; i < allStatus.Count; i++)
                {
                    GameObject turn = Instantiate(_turnPrefab, _turnParent);
                    Image[] imgs = turn.GetComponentsInChildren<Image>();
                    Image img = imgs[0];
                    for (int j = 0; j < imgs.Length; j++)
                    {
                        if (imgs[j].name == "Profile")
                        {
                            img = imgs[j];
                            break;
                        }    
                    }

                    _turnQueue.Enqueue(turn);
                    _turnImageQueue.Enqueue(img);
                    _statusQueue.Enqueue(allStatus[i]);
                }
            }

            else
            {
                Debug.Log("TurnPanel 프리팹 로드 실패");
            }
            
        }

        catch (Exception e)
        {
            Debug.LogError($"에러 : {e.Message}");
        }
    }

    private async UniTaskVoid SetProfileAsync()
    {
        try
        {
            await UniTask.WaitUntil(() => BattleManager.Instance.PlayerStatusList.Count == GameManager.Instance.GetDigimonStatusList().Count);
            await UniTask.WaitUntil(() => BattleManager.Instance.EnemyStatusList.Count == GameManager.Instance.GetBattleList().Count);

            await LoadSpriteImages();


            await SetTurnAsync();       // TurnPanel 프리팹 로드 및 생성

            SetTurnImage();

            // EnemyStatus UI 프리팹 로드
            _enemyStatusHandle = Addressables.LoadAssetAsync<GameObject>("EnemyStatus");
            await _enemyStatusHandle.Task;

            if (_enemyStatusHandle.Status == AsyncOperationStatus.Succeeded)
            {
                _enemyStatusPrefab = _enemyStatusHandle.Result;
            }
            else
            {
                Debug.Log("로드 실패");
            }

            for (int i = 0; i < BattleManager.Instance.EnemyStatusList.Count; i++)
            {
                GameObject go = Instantiate(_enemyStatusPrefab, _enemyStatusParent);
                
                _enemyStatusUIList.Add(go);

                TMP_Text[] tmps = go.GetComponentsInChildren<TMP_Text>();

                for (int j = 0; j < tmps.Length; j++)
                {
                    if (tmps[j].name == "Name")
                    {
                        string levelName = "Lv." + 
                            BattleManager.Instance.EnemyStatusList[i].Level.ToString() + " " +
                            BattleManager.Instance.EnemyStatusList[i].DigimonNameKor;
                        tmps[j].text = levelName;
                    }
                    
                    else if (tmps[j].name == "Attr")
                    {
                        string attr = BattleManager.Instance.EnemyStatusList[i].Attr.ToString().Substring(0, 2);

                        string colorCode = ColorTable.GetColor(attr);

                        tmps[j].text = $"<color={colorCode}>{attr}</color>";
                    }
                }

                Image[] imgs = go.GetComponentsInChildren<Image>();

                for (int j = 0; j < imgs.Length; j++)
                {
                    if (imgs[j].name == "HP")
                    {
                        _uidToEnemyHP[BattleManager.Instance.EnemyStatusList[i].UID] = imgs[j];
                        //_enemyHPList.Add(imgs[j]);
                    }
                }
            }

            DigimonStatus first = BattleManager.Instance.PlayerStatusList[0];

            for (int i = 1; i < BattleManager.Instance.PlayerStatusList.Count; i++)
            {
                if (first.SPD < BattleManager.Instance.PlayerStatusList[i].SPD)
                {
                    first = BattleManager.Instance.PlayerStatusList[i];
                }
            }

            _profileImage.sprite = _nameToSprite[first.DigimonName + "Sprite"];
            _nameText.text = first.DigimonNameKor;
            _levelText.text = $"레벨 : {first.Level}";
            _hpText.text = $"HP : {first.CurrentHP} / {first.HP}";
            string at = first.Attr.ToString().Substring(0, 2);
            string hexCode = ColorTable.GetColor(at);
            _attrText.text = $"속성 : <color={hexCode}>{at}</color>";
            _typeText.text = $"타입 : {first.TypeKor}";
            _actionText.text = first.ActionCount.ToString();

            FadeInOut.Instance.FadeOut();
        }
        
        catch (Exception e)
        {
            Debug.LogError($"에러 : {e.Message}");
        }
    }

    public void UpdateProfile(DigimonStatus status)
    {
        _profileImage.sprite = _nameToSprite[status.DigimonName + "Sprite"];
        _nameText.text = status.DigimonNameKor;
        _levelText.text = $"레벨 : {status.Level}";
        _hpText.text = $"HP : {status.CurrentHP} / {status.HP}";
        string attr = status.Attr.ToString().Substring(0, 2);
        string colorCode = ColorTable.GetColor(attr);
        _attrText.text = $"속성 : <color={colorCode}>{attr}</color>";
        _typeText.text = $"타입 : {status.TypeKor}";
        _actionText.text = status.ActionCount.ToString();
    }

    private void OnDestroy()
    {
        if (_spriteHandle.IsValid())
        {
            Addressables.Release(_spriteHandle);
        }

        if (_enemyStatusHandle.IsValid())
        {
            Addressables.Release(_enemyStatusHandle);
        }
    }

    private async UniTask LoadSpriteImages()
    {
        if (DigimonDB.Instance != null && DigimonDB.Instance.HasDigimonSprites())
        {
            Debug.Log("DB에 스프라이트 있음");
            _nameToSprite = DigimonDB.Instance.GetDigimonSprites();
        }


        else
        {
            Debug.Log("DB에 스프라이트 없음");
            _spriteHandle = Addressables.LoadAssetsAsync<Sprite>("DigimonSprites", null);
            await _spriteHandle.Task;
            if (_spriteHandle.Status == AsyncOperationStatus.Succeeded)
            {
                IList<Sprite> list = _spriteHandle.Result;

                foreach (Sprite sprite in list)
                {
                    _nameToSprite[sprite.name] = sprite;
                }
            }
            else
            {
                Debug.Log("로드 실패");
            }
        }
    }

    public void TogglePlayerPanel(bool enabled)
    {
        _attackPanel.SetActive(enabled);
        _statusPanel.SetActive(enabled);
    }

    public void HideAllUI()
    {
        _attackPanel.SetActive(false);
        _statusPanel.SetActive(false);
        _enemyStatusPanel.SetActive(false);
        _turnPanel.SetActive(false);
    }

    public void ShowTargetNumber()
    {
        for (int i = 0; i < BattleManager.Instance.EnemyStatusList.Count; i++)
        {
            if (BattleManager.Instance.EnemyStatusList[i].CurrentHP <= 0)
            {
                continue;
            }

            Vector3 pos = BattleManager.Instance.EnemyStatusList[i].transform.position + Vector3.up * _heightOffset;
            _numberList[i].SetActive(true);
            _numberList[i].transform.position = pos;
        }
    }

    public void HideTargetNumber()
    {
        for (int i = 0; i < _numberList.Count; i++)
        {
            _numberList[i].SetActive(false);
        }
    }

    public void UpdateEnemyHP(DigimonStatus status)
    {
        _uidToEnemyHP[status.UID].fillAmount = (float)status.CurrentHP / (float)status.HP;
    }


    public void ShowBattleMsg(bool enabled, string msg = "")
    {
        _dialogueBox.SetActive(enabled);
        _battleMsgText.gameObject.SetActive(enabled);
        _battleMsgText.text = msg;
    }

    public async UniTask ShowBattleMsgAsync(string msg = "")
    {
        _dialogueBox.SetActive(true);
        
        _battleMsgText.gameObject.SetActive(true);
        _battleMsgText.text = msg;

        await UniTask.Delay(2000);

        _battleMsgText.gameObject.SetActive(false);
        _dialogueBox.SetActive(false);
    }

}
