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

    private Dictionary<string, Sprite> _nameToSprite = new Dictionary<string, Sprite>();

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

            AsyncOperationHandle<IList<Sprite>> handle = Addressables.LoadAssetsAsync<Sprite>("SpritesRookie", null);
            await handle.Task;
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                IList<Sprite> list = handle.Result;

                foreach (Sprite sprite in list)
                {
                    _nameToSprite[sprite.name] = sprite;
                }
            }
            else
            {
                Debug.Log("로드 실패");
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
        _attrText.text = status.AttrKor;
        _actionText.text = status.ActionCount.ToString();
    }
}
