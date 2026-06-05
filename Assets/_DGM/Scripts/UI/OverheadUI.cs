using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class OverheadUI : MonoBehaviour
{
    [SerializeField] private float _heightOffset;
    [SerializeField] private float _midHeightOffset;
    [SerializeField] private float _largeHeightOffset;
    [SerializeField] private float _xlargeHeightOffset;
    [SerializeField] private GameObject _overheadUIPrefab;
    [SerializeField] private List<GameObject> _enemyList = new List<GameObject>();

    private List<GameObject> _overheadList = new List<GameObject>();
    private List<TMP_Text> _nameTextList = new List<TMP_Text>();
    private List<TMP_Text> _attrTextList = new List<TMP_Text>();
    private List<EnemyStatusData> _enemyStatusDataList = new List<EnemyStatusData>();
    private List<DigimonStatus> _enemyStatusList = new List<DigimonStatus>();
    void Start()
    {
        MakeOverheadUI();
    }

    void LateUpdate()
    {
        if (_overheadList.Count <= 0)
            return;

        for (int i = 0; i < _enemyList.Count; i++)
        {
            if (_enemyStatusList[i].Grade == EGrade.Baby)
            {
                _overheadList[i].transform.position = _enemyList[i].transform.position + Vector3.up * _heightOffset;
            }

            // 성장기 오프셋
            else if (_enemyStatusList[i].Grade == EGrade.Rookie)
            {
                if (_enemyStatusList[i].DigimonName == "Terriermon")
                {
                    //_heightOffset = 0f;
                    _overheadList[i].transform.position = _enemyList[i].transform.position + Vector3.up * _heightOffset;
                }

                else if (_enemyStatusList[i].DigimonName == "Lopmon")
                {
                    //_heightOffset = 0f;
                    _overheadList[i].transform.position = _enemyList[i].transform.position + Vector3.up * _heightOffset;
                }

                else if (_enemyStatusList[i].DigimonName == "Lucemon")
                {
                    //_heightOffset = 0f;
                    _overheadList[i].transform.position = _enemyList[i].transform.position + Vector3.up * _largeHeightOffset;
                }

                else
                {
                    _overheadList[i].transform.position = _enemyList[i].transform.position + Vector3.up * _midHeightOffset;
                    //_heightOffset = 0.8f;
                }
            }

            // 성숙기 오프셋
            else if (_enemyStatusList[i].Grade == EGrade.Champion)
            {
                if (_enemyStatusList[i].DigimonName == "Gotomon")
                {
                    _overheadList[i].transform.position = _enemyList[i].transform.position + Vector3.up * _midHeightOffset;
                    //_heightOffset = 0.8f;
                }

                else if (_enemyStatusList[i].DigimonName == "Devimon")
                {
                    _overheadList[i].transform.position = _enemyList[i].transform.position + Vector3.up * _xlargeHeightOffset;
                    //_heightOffset = 2f;
                }

                else if (_enemyStatusList[i].DigimonName == "Magnamon")
                {
                    _overheadList[i].transform.position = _enemyList[i].transform.position + Vector3.up * _xlargeHeightOffset;
                    //_heightOffset = 2f;
                }

                else
                {
                    _overheadList[i].transform.position = _enemyList[i].transform.position + Vector3.up * _largeHeightOffset;
                    //_heightOffset = 1.5f;
                }
            }
            
            //_overheadList[i].transform.position = _enemyList[i].transform.position + Vector3.up * _heightOffset;
        }
    }

    private void MakeOverheadUI()
    {
        for (int i = 0; i < _enemyList.Count; i++)
        {
            GameObject enemy = _enemyList[i];
            
            EnemyFieldMove script = enemy.GetComponent<EnemyFieldMove>();
            int id = script.PartyList[0];

            DigimonStatus enemyStatus = enemy.AddComponent<DigimonStatus>();

            EnemyStatusData data = DigimonDB.Instance.GetEnemyStatusDataById(id);

            GrowthType growthType = DigimonDB.Instance.GetGrowthType(data.GrowthType);

            enemyStatus.Init(data, growthType);

            GameObject overheadUI = Instantiate(_overheadUIPrefab, enemy.transform.position, _overheadUIPrefab.transform.rotation, transform);

            TMP_Text nameText = overheadUI.transform.GetChild(0).GetComponent<TMP_Text>(); // Name

            string name = $"Lv.{data.Level} {enemyStatus.DigimonNameKor} ";

            string attr = enemyStatus.Attr.ToString().Substring(0, 2);
            
            if (attr != "No")
            {
                string colorCode = ColorTable.GetColor(attr);

                name += $"<color={colorCode}>{attr}</color>";
            }
            
            nameText.text = name;

            _enemyStatusList.Add(enemyStatus);
            _enemyStatusDataList.Add(data);
            _overheadList.Add(overheadUI);
            _nameTextList.Add(nameText);
            //_attrTextList.Add(attrText);

        }

    }
}
