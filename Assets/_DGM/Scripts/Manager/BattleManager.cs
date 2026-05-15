using Cinemachine;
using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;

/*
player
pos : x : -4 , z : -2 , 0 , 2
rot : y 90


enemy
pos : x : 4 , z : -2 , 0 , 2
rot : y 270
*/

public enum EActionType { Attack, Skill, Guard, Evo }

public class ActionCommand
{
    public DigimonStatus Actor;   // 행동하는 주체
    public EActionType ActionType;       // 공격/스킬/방어
    public DigimonStatus Target;  // 대상 (방어는 자기 자신)
    public bool IsEnemy;

    public ActionCommand(DigimonStatus actor, EActionType actionType, DigimonStatus target, bool isEnemy)
    {
        this.Actor = actor;
        this.ActionType = actionType;
        this.Target = target;
        this.IsEnemy = isEnemy;
    }
}

public class BattleManager : Singleton<BattleManager>
{
    [SerializeField] private BattleUIManager _battleUI;
    [SerializeField] private Vector3 _vcamPos;
    private const int HIGH_PRIORITY = 15;
    private const int LOW_PRIORITY = 10;
    // 게임오브젝트
    [SerializeField] private List<GameObject> _playerDigimonGoList = new List<GameObject>();
    [SerializeField] private List<GameObject> _enemyDigimonGoList = new List<GameObject>();

    // DigimonStatus
    [SerializeField] private List<DigimonStatus> _playerStatusList = new List<DigimonStatus>();
    [SerializeField] private List<DigimonStatus> _enemyStatusList = new List<DigimonStatus>();

    [SerializeField] private CinemachineVirtualCamera _dollyCam;
    [SerializeField] private CinemachineVirtualCamera _vcam;

    private IReadOnlyList<int> _enemyList;

    private InputManager _input;

    private int _currentPlayerIndex = 0;
    private int _inputCount = 0;

    private bool _isBattleReady = false;

    private List<ActionCommand> _battleCommandList = new List<ActionCommand>();

    [SerializeField] private bool _isAttackMode = false;

    public List<GameObject> PlayerDigimonGoList => _playerDigimonGoList;
    public List<GameObject> EnemyDigimonGoList => _enemyDigimonGoList;

    public List<DigimonStatus> PlayerStatusList => _playerStatusList;
    public List<DigimonStatus> EnemyStatusList => _enemyStatusList;

    private bool _isAttack = true;

