using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class OverheadUI : MonoBehaviour
{
    [SerializeField] private float _heightOffset;

    [SerializeField] private GameObject _overheadUIPrefab;
    [SerializeField] private List<GameObject> _enemyList = new List<GameObject>();

    private List<GameObject> _overheadList = new List<GameObject>();
    private List<TMP_Text> _nameTextList = new List<TMP_Text>();
    private List<TMP_Text> _attrTextList = new List<TMP_Text>();

    void Start()
    {
        MakeOverheadUI();

        Debug.Log($"{_overheadList.Count} {_nameTextList.Count} {_attrTextList.Count}");
    }

    void LateUpdate()
    {
        if (_overheadList.Count <= 0)
            return;

        for (int i = 0; i < _enemyList.Count; i++)
        {
            _overheadList[i].transform.position = _enemyList[i].transform.position + Vector3.up * _heightOffset;
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
            EvoTree evoTree = DigimonDB.Instance.GetEvoTreeById(5001 + i);

            enemyStatus.Init(data, growthType, evoTree);

            GameObject overheadUI = Instantiate(_overheadUIPrefab, enemy.transform.position, _overheadUIPrefab.transform.rotation, transform);

            TMP_Text nameText = overheadUI.transform.GetChild(0).GetComponent<TMP_Text>(); // Name
            TMP_Text attrText = overheadUI.transform.GetChild(1).GetComponent<TMP_Text>(); // Attribute
            string name = $"Lv.{data.Level} {enemyStatus.DigimonNameKor}";
            string attr = enemyStatus.Attr.ToString().Substring(0, 2);

            nameText.text = name;
            if (attr != "No")
            {
                attrText.text = attr;
                attrText.color = attr switch
                {
                    "Va" => Color.green,
                    "Da" => Color.blue,
                    "Vi" => Color.red,
                    _ => Color.yellow,
                };
            }

            _overheadList.Add(overheadUI);
            _nameTextList.Add(nameText);
            _attrTextList.Add(attrText);

            Debug.Log($"{name} {attr}");
            
        }

    }
}
