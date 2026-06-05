using Cinemachine;
using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Unity.VisualScripting;
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
    [SerializeField] private Vector3 _vcamPos;  //공격할 타겟 번호 선택할 때 카메라 오프셋
    private const int HIGH_PRIORITY = 15;
    private const int LOW_PRIORITY = 10;
    // 게임오브젝트
    [SerializeField] private List<GameObject> _playerDigimonGoList = new List<GameObject>();
    [SerializeField] private List<GameObject> _enemyDigimonGoList = new List<GameObject>();

    // DigimonStatus
    [SerializeField] private List<DigimonStatus> _playerStatusList = new List<DigimonStatus>();
    [SerializeField] private List<DigimonStatus> _enemyStatusList = new List<DigimonStatus>();

    //[SerializeField] private CinemachineBrain _brain;
    [SerializeField] private CinemachineVirtualCamera _dollyCam;
    [SerializeField] private CinemachineVirtualCamera _vcam;
    //[SerializeField] private CinemachineVirtualCamera _vcam2;

    private CinemachineBasicMultiChannelPerlin _noise;

    private IReadOnlyList<int> _enemyList;

    private InputManager _input;

    private int _currentPlayerIndex = 0;
    private int _inputCount = 0;

    private bool _isBattleReady = false;

    private List<ActionCommand> _battleCommandList = new List<ActionCommand>();

    private bool _isAttackMode = false;     // 공격 또는 스킬 버튼 눌렀을 때 어택모드 일때 1,2,3 입력 가능, 다른 버튼 눌렀을 때 1,2,3 입력 막으려고 사용

    public List<GameObject> PlayerDigimonGoList => _playerDigimonGoList;
    public List<GameObject> EnemyDigimonGoList => _enemyDigimonGoList;

    public List<DigimonStatus> PlayerStatusList => _playerStatusList;
    public List<DigimonStatus> EnemyStatusList => _enemyStatusList;
    public bool IsEvoCompleted { get; set; } = false;
    
    private bool _isAttack = true;

    private bool _isFinish = false;
    
    private bool _isVictory = false;

    private List<int> _deleteList = new List<int>();
    private void OnEnable()
    {
        _inputCount = 0;

        _playerDigimonGoList.Clear();
        _enemyDigimonGoList.Clear();

        _playerStatusList.Clear();
        _enemyStatusList.Clear();

        _battleCommandList.Clear();

        _noise = _vcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

        SceneLoader.Instance.OnBattleSceneLoaded += OnCompleteSceneLoad;
        DigimonSpawner.Instance.OnPlayerDigimonSpawned += OnCompletePlayerDigimonSpawn;
        DigimonSpawner.Instance.OnEnemyDigimonSpawned += OnCompleteEnemyDigimonSpawn;

        _input = InputManager.Instance;

        _input.OnEvo += HandleEvo;
        _input.OnAttack += HandleAttackWrapper;
        _input.OnSkill += HandleSkillWrapper;
        _input.OnGuard += HandleGuard;
        //_input.OnSelect += HandleSelect;
        _input.OnRun += HandleRun;
        _input.OnPress1 += HandlePress1;
        _input.OnPress2 += HandlePress2;
        _input.OnPress3 += HandlePress3;
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
            _input.OnAttack -= HandleAttackWrapper;
            _input.OnSkill -= HandleSkillWrapper;
            _input.OnGuard -= HandleGuard;
            _input.OnRun -= HandleRun;
            _input.OnPress1 -= HandlePress1;
            _input.OnPress2 -= HandlePress2;
            _input.OnPress3 -= HandlePress3;
        }

    }
    private async void HandleAttackWrapper(bool pressed)
    {
        await HandleAttack(pressed);
    }

    private async void HandleSkillWrapper(bool pressed)
    {
        await HandleSkill(pressed);
    }

    private void HandleEvo(bool pressed)
    {
        // 진화는 행동력 3 소모

        DigimonStatus status = _playerStatusList[_inputCount];
        
        if (status.Grade == EGrade.Baby)
        {
            BattleUIManager.Instance.ShowBattleMsgAsync("유년기는 레벨 5에서 자동 진화").Forget();
            return;
        }

        if (status.ActionCount < 3)
        {
            BattleUIManager.Instance.ShowBattleMsgAsync("행동력 부족 (행동력 3 필요)").Forget();
            return;
        }

        if (status.EvoID == -1)
        {
            BattleUIManager.Instance.ShowBattleMsgAsync("진화체 없음").Forget();
            return;
        }

        if (status.Level < 7)
        {
            BattleUIManager.Instance.ShowBattleMsgAsync("레벨 7 이상부터 진화 가능").Forget();
            return;
        }

        if (!_isAttackMode)
        {
            Debug.Log("진화");
            ActionCommand c = new ActionCommand(_playerStatusList[_inputCount], EActionType.Evo, null, false);
            _battleCommandList.Add(c);

            _isAttackMode = true;
            BattleUIManager.Instance.TogglePlayerPanel(false);

            CheckNext();
        }
    }

    private async UniTask HandleAttack(bool pressed)
    {
        // 공격은 행동력 1 소모
        if (_playerStatusList[_inputCount].ActionCount <= 0)
        {
            _input.EnableBattle(false);
            BattleUIManager.Instance.TogglePlayerPanel(false);

            await BattleUIManager.Instance.ShowBattleMsgAsync("행동력이 부족합니다. [행동력 1 필요]");

            _input.EnableBattle(true);
            BattleUIManager.Instance.TogglePlayerPanel(true);
            return;
        }
        
        if (!_isAttackMode)
        {
            Debug.Log("HandleAttack 실행");
            _isAttack = true;
            EnterAttackMode();
        }
    }

    private async UniTask HandleSkill(bool pressed)
    {
        Debug.Log($"HandleSkill - _inputCount : {_inputCount}");
        if (_inputCount < 0 || _inputCount >= _playerStatusList.Count)
        {
            Debug.LogWarning($"_inputCount {_inputCount} 가 리스트 범위를 벗어남");
            return;
        }

        Debug.Log($"_playerStatusList[_inputCount] : {_playerStatusList[_inputCount]}");
        Debug.Log($"ActionCount : {_playerStatusList[_inputCount].ActionCount}");

        // 유년기는 스킬사용 불가
        if (_playerStatusList[_inputCount].Grade == EGrade.Baby)
        {
            _input.EnableBattle(false);
            BattleUIManager.Instance.TogglePlayerPanel(false);

            await BattleUIManager.Instance.ShowBattleMsgAsync("유년기는 스킬을 사용할 수 없습니다.");

            _input.EnableBattle(true);
            BattleUIManager.Instance.TogglePlayerPanel(true);
            return;
        }

        // 스킬은 행동력 2 소모
        if (_playerStatusList[_inputCount].ActionCount <= 1)
        {
            _input.EnableBattle(false);
            BattleUIManager.Instance.TogglePlayerPanel(false);

            await BattleUIManager.Instance.ShowBattleMsgAsync("행동력이 부족합니다. [행동력 2 필요]");

            _input.EnableBattle(true);
            BattleUIManager.Instance.TogglePlayerPanel(true);
            return;
        }

        if (!_isAttackMode)
        {
            Debug.Log("HandleSkill 실행");
            _isAttack = false;
            EnterAttackMode();
        }
    }

    private void HandleGuard(bool pressed)
    {
        // 행동력 0 소모
        if (!_isAttackMode)
        {
            Debug.Log("가드");
            ActionCommand c = new ActionCommand(_playerStatusList[_inputCount], EActionType.Guard, null, false);
            _battleCommandList.Add(c);

            _isAttackMode = true;
            BattleUIManager.Instance.TogglePlayerPanel(false);

            CheckNext();
        }
    }

    private void HandleRun(bool pressed)
    {
        if (!_isAttackMode)
        {
            Debug.Log("도망");
            _input.EnableBattle(false);
            BattleUIManager.Instance.TogglePlayerPanel(false);
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

            CheckNext();
        }
    }

    private bool CheckNext()
    {
        _inputCount++;

        BattleUIManager.Instance.HideTargetNumber();

        if (_inputCount < _playerStatusList.Count)
        {
            Debug.Log("다음 디지몬 입력");
            EnterInputMode();
            BattleUIManager.Instance.UpdateProfile(_playerStatusList[_inputCount]);
            return true;
        }

        // 입력 끝
        return false;
    }

    

    private void Start()
    {
        _input.SwitchToBattleMap();
    }
    void Update()
    {
        if (_isFinish)
            return;

        if (_playerStatusList.Count == 0 || _enemyStatusList.Count == 0)
            return;

        if (_inputCount < _playerStatusList.Count)
            return;
        
        if (!_isBattleReady)
        {
            Debug.Log("Update 에서 PlayBattle().Forget(); 실행 전");
            _isBattleReady = true;
            PlayBattle().Forget();
        }
    }

    public void UpdatePlayerStatusList()
    {
        IReadOnlyList<DigimonStatus> list = GameManager.Instance.GetDigimonStatusList();

        for (int i = 0; i <  _playerStatusList.Count; i++)
        {
            for (int j = 0; j < list.Count; j++)
            {
                if (_playerStatusList[i].ID == list[j].ID)
                {
                    if (_playerStatusList[i].Level < list[j].Level)
                    {
                        Debug.Log($"UpdatePlayerStatus - {_playerStatusList[i].DigimonNameKor}");
                        _playerStatusList[i].HP = list[j].HP;
                        _playerStatusList[i].CurrentHP = list[j].HP;
                        _playerStatusList[i].Level = list[j].Level;
                        _playerStatusList[i].ATK = list[j].ATK;
                        _playerStatusList[i].DEF = list[j].DEF;
                        _playerStatusList[i].INT = list[j].INT;
                        _playerStatusList[i].SPD = list[j].SPD;
                    }

                }
            }

        }
    }
    private async UniTaskVoid PlayBattle()
    {
        _input.EnableBattle(false);

        SetEnemyCommand();

        _battleCommandList.Sort((a, b) => b.Actor.SPD.CompareTo(a.Actor.SPD));

        for (int i = 0; i < _battleCommandList.Count; i++)
        {
            if (_battleCommandList[i].Actor.CurrentHP <= 0)
                continue;

            await ExecuteCommand(_battleCommandList[i]);    // 액션 끝나고 매번 전투가 끝났는지 체크

            if (_isFinish) // 전투 끝났으면 빠져나옴
                break;

            await BattleUIManager.Instance.RemoveDead();
            await BattleUIManager.Instance.UpdateTurn();
        }

        ReadyNextTurn();

        //_isFinish = await CheckBattleFinish();
        if (_isFinish)
        {
            // 전투 승리 OR 패배
            ReturnField();
            return;
        }

        EnterInputMode();
    }

    private void DeletePrevDigimon()
    {
        var list = GameManager.Instance.GetMutableDigimonStatusList();

        for (int i = 0; i < _deleteList.Count; i++)
        {
            for (int j = 0; j < _playerDigimonGoList.Count; j++)
            {
                // 비활성화 된 것 활성화
                _playerDigimonGoList[j].SetActive(true);

                DigimonStatus status = _playerDigimonGoList[j].GetComponent<DigimonStatus>();

                if (status.ID == _deleteList[i])
                {
                    _playerDigimonGoList[j].SetActive(false);
                    _playerDigimonGoList.RemoveAt(j);
                }
            }

            for (int j = 0; j < list.Count; j++)
            {
                if (list[j].ID == _deleteList[i])
                {
                    list.RemoveAt(j);
                    break;
                }
            }
        }
    }

    private void ReadyNextTurn()
    {
        // 플레이어는 행동력 1 증가
        // 죽은애는 리스트에서 제외
        Debug.Log($"ReadyNextTurn - _playerStatusList.Count : {_playerStatusList.Count}");
        for (int i = _playerStatusList.Count - 1; i >= 0; i--)
        {
            DigimonStatus status = _playerStatusList[i];

            if (status.CurrentHP <= 0)
            {
                //_playerStatusList.Remove(status);
                _playerStatusList.RemoveAt(i);
                _playerDigimonGoList[i].SetActive(false);
                _playerDigimonGoList.RemoveAt(i);
                continue;
            }

            // 삭제 처리
            //for (int j = 0; j < _deleteList.Count; j++)
            //{
            //    if (status.ID == _deleteList[j])
            //    {
            //        _playerStatusList.RemoveAt(i);
            //        break;
            //    }
            //}

            //if (_deleteList.Count > 0)
            //    DeletePrevDigimon();

            status.ActionCount++;
        }

        for (int i = _enemyStatusList.Count - 1; i >= 0; i--)
        {
            DigimonStatus status = _enemyStatusList[i];

            if (status.CurrentHP <= 0)
            {
                //_enemyStatusList.Remove(status);
                _enemyStatusList.RemoveAt(i);
                _enemyDigimonGoList[i].SetActive(false);
                _enemyDigimonGoList.RemoveAt(i);
                continue;
            }

            //status.ActionCount++;
        }


        _playerStatusList.Sort((a, b) => b.SPD.CompareTo(a.SPD));

        if (_playerStatusList.Count > 0)
            BattleUIManager.Instance.UpdateProfile(_playerStatusList[0]);

        BattleUIManager.Instance.ResetTurnImage();  //턴 끝나고 턴UI 다시 설정 (레벨업 또는 진화하면 스탯 바뀜)

        _battleCommandList.Clear();
        _deleteList.Clear();
        _inputCount = 0;
        _isBattleReady = false;
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
            _isVictory = false;
            BattleUIManager.Instance.HideAllUI();
            return true;
        }

        if (isEnemyDead)
        {
            // 승
            Debug.Log("승");
            _isVictory = true;
            BattleUIManager.Instance.HideAllUI();
            await ShowMyDigimon();
            return true;
        }

        // 진행중
        Debug.Log("진행중");
        return false;
    }

    private async UniTask ExecuteCommand(ActionCommand cmd)
    {
        // 전부 살아있는 애들만 있음

        if (_isFinish)
        {
            return;
        }

        if (cmd.Actor.CurrentHP <= 0)
        {
            return;
        }

        Debug.Log($"ExecuteCommand - Actor : {cmd.Actor.DigimonNameKor} ATK : {cmd.Actor.ATK} DEF : {cmd.Actor.DEF}");
        switch (cmd.ActionType)
        {
            case EActionType.Attack:
                ShowDigimon(cmd.Actor, cmd.IsEnemy);
                await cmd.Actor.Attack(cmd.Target, !cmd.IsEnemy);   //Actor말고 Target이 enemy인지 확인
                cmd.Actor.ActionCount -= 1;
                break;
            case EActionType.Skill:
                ShowDigimon(cmd.Actor, cmd.IsEnemy);
                await cmd.Actor.Skill(cmd.Target, !cmd.IsEnemy);    //Actor말고 Target이 enemy인지 확인
                cmd.Actor.ActionCount -= 2;
                break;
            case EActionType.Guard:
                ShowDigimon(cmd.Actor, cmd.IsEnemy);
                await cmd.Actor.Guard();
                cmd.Actor.ActionCount += 1;
                break;
            case EActionType.Evo:
                ShowDigimon(cmd.Actor, cmd.IsEnemy);
                await cmd.Actor.Evo();
                cmd.Actor.ActionCount -= 3;
                break;
        }
        
        _isFinish = await CheckBattleFinish();

    }

    private void SetEnemyCommand()
    {
        for (int i = 0; i < _enemyStatusList.Count; i++)
        {
            DigimonStatus actor = _enemyStatusList[i];

            DigimonStatus target = _playerStatusList[Random.Range(0, _playerStatusList.Count)];

            //int idx = Random.Range(0, _playerStatusList.Count);
            //DigimonStatus target = _playerStatusList[idx];

            // 공격 또는 방어 랜덤으로 수행
            int r = Random.Range(0, 10);

            Debug.Log($"SetEnemyCommand - {actor.DigimonNameKor} {actor.UID}");
            Debug.Log($"적 공격 스킬 방어 random : {r}");
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
                if (r < 3)
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

    private void OnCompletePlayerDigimonSpawn(GameObject go, DigimonStatus status, ESpawnType type)
    {
        status.ActionCount = 5;
        
        if (type == ESpawnType.LevelUp)
        {
            Debug.Log("BattleManager - 레벨업해서 진화");

            // 배틀 중 레벨업으로 인한 진화 디지몬 스폰
            // 턴 끝나면 기존 디지몬 제거, 진화 디지몬 활성화

            //List<DigimonStatus> list = GameManager.Instance.GetMutableDigimonStatusList();
            //go.SetActive(false);
            //_playerDigimonGoList.Add(go);
            //_playerStatusList.Add(status);
            //list.Add(status);

            //_deleteList.Add(status.PrevID); //동시에 2마리가 진화했을 때 처리를 위한 리스트

            int idx = _playerStatusList.FindIndex(s => s.ID == status.PrevID);
            if (idx >= 0)
            {
                // 기존 오브젝트 비활성화
                _playerDigimonGoList[idx].SetActive(false);

                // 같은 인덱스에 새 디지몬 교체
                _playerDigimonGoList[idx] = go;
                _playerStatusList[idx] = status;

                // GameManager 리스트에서 기존 PrevID 제거
                var list = GameManager.Instance.GetMutableDigimonStatusList();
                int removeIdx = list.FindIndex(s => s.ID == status.PrevID);
                if (removeIdx >= 0)
                    list.RemoveAt(removeIdx);

                // 새 진화체 추가
                list.Add(status);

                EffectManager.Instance.PlayEvo(go.transform.position);
                BattleUIManager.Instance.UpdateEvoImage(status);
            }
            else
            {
                Debug.Log("레벨업해서 진화했는데 인덱스 못찾음");
                // 못 찾으면 그냥 추가 (예외 처리)
                //_playerDigimonGoList.Add(go);
                //_playerStatusList.Add(status);
                //GameManager.Instance.GetMutableDigimonStatusList().Add(status);
            }

        }

        else if (type == ESpawnType.Evo)
        {
            Debug.Log("BattleManager - Evo 입력으로 진화");

            status.IncreaseStatus(status.Level - 1);

            int idx = _playerStatusList.FindIndex(s => s.ID == status.PrevID);
            if (idx >= 0)
            {
                _playerDigimonGoList[idx].SetActive(false);

                _playerDigimonGoList[idx] = go;
                _playerStatusList[idx] = status;
            }

            //for (int i = 0; i < _playerStatusList.Count; i++)
            //{
            //    if (_playerStatusList[i].ID == status.PrevID)
            //    {
            //        _playerStatusList[i].gameObject.SetActive(false);

            //        _playerStatusList.RemoveAt(i);


            //        _playerStatusList.Insert(i, status);
            //        _playerDigimonGoList.RemoveAt(i);
            //        _playerDigimonGoList.Add(go);
            //        break;
            //    }
            //}

            EffectManager.Instance.PlayEvo(go.transform.position);
            IsEvoCompleted = true;
            BattleUIManager.Instance.UpdateEvoImage(status);
            //_playerStatusList.Sort((a, b) => b.SPD.CompareTo(a.SPD));
        }

        else
        {
            // 전투에 진입했을 때 스폰 후에 여기로 진입
            _playerDigimonGoList.Add(go);
            _playerStatusList.Add(status);

            // 모두 스폰되면 SPD 순서로 정렬
            if (_playerStatusList.Count == GameManager.Instance.GetDigimonStatusList().Count)
            {
                _playerStatusList.Sort((a, b) => b.SPD.CompareTo(a.SPD));
            }
        }
         
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

    public Vector3 GetPos(int idx, bool isEnemy)
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
            DigimonSpawner.Instance.SpawnPlayerDigimon(i, statusList[i].DigimonName, pos, rot, ESpawnType.Normal);
        }
    }

    private async UniTaskVoid SpawnEnemies()
    {
        _enemyList = GameManager.Instance.GetBattleList();

        for (int i = 0; i < _enemyList.Count; i++)
        {
            Vector3 pos = GetPos(i, true);
            Quaternion rot = Quaternion.Euler(0f, 270f, 0f);
            EnemyStatusData data = DigimonDB.Instance.GetEnemyStatusDataById(_enemyList[i]);
            await DigimonSpawner.Instance.SpawnEnemyDigimon(data.ID, data.DigimonName, pos, rot);
        }
    }

    private void ReturnField()
    {
        // 전투 끝나면 호출
        if (_isVictory)
        {
            QuestManager.Instance.QuestCheckList[1] = true;   // 두 번째 퀘스트 승리하기 클리어
            
            // id로 데빌몬, 매그너몬, 루체몬 구분해서 퀘스트 클리어 처리
            if (GameManager.Instance.GetBattleList()[0] == 7011)        // 4번째 퀘스트 데빌몬
            {
                QuestManager.Instance.QuestCheckList[3] = true;
            }

            else if (GameManager.Instance.GetBattleList()[0] == 7017)   // 5번째 퀘스트 매그너몬
            {
                QuestManager.Instance.QuestCheckList[4] = true;
            }

            else if (GameManager.Instance.GetBattleList()[0] == 6019)   // 6번째 퀘스트 루체몬
            {
                QuestManager.Instance.QuestCheckList[5] = true;
            }
        }

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
        BattleUIManager.Instance.HideTargetNumber();
        ShowDollyCam(true);
        ShowBackCam(false);
        BattleUIManager.Instance.TogglePlayerPanel(true);
    }

    private void EnterAttackMode()
    {
        _isAttackMode = true;
        ShowDollyCam(false);
        ShowBackCam(true);
        BattleUIManager.Instance.TogglePlayerPanel(false);
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
            if (_playerStatusList[_inputCount] == null)
            {
                Debug.Log("Status is null or destroyed");
                return;
            }

            Vector3 pos = _playerStatusList[_inputCount].transform.position + new Vector3(_vcamPos.x, _vcamPos.y, _vcamPos.z);
            
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
        // 가상카메라 전환 된 후에 UI 표시

        await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);

        BattleUIManager.Instance.ShowTargetNumber();
    }

    public void ShowDigimon(DigimonStatus status, bool isEnemy)
    {
        Vector3 pos = isEnemy ? 
            status.transform.position + Vector3.right * -3f + Vector3.up * 1f :
            status.transform.position + Vector3.right * 3f + Vector3.up * 1f;

        Debug.Log($"ShowDigimon : {status.transform.position}");
        Debug.Log($"ShowDigimon pos: {pos}");

        _vcam.transform.position = pos;
        _vcam.transform.rotation = status.transform.rotation * Quaternion.Euler(0f, 180f, 0f);

        _dollyCam.Priority = LOW_PRIORITY;
        _vcam.Priority = HIGH_PRIORITY;
    }

    public async UniTask ShowSkillCam(bool isEnemy)
    {
        // 타겟이 isEnemy이기 때문에 actor는 반대로 사용

        Vector3 startPos = _vcam.transform.position;
        Vector3 targetPos = _vcam.transform.position + _vcam.transform.forward * -3f + Vector3.up * 3f;

        _vcam.transform.rotation = Quaternion.Euler(30f, _vcam.transform.eulerAngles.y, _vcam.transform.eulerAngles.z);
        float duration = 0.3f;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            _vcam.transform.position = Vector3.Lerp(startPos, targetPos, t);
            elapsed += Time.deltaTime;

            await UniTask.Yield();
        }

        _vcam.transform.position = targetPos;

        //await UniTask.Delay(1000);
    }

    public async UniTask ShowMyDigimon()
    {
        _dollyCam.Priority = LOW_PRIORITY;
        _vcam.Priority = HIGH_PRIORITY;

        int idx = -1;
        for (int i = 0; i < _playerStatusList.Count; i++)
        {
            if (_playerStatusList[i].CurrentHP > 0)
            {
                _playerStatusList[i].Victory();

                if (idx == -1)
                    idx = i;
            }
        }

        _vcam.transform.position = _playerStatusList[idx].transform.position + Vector3.right * 3f + Vector3.up * 1f;
        _vcam.transform.rotation = _playerStatusList[idx].transform.rotation * Quaternion.Euler(0f, 180f, 0f);

        await UniTask.Delay(2000);
        //await _playerStatusList[idx].Victory();
    }

    public async UniTask ShakeCameraHorizontalAsync()
    {
        _noise.m_AmplitudeGain = 1f;
        _noise.m_FrequencyGain = 1f;

        await UniTask.Delay(500);

        _noise.m_AmplitudeGain = 0f;
        _noise.m_FrequencyGain = 0f;
    }
}
