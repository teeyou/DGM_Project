using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEditor.PlayerSettings;
using static UnityEngine.Rendering.DebugUI.Table;

/*
player
pos : x : -3 , z : -1 , 1 , 3
rot : y 90


enemy
pos : x : 3 , z : -1 , 1 , 3
rot : y 270
*/

public class BattleManager : MonoBehaviour
{
    // 게임오브젝트
    private List<GameObject> _playerDigimonGoList = new List<GameObject>();
    private List<GameObject> _enemyDigimonGoList = new List<GameObject>();

    // DigimonStatus
    private List<DigimonStatus> _playerStatusList = new List<DigimonStatus>();
    private List<DigimonStatus> _enemyStatusList = new List<DigimonStatus>();

    private IReadOnlyList<int> _enemyList;

    private int _playerCount = 0;
    private int _enemyCount = 0;

    private bool _playerReady = false;
    private bool _enemyReady = false;

    private void OnEnable()
    {
        SceneLoader.Instance.OnBattleSceneLoaded += OnCompleteSceneLoad;
        DigimonSpawner.Instance.OnPlayerDigimonSpawned += OnCompletePlayerDigimonSpawn;
        DigimonSpawner.Instance.OnEnemyDigimonSpawned += OnCompleteEnemyDigimonSpawn;
    }

    private void OnDisable()
    {
        if (SceneLoader.Instance != null)
        {
            SceneLoader.Instance.OnBattleSceneLoaded -= OnCompleteSceneLoad;
        }

        if (DigimonSpawner.Instance != null)
        {
            DigimonSpawner.Instance.OnPlayerDigimonSpawned -= OnCompletePlayerDigimonSpawn;
            DigimonSpawner.Instance.OnEnemyDigimonSpawned -= OnCompleteEnemyDigimonSpawn;
        }
            
    }
    void Update()
    {
        
    }

    private void OnCompletePlayerDigimonSpawn(GameObject go, DigimonStatus status)
    {
        _playerDigimonGoList.Add(go);
        _playerStatusList.Add(status);

        _playerCount++;

        Debug.Log($"_playerCount : {_playerCount}");
    }

    private void OnCompleteEnemyDigimonSpawn(GameObject go, DigimonStatus data)
    {
        _enemyDigimonGoList.Add(go);
        _enemyStatusList.Add(data);

        _enemyCount++;

        Debug.Log($"_enemyCount : {_enemyCount}");
    }

    private void OnCompleteSceneLoad()
    {
        Debug.Log("BattleManager 초기화 수행");
        SpawnPlayerDigimon();
        SpawnEnemies();

    }

    private Vector3 GetPos(int idx, bool isEnemy)
    {
        float x = isEnemy ? 4f : -4f;
        float z = idx switch
        {
            0 => 0f,
            1 => 2f,
            _ => -2f
        };

        return new Vector3(x, 0f, z);
    }

    private void SpawnPlayerDigimon()
    {
        IReadOnlyList<DigimonStatus> statusList = GameManager.Instance.GetDigimonStatusList();

        for (int i = 0; i < statusList.Count; i++)
        {
            Vector3 pos = GetPos(i, false);
            
            Quaternion rot = Quaternion.Euler(0f, 90f, 0f);
            DigimonStatus status = statusList[i];
            DigimonSpawner.Instance.SpawnPlayerDigimon(i, statusList[i].DigimonName, pos, rot);
        }
    }

    private void SpawnEnemies()
    {
        _enemyList = GameManager.Instance.GetBattleList();

        for (int i = 0; i < _enemyList.Count; i++)
        {
            Vector3 pos = GetPos(i, true);
            Quaternion rot = Quaternion.Euler(0f, 270f, 0f);
            EnemyStatusData data = DigimonDB.Instance.GetEnemyStatusDataById(_enemyList[i]);
            DigimonSpawner.Instance.SpawnEnemyDigimon(data.ID, data.DigimonName, pos, rot);
        }
    }

    private void ReturnField()
    {
        // 전투 끝나면 호출
        GameManager.Instance._battleList.Clear();
    }

}