    private bool _isFinish = false;

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
        //_input.OnSelect += HandleSelect;
        _input.OnRun += HandleRun;
        _input.OnPress1 += HandlePress1;
        _input.OnPress2 += HandlePress2;
        _input.OnPress3 += HandlePress3;
    }

    private void HandleEvo(bool pressed)
    {
        //if (!_isAttackMode)
        //{
        //    Debug.Log("진화");
        //    ActionCommand c = new ActionCommand(_playerStatusList[_inputCount], EActionType.Evo, null);
        //    _battleCommandList.Add(c);
        //    _inputCount++;
        //}
    }

    private void HandleAttack(bool pressed)
    {
        if (!_isAttackMode)
        {
            _isAttack = true;
            EnterAttackMode();
        }
    }

    private void HandleSkill(bool pressed)
    {
        if (!_isAttackMode)
        {
            _isAttack = false;
            EnterAttackMode();
        }
    }

    private void HandleGuard(bool pressed)
    {
        if (!_isAttackMode)
        {
            Debug.Log("가드");
            ActionCommand c = new ActionCommand(_playerStatusList[_inputCount], EActionType.Guard, null, false);
            _battleCommandList.Add(c);
            _inputCount++;

            _isAttackMode = true;
            _battleUI.HidePlayerPanel();

            CheckNext();
        }
    }

    //private void HandleSelect(bool pressed)
    //{
    //    if (_isAttackMode)
    //    {
    //        Debug.Log("선택");
    //        _inputCount++;
    //    }
    //}

    private void HandleRun(bool pressed)
    {
        if (!_isAttackMode)
        {
            Debug.Log("도망");
            _input.EnableBattle(false);
            _battleUI.HidePlayerPanel();
            RunAsync().Forget();
        }
    }

    private void HandlePress1(bool pressed)
    {
        if (_isAttackMode)
        {
            if (_enemyStatusList.Count >= 1 && _enemyStatusList[0].CurrentHP <= 0)
            {
                return;
            }

            Debug.Log("1번");
            EActionType type = _isAttack ? EActionType.Attack : EActionType.Skill;
            ActionCommand c = new ActionCommand(_playerStatusList[_inputCount], type, _enemyStatusList[0], false);
            _battleCommandList.Add(c);
            _inputCount++;

            CheckNext();
        }
    }
    private void HandlePress2(bool pressed)
    {
        if (_isAttackMode)
        {
            if (_enemyStatusList.Count < 2)
            {
                return;
            }

            if (_enemyStatusList.Count >= 2 && _enemyStatusList[1].CurrentHP <= 0)
            {
                return;
            }

            Debug.Log("2번");
            EActionType type = _isAttack ? EActionType.Attack : EActionType.Skill;
            ActionCommand c = new ActionCommand(_playerStatusList[_inputCount], type, _enemyStatusList[1], false);
            _battleCommandList.Add(c);
            _inputCount++;

            CheckNext();
        }
    }
    private void HandlePress3(bool pressed)
    {
        if (_isAttackMode)
        {
            if (_enemyStatusList.Count < 3)
            {
                return;
            }

            if (_enemyStatusList.Count >= 3 && _enemyStatusList[2].CurrentHP <= 0)
            {
                return;
            }

            Debug.Log("3번");
            EActionType type = _isAttack ? EActionType.Attack : EActionType.Skill;
            ActionCommand c = new ActionCommand(_playerStatusList[_inputCount], type, _enemyStatusList[2], false);
            _battleCommandList.Add(c);
            _inputCount++;

            CheckNext();
        }
    }

    private bool CheckNext()
    {
        _battleUI.HideTargetNumber();

        if (_inputCount < _playerStatusList.Count)
        {
            Debug.Log("다음 디지몬 입력");
            EnterInputMode();
            _battleUI.SetProfile(_playerStatusList[_inputCount]);
            return true;
        }

        // 입력 끝
        return false;
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
            //_input.OnSelect -= HandleSelect;
            _input.OnRun -= HandleRun;
            _input.OnPress1 -= HandlePress1;
            _input.OnPress2 -= HandlePress2;
            _input.OnPress3 -= HandlePress3;
        }
            
    }

    private void Start()
    {
        _input.SwitchToBattleMap();
    }
    void Update()
    {
        if (_playerStatusList.Count == 0 || _enemyStatusList.Count == 0)
            return;

        if (_inputCount < _playerStatusList.Count)
            return;
        
        if (!_isBattleReady)
        {
            _isBattleReady = true;
            PlayBattle().Forget();
        }
    }

    private async UniTaskVoid PlayBattle()
    {
        _input.EnableBattle(false);

        //_battleUI.HideTargetNumber();
        SetEnemyCommand();

        _battleCommandList.Sort((a, b) => b.Actor.SPD.CompareTo(a.Actor.SPD));

        for (int i = 0; i < _battleCommandList.Count; i++)
        {
            Debug.Log($"{i} : {_battleCommandList[i].Actor.DigimonName} {_battleCommandList[i].Actor.SPD}");
        }

        for (int i = 0; i < _battleCommandList.Count; i++)
        {
            await ExecuteCommand(_battleCommandList[i]);
        }

        _battleCommandList.Clear();
        _inputCount = 0;
        _isBattleReady = false;

        _isFinish = await CheckBattleFinish();
        if (_isFinish)
        {
            // 전투 승리 OR 패배
            ReturnField();
            return;
        }

        EnterInputMode();
    }

    private async UniTask<bool> CheckBattleFinish()
    {
        bool isPlayerDead = true;
        bool isEnemyDead = true;
        for (int i = 0; i < _playerStatusList.Count; i++)
        {
            if (_playerStatusList[i].CurrentHP > 0)
            {
                isPlayerDead = false;
                break;
            }
        }

        for (int i = 0; i < _enemyStatusList.Count; i++)
        {
            if (_enemyStatusList[i].CurrentHP > 0)
            {
                isEnemyDead = false;
                break;
            }
        }

        if (isPlayerDead)
        {
            // 패배
            Debug.Log("패");
            return true;
        }

        if (isEnemyDead)
        {
            // 승
            Debug.Log("승");
            await ShowMyDigimon();
            return true;
        }

        // 진행중
        Debug.Log("진행중");
        return false;
    }

    private async UniTask ExecuteCommand(ActionCommand cmd)
    {
        if (cmd.Actor.CurrentHP <= 0)
        {
            return;
        }

        switch (cmd.ActionType)
        {
            case EActionType.Attack:

                ShowDigimon(cmd.Actor, cmd.IsEnemy);
                await cmd.Actor.Attack(cmd.Target, !cmd.IsEnemy);   // Target이 enemy인지 확인
                break;
            case EActionType.Skill:
                ShowDigimon(cmd.Actor, cmd.IsEnemy);
                await cmd.Actor.Skill(cmd.Target, !cmd.IsEnemy);    // Target이 enemy인지 확인
                break;
            case EActionType.Guard:
                ShowDigimon(cmd.Actor, cmd.IsEnemy);
                await cmd.Actor.Guard();
                break;
            case EActionType.Evo:
                ShowDigimon(cmd.Actor, cmd.IsEnemy);
                await cmd.Actor.Evo();
                break;
        }
    }

    private void SetEnemyCommand()
    {
        for (int i = 0; i < _enemyStatusList.Count; i++)
        {
            DigimonStatus actor = _enemyStatusList[i];

            int idx = Random.Range(0, _playerStatusList.Count);
            DigimonStatus target = _playerStatusList[idx];

            int r = Random.Range(0, 10);
            Debug.Log($"random : {r}");
            ActionCommand c;

            if (actor.Grade == EGrade.Baby)
            {
                if (r < 9)
                {
                    // 공격
                    c = new ActionCommand(actor, EActionType.Attack, target, true);
                }

                else
                {
                    // 방어
                    c = new ActionCommand(actor, EActionType.Guard, null, true);
                }
            }

            else
            {
                if (r < 6)
                {
                    // 공격
                    c = new ActionCommand(actor, EActionType.Attack, target, true);
                }

                else if (r < 9)
                {
                    // 스킬
                    c = new ActionCommand(actor, EActionType.Skill, target, true);
                }

                else
                {
                    // 방어
                    c = new ActionCommand(actor, EActionType.Guard, null, true);
                }
            }
            

            _battleCommandList.Add(c);
        }
    }

    

    private void OnCompletePlayerDigimonSpawn(GameObject go, DigimonStatus status)
    {
        _playerDigimonGoList.Add(go);
        _playerStatusList.Add(status);
    }

    private void OnCompleteEnemyDigimonSpawn(GameObject go, DigimonStatus data)
    {
        _enemyDigimonGoList.Add(go);
        _enemyStatusList.Add(data);
    }

    private void OnCompleteSceneLoad()
    {
        Debug.Log("BattleManager 초기화 수행");
        SpawnPlayerDigimon();
        SpawnEnemies().Forget();
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

    private async UniTaskVoid SpawnEnemies()
    {
        _enemyList = GameManager.Instance.GetBattleList();

        Debug.Log($"_enemyList Count : {_enemyList.Count}");
        for (int i = 0; i < _enemyList.Count; i++)
        {
            Vector3 pos = GetPos(i, true);
            Quaternion rot = Quaternion.Euler(0f, 270f, 0f);
            EnemyStatusData data = DigimonDB.Instance.GetEnemyStatusDataById(_enemyList[i]);
            await DigimonSpawner.Instance.SpawnEnemyDigimon(data.ID, data.DigimonName, pos, rot);

            Debug.Log($"EnemyStatusData data : {data.ID} {data.DigimonName}");
        }
    }

    private void ReturnField()
    {
        // 전투 끝나면 호출
        GameManager.Instance.ClearBattleList();
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

    private void EnterInputMode()
    {
        _input.EnableBattle(true);
        _isAttackMode = false;
        _battleUI.HideTargetNumber();
        ShowDollyCam(true);
        ShowBackCam(false);
        _battleUI.ShowInputMode();
    }

    private void EnterAttackMode()
    {
        _isAttackMode = true;
        ShowDollyCam(false);
        ShowBackCam(true);
        _battleUI.HidePlayerPanel();
    }

    private void ShowDollyCam(bool enabled)
    {
        if (enabled)
        {
            _dollyCam.Priority = HIGH_PRIORITY;
        }

        else
        {
            _dollyCam.Priority = LOW_PRIORITY;
        }
    }

    private void ShowBackCam(bool enabled)
    {
        if (enabled)
        {
            Vector3 pos = _playerDigimonGoList[_currentPlayerIndex].transform.position + new Vector3(_vcamPos.x, _vcamPos.y, _vcamPos.z);
            
            _vcam.transform.position = pos;
            _vcam.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
            _vcam.Priority = HIGH_PRIORITY;

            DelayShowTarget().Forget();
        }

        else
        {
            _vcam.Priority = LOW_PRIORITY;
        }
    }

    private async UniTaskVoid DelayShowTarget()
    {
        await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);

        _battleUI.ShowTargetNumber();
    }

    public void ShowDigimon(DigimonStatus status, bool isEnemy)
    {
        _dollyCam.Priority = LOW_PRIORITY;
        _vcam.Priority = HIGH_PRIORITY;

        Vector3 pos = isEnemy ? 
            status.transform.position + Vector3.right * -3f + Vector3.up * 1f :
            status.transform.position + Vector3.right * 3f + Vector3.up * 1f;

        _vcam.transform.position = pos;
        _vcam.transform.rotation = status.transform.rotation * Quaternion.Euler(0f, 180f, 0f);
    }

    public async UniTask ShowMyDigimon()
    {
        _dollyCam.Priority = LOW_PRIORITY;
        _vcam.Priority = HIGH_PRIORITY;

        _vcam.transform.position = _playerStatusList[0].transform.position + Vector3.right * 3f + Vector3.up * 1f;
        _vcam.transform.rotation = _playerStatusList[0].transform.rotation * Quaternion.Euler(0f, 180f, 0f);

        await _playerStatusList[0].Victory();
    }
}
