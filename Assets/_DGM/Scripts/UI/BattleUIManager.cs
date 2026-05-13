using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
using static UnityEngine.ParticleSystem;

public class BattleUIManager : MonoBehaviour
{
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
            await UniTask.WaitUntil(() => _battleManager.EnemyStatusList.Count > 0);

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
                        tmps[j].text = _battleManager.EnemyStatusList[i].DigimonNameKor;
                    }
                    
                    else if (tmps[j].name == "Attr")
                    {
                        string attr = _battleManager.EnemyStatusList[i].Attr.ToString().Substring(0, 2);

                        string colorCode = attr switch
                        {
                            "No" => "#FFFFFF",   // 하얀색
                            "Va" => "#00FF00",   // 초록색
                            "Da" => "#00C0FF",   // 하늘색
                            "Vi" => "#FF0000",   // 빨간색
                            "Fr" => "#FFFF00",   // 노란색
                            _ => "#FFFFFF"       // 하얀색
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
            _hpText.text = $"{first.CurrentHP} / {first.HP}";
            _attrText.text = first.AttrKor;
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
        //_profileImage = 
        _hpText.text = $"{status.HP} / {status.CurrentHP}";
        _attrText.text = status.Attr.ToString();
        _actionText.text = status.ActionCount.ToString();
    }

    private void OnDestroy()
    {
        Addressables.Release(_spriteHandle);
        Addressables.Release(_enemyStatusHandle);
    }
}
