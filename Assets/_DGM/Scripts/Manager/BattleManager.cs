using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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
    [SerializeField] private List<GameObject> _playerDigimonGoList = new List<GameObject>();
    [SerializeField] private List<GameObject> _enemyDigimonGoList = new List<GameObject>();

    // DigimonStatus
    [SerializeField] private List<DigimonStatus> _playerStatusList = new List<DigimonStatus>();
    [SerializeField] private List<DigimonStatus> _enemyStatusList = new List<DigimonStatus>();

    private IReadOnlyList<int> _enemyList;

    private int _playerCount = 0;
    private int _enemyCount = 0;

    private InputManager _input;

    public List<GameObject> PlayerDigimonGoList => _playerDigimonGoList;
    public List<GameObject> EnemyDigimonGoList => _enemyDigimonGoList;

    public List<DigimonStatus> PlayerStatusList => _playerStatusList;
    public List<DigimonStatus> EnemyStatusList => _enemyStatusList;

    private void OnEnable()
    {
        SceneLoader.Instance.OnBattleSceneLoaded += OnCompleteSceneLoad;
        DigimonSpawner.Instance.OnPlayerDigimonSpawned += OnCompletePlayerDigimonSpawn;
        DigimonSpawner.Instance.OnEnemyDigimonSpawned += OnCompleteEnemyDigimonSpawn;

        _input = InputManager.Instance;

        _input.OnEvo += HandleEvo;
        _input.OnAttack += HandleAttack;
        _input.OnSkill += HandleSkill;
        _input.OnGuard += HandleGuard;
        _input.OnSelect += HandleSelect;
        _input.OnRun += HandleRun;
        _input.OnMenuMove += HandleMove;
    }

    private void HandleEvo(bool pressed)
    {
        Debug.Log("진화");
    }

    private void HandleAttack(bool pressed)
    {
        Debug.Log("공격");
    }

    private void HandleSkill(bool pressed)
    {
        Debug.Log("스킬");
    }

    private void HandleGuard(bool pressed)
    {
        Debug.Log("가드");
    }

    private void HandleSelect(bool pressed)
    {
        Debug.Log("선택");
    }

    private void HandleRun(bool pressed)
    {
        Debug.Log("도망");
        RunAsync().Forget();
    }

    private void HandleMove(Vector2 v)
    {
        Debug.Log($"이동 {v.x}");
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

        if (_input != null)
        {
            _input.OnEvo -= HandleEvo;
            _input.OnAttack -= HandleAttack;
            _input.OnSkill -= HandleSkill;
            _input.OnGuard -= HandleGuard;
            _input.OnSelect -= HandleSelect;
            _input.OnRun -= HandleRun;
            _input.OnMenuMove -= HandleMove;
        }
            
    }

    private void Start()
    {


        
    }
    void Update()
    {
        
    }

    private void OnCompletePlayerDigimonSpawn(GameObject go, DigimonStatus status)
    {
        _playerDigimonGoList.Add(go);
        _playerStatusList.Add(status);
        status.CurrentHP = status.HP;
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
        SceneLoader.Instance.LoadTargetScene(GameManager.Instance.ReturnSceneName, true);
        GameManager.Instance.ReturnSceneName = null;
    }

     
    private async UniTask RunAsync()
    {
        for (int i = 0; i < _playerStatusList.Count; i++)
        {
            _playerStatusList[i].Run();
        }

        await UniTask.Delay(1000);

        ReturnField();
    }
}
