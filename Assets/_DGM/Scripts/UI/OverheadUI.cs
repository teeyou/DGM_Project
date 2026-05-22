using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class OverheadUI : MonoBehaviour
{
    [SerializeField] private float _smallHeightOffset;
    [SerializeField] private float _mediumHeightOffset;
    [SerializeField] private float _largeHeightOffset;

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
            _overheadList[i].transform.position = _enemyList[i].transform.position + Vector3.up * _smallHeightOffset;
        }
    }

    private void MakeOverheadUI()
    {
        for (int i = 0; i < _enemyList.Count; i++)
        {
            GameObject enemy = _enemyList[i];
            DigimonStatus enemyStatus = enemy.AddComponent<DigimonStatus>();

            EnemyStatusData data = DigimonDB.Instance.GetEnemyStatusDataById(5001 + i);

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
