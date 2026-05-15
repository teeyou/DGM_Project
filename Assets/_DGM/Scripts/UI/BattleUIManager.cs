using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class BattleUIManager : Singleton<BattleUIManager>
{
    [SerializeField] private float _heightOffset;

    [SerializeField] private List<GameObject> _numberList;
    
    [SerializeField] private GameObject _attackPanel;
    [SerializeField] private GameObject _statusPanel;

    [SerializeField] private Image _profileImage;
    [SerializeField] private TMP_Text _hpText;
    [SerializeField] private TMP_Text _attrText;
    [SerializeField] private TMP_Text _actionText;

    [SerializeField] private BattleManager _battleManager;
    [SerializeField] private Transform _enemyStatusParent;

    private Dictionary<string, Sprite> _nameToSprite = new Dictionary<string, Sprite>();

    private AsyncOperationHandle<IList<Sprite>> _spriteHandle;
    private AsyncOperationHandle<GameObject> _enemyStatusHandle;

    private GameObject _enemyStatusPrefab;
    private List<GameObject> _enemyStatusUIList = new List<GameObject>();
    private List<Image> _enemyHPList = new List<Image>();

    void Start()
    {
        SetProfileAsync().Forget();
    }

    void Update()
    {
        
    }

    private async UniTaskVoid SetProfileAsync()
    {
        try
        {
            await UniTask.WaitUntil(() => _battleManager.PlayerStatusList.Count > 0);
            await UniTask.WaitUntil(() => _battleManager.EnemyStatusList.Count == GameManager.Instance.GetBattleList().Count);

            await LoadSpriteImages();

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

            for (int i = 0; i < _battleManager.EnemyStatusList.Count; i++)
            {
                GameObject go = Instantiate(_enemyStatusPrefab, _enemyStatusParent);
                
                _enemyStatusUIList.Add(go);

                TMP_Text[] tmps = go.GetComponentsInChildren<TMP_Text>();

                for (int j = 0; j < tmps.Length; j++)
                {
                    if (tmps[j].name == "Name")
                    {
                        string levelName = "Lv." + 
                            _battleManager.EnemyStatusList[i].Level.ToString() + " " +
                            _battleManager.EnemyStatusList[i].DigimonNameKor;
                        tmps[j].text = levelName;
                    }
                    
                    else if (tmps[j].name == "Attr")
                    {
                        string attr = _battleManager.EnemyStatusList[i].Attr.ToString().Substring(0, 2);

                        string colorCode = attr switch
                        {
                            "No" => ColorTable.White,
                            "Va" => ColorTable.Green,
                            "Da" => ColorTable.Sky,
                            "Vi" => ColorTable.Red,
                            "Fr" => ColorTable.Yellow,
                            "Un" => ColorTable.Purple,  
                            _ => ColorTable.White,
                        };

                        tmps[j].text = $"<color={colorCode}>{attr}</color>";
                    }
                }

                Image[] imgs = go.GetComponentsInChildren<Image>();

                for (int j = 0; j < imgs.Length; j++)
                {
                    if (imgs[j].name == "HP")
                    {
                        _enemyHPList.Add(imgs[j]);
                    }
                }
            }

            DigimonStatus first = _battleManager.PlayerStatusList[0];

            for (int i = 1; i < _battleManager.PlayerStatusList.Count; i++)
            {
                if (first.SPD < _battleManager.PlayerStatusList[i].SPD)
                {
                    first = _battleManager.PlayerStatusList[i];
                }
            }

            _profileImage.sprite = _nameToSprite[first.DigimonName];
            _hpText.text = $"HP : {first.CurrentHP} / {first.HP}";
            _attrText.text = first.Attr.ToString();
            _actionText.text = first.ActionCount.ToString();

            FadeInOut.Instance.FadeOut();
        }
        
        catch (Exception e)
        {
            Debug.LogError($"에러 : {e.Message}");
        }
    }

    public void SetProfile(DigimonStatus status)
    {
        _profileImage.sprite = _nameToSprite[status.DigimonName];
        _hpText.text = $"HP : {status.CurrentHP} / {status.HP}";
        _attrText.text = status.Attr.ToString();
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

    public void HidePlayerPanel()
    {
        _attackPanel.SetActive(false);
        //_statusPanel.SetActive(false);
    }

    public void ShowInputMode()
    {
        _attackPanel.SetActive(true);
        _statusPanel.SetActive(true);
    }

    public void ShowTargetNumber()
    {
        for (int i = 0; i < _battleManager.EnemyStatusList.Count; i++)
        {
            if (_battleManager.EnemyStatusList[i].CurrentHP <= 0)
            {
                continue;
            }

            Vector3 pos = _battleManager.EnemyStatusList[i].transform.position + Vector3.up * _heightOffset;
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

    public void UpdateEnemyHP()
    {
        for (int i = 0; i < _enemyHPList.Count; i++)
        {
            DigimonStatus status = BattleManager.Instance.EnemyStatusList[i];
            _enemyHPList[i].fillAmount = (float)status.CurrentHP / (float)status.HP;
        }
    }

    public void UpdatePlayer()
    {
        DigimonStatus status = BattleManager.Instance.PlayerStatusList[0];
        _hpText.text = $"HP : {status.CurrentHP} / {status.HP}";
    }
}
