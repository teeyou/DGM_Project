using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

public class DigimonStatus : MonoBehaviour
{
    private const int DELAY_MESSAGE_TIME = 1500;

    [SerializeField] private int id;
    [SerializeField] private string digimonNameKor;
    [SerializeField] private string digimonName;
    [SerializeField] private EAttribute attr;
    [SerializeField] private string attrKor;
    [SerializeField] private EGrade grade;
    [SerializeField] private string gradeKor;
    [SerializeField] private EType type;
    [SerializeField] private string typeKor;

    [SerializeField] private int level;
    [SerializeField] private int hp;
    [SerializeField] private int atk;
    [SerializeField] private int def;
    [SerializeField] private int intel;
    [SerializeField] private int speed;

    //[SerializeField] private float criticalRate;
    //[SerializeField] private float dodgeRate;

    [SerializeField] private GrowthType growthType;
    [SerializeField] private string growthTypeKor;
    [SerializeField] private int evo;
    [SerializeField] private int prev;

    [SerializeField] private int _exp;
    private int _requiredExp;

    private int _uid;
    private int _currentHP;
    private int _actionCount;

    private Animator _animator;

    private Effect _effect;

    private bool _isSkillFinished = false;
    private bool _isRookieEvolved = false;
    public void Init(DigimonStatus data)
    {
        this.id = data.id;
        this.digimonNameKor = data.digimonNameKor;
        this.digimonName = data.digimonName;
        this.attr = data.attr;
        this.attrKor = data.attrKor;
        this.grade = data.grade;
        this.gradeKor = data.gradeKor;
        this.type = data.type;
        this.typeKor = data.typeKor;
        this.level = data.level;

        this.hp = data.hp;
        this.atk = data.atk;
        this.def = data.def;
        this.intel = data.intel;
        this.speed = data.speed;


        this.growthType = data.growthType;
        this.growthTypeKor = data.growthTypeKor;
        this.evo = data.evo;
        this.prev = data.prev;

        _currentHP = data.hp;

        _actionCount = 5;

        _exp = 0;
        _requiredExp = LevelSystem.Instance.GetRequiredEXP(data.level);

        _uid = gameObject.GetInstanceID();
    }
    public void Init(StatusData data, GrowthType growthType)
    {
        id = data.ID;
        digimonNameKor = data.KorName;
        digimonName = data.DigimonName;
        attr = (EAttribute)Enum.Parse(typeof(EAttribute), data.Attr);
        attrKor = data.KorAttr;
        grade = (EGrade)Enum.Parse(typeof(EGrade), data.Grade);
        gradeKor = data.KorGrade;
        type = (EType)Enum.Parse(typeof(EType), data.Type);
        typeKor = data.KorType;

        hp = data.BaseHP;
        atk = data.BaseATK;
        def = data.BaseDEF;
        intel = data.BaseINT;
        speed = data.BaseSPD;

        this.growthType = growthType;
        this.growthTypeKor = data.KorGrowthType;
        this.evo = data.Evo;

        _currentHP = data.BaseHP;
        _actionCount = 5;

        _uid = gameObject.GetInstanceID();

        if (data is EnemyStatusData enemyData)
        {
            level = enemyData.Level;
            _exp = enemyData.EXP;
        }

        else
        {
            prev = data.Prev;

            _exp = 0;

            if (grade == EGrade.Baby)
                level = 1;

            else if (grade == EGrade.Rookie)
            {
                level = 5; // 성장기
            }
            else if (grade == EGrade.Champion)
            {
                //level = 10;
                var list = GameManager.Instance.GetDigimonStatusList();
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i].ID == PrevID)
                    {
                        level = list[i].Level;
                        _exp = list[i].EXP;
                        break;
                    }
                }
            }

            _requiredExp = LevelSystem.Instance.GetRequiredEXP(level);  // 레벨업 필요 경험치
        }
    }

    public int ID => id;
    public int UID => _uid;
    public string DigimonNameKor => digimonNameKor;
    public string DigimonName => digimonName;
    public EAttribute Attr => attr;
    public string AttrKor => attrKor;
    public EGrade Grade => grade;
    public string GradeKor => gradeKor;
    public EType Type => type;
    public string TypeKor => typeKor;

    public int Level { get { return level; } set { level = value; } }
    public int HP { get { return hp; } set { hp = value; } }
    
    public int ATK { get { return atk; } set { atk = value; } } 

    public int DEF { get { return def; } set { def = value; } }

    public int INT { get { return intel; } set { intel = value; } }
    public int SPD { get { return speed; } set { speed = value; } }
    public float CriticalRate => Mathf.Min(0.5f, 0.05f + (0.002f * INT));
    public float DodgeRate => Mathf.Min(0.3f, 0.02f + (0.001f * SPD));

    public GrowthType GrowthType => growthType;
    public string GrowthTypeKor => growthTypeKor;
    
    public int EvoID => evo;
    public int PrevID => prev;

    public int CurrentHP { get { return _currentHP; } set { _currentHP = value; } }
    public int ActionCount { get { return _actionCount; } set { _actionCount = value; } }

    public int EXP { get { return _exp; } set { _exp = value; } }
    public int RequiredEXP { get { return _requiredExp; } set { _requiredExp = value; } }
    private void Awake()
    {
        //_currentHP = HP;
        //_actionCount = 5;
        _animator = GetComponent<Animator>();
    }

    public void Victory()
    {
        _animator.SetTrigger("Victory");
    }

    //public async UniTask Victory()
    //{
    //    _animator.SetTrigger("Victory");

    //    await UniTask.Delay(2000);
    //}

    public void IncreaseEXP(int exp)
    {
        if (Level >= 50)
        {
            Debug.Log("최대 레벨 경험치 X");
            EXP = 0;
            return;
        }

        EXP += exp;

        var battleList = BattleManager.Instance.PlayerStatusList;
        int idx = -1;

        for (int i = 0; i < battleList.Count; i++)
        {
            DigimonStatus s = battleList[i];
            if (ID == s.PrevID)
            {
                idx = i;
                break;
            }
        }

        string msg = $"{digimonNameKor} EXP +{exp}";
        if (idx != -1)
        {
            msg = $"{battleList[idx].DigimonNameKor} EXP +{exp}";
        }

        BattleUIManager.Instance.EnqueueLog(msg);
        //BattleUIManager.Instance.ShowLogAsync($"{digimonNameKor} EXP +{exp}", 2000).Forget();

        CheckLevelUp();
    }

    public void CheckLevelUp()
    {
        if (Level >= 50)
        {
            Debug.Log("최대 레벨 달성");
            EXP = 0;
            return;
        }

        while (EXP >= RequiredEXP)
        {
            if (Level >= 50)
            {
                Debug.Log("최대 레벨 달성");
                EXP = 0;
                return;
            }

            LevelUp();

            EXP -= RequiredEXP;
            EXP = EXP <= 0 ? 0 : EXP;

            RequiredEXP = LevelSystem.Instance.GetRequiredEXP(Level);
            Debug.Log($"Level : {Level} RequiredEXP : {RequiredEXP}");
        }
    }

    public void IncreaseStatus(int count)
    {
        for (int i = 0; i < count; i++)
        {
            HP += growthType.HPInc;
            ATK += growthType.ATKInc;
            DEF += growthType.DEFInc;
            INT += growthType.INTInc;
            SPD += growthType.SPDInc;
        }

        CurrentHP = HP;
    }

    private void LevelUp()
    {
        Debug.Log("LevelUp");
        Level++;
        IncreaseStatus(1);
        AudioManager.Instance.PlaySFX("LevelUpSFX");
        var battleList = BattleManager.Instance.PlayerStatusList;
        int idx = -1;

        for (int i = 0; i < battleList.Count; i++)
        {
            DigimonStatus s = battleList[i];
            if (ID == s.PrevID)
            {
                Debug.Log("진화체도 레벨업");
                s.Level++;
                s.IncreaseStatus(1);
                idx = i;
                break;
            }
        }

        string msg = $"{digimonNameKor} LEVEL UP";
        if (idx != -1)
        {
            msg = $"{battleList[idx].DigimonNameKor} LEVEL UP";
        }
        BattleUIManager.Instance.EnqueueLog(msg);
        //BattleUIManager.Instance.ShowLogAsync($"{digimonNameKor} LEVEL UP", 2000).Forget();

        BattleManager.Instance.UpdatePlayerStatusList();
        
        // 진화 가능한 상황
        CheckRookieEvolution();
    }

    private void CheckRookieEvolution()
    {
        // 성장기로 진화
        if (_isRookieEvolved)
            return;

        if (grade == EGrade.Baby && Level >= 5)
        {
            Debug.Log("성장기로 진화");
            _isRookieEvolved = true;
            //IReadOnlyList<DigimonStatus> list = GameManager.Instance.GetDigimonStatusList();
            List<DigimonStatus> list = BattleManager.Instance.PlayerStatusList;
            int idx = -1;

            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].ID == ID)
                {
                    idx = i;
                    break;
                }
            }

            //Transform t = BattleManager.Instance.PlayerDigimonGoList[idx].transform;
            StatusData data = DigimonDB.Instance.GetStatusDataById(evo);

            Vector3 pos = BattleManager.Instance.GetPos(idx, false);
            Quaternion rot = Quaternion.Euler(0f, 90f, 0f);
            DigimonSpawner.Instance.SpawnPlayerDigimon(-1, data.DigimonName, pos, rot, ESpawnType.LevelUp);

            string msg = $"{digimonNameKor} 진화";
            BattleUIManager.Instance.EnqueueLog(msg);
            //BattleUIManager.Instance.ShowLogAsync(msg).Forget();
        }
    }

    public async UniTask Attack(DigimonStatus target, bool isEnemy)
    {
        //target이 Enemy ?

        // 타겟이 플레이어일 때
        if (!isEnemy)
        {
            if (!target.gameObject.activeSelf)
            {
                Debug.Log("진화해서 공격 대상이 비활성화 된 상태");

                var list = BattleManager.Instance.PlayerStatusList;

                for (int i = 0; i < list.Count; i++)
                {
                    if (target.EvoID == list[i].ID)
                    {
                        Debug.Log($"공격 대상의 진화체 : {list[i].DigimonNameKor}");
                        target = list[i];
                        break;
                    }
                }
            }
            
        }

        if (target.CurrentHP <= 0)
        {
            string s = "대상이 이미 기절했다.";
            BattleUIManager.Instance.ShowBattleMsg(true, s);

            await UniTask.Delay(DELAY_MESSAGE_TIME);

            BattleUIManager.Instance.ShowBattleMsg(false);

            //await UniTask.Delay(500);
            return;
        }

        _animator.SetTrigger("Attack");

        string msg = isEnemy ? $"<color={ColorTable.Green}>{digimonNameKor}</color>이 <color={ColorTable.Red}>{target.digimonNameKor}</color>에게 공격"
                : $"<color={ColorTable.Red}>{digimonNameKor}</color>이 <color={ColorTable.Green}>{target.digimonNameKor}</color>에게 공격";

        if (type == EType.Fight)
        {
            msg = isEnemy ? $"[투지] <color={ColorTable.Green}>{digimonNameKor}</color>이 <color={ColorTable.Red}>{target.digimonNameKor}</color>에게 공격"
                : $"[투지] <color={ColorTable.Red}>{digimonNameKor}</color>이 <color={ColorTable.Green}>{target.digimonNameKor}</color>에게 공격";
        }
    
        BattleUIManager.Instance.ShowBattleMsg(true, msg);

        await UniTask.Delay(DELAY_MESSAGE_TIME);

        BattleUIManager.Instance.ShowBattleMsg(false);

        //await UniTask.Delay(500);

        BattleManager.Instance.ShowDigimon(target, isEnemy);

        (int damage, bool isCritical) = DamageCalculator.Calculate(this, target, false);
        await target.TakeDamage(damage, isCritical, isEnemy, this);
    }

    public async UniTask Skill(DigimonStatus target, bool isEnemy)
    {
        //target이 Enemy ?

        // 타겟이 플레이어일 때
        if (!isEnemy)
        {
            if (!target.gameObject.activeSelf)
            {
                Debug.Log("진화해서 공격 대상이 비활성화 된 상태");

                var list = BattleManager.Instance.PlayerStatusList;

                for (int i = 0; i < list.Count; i++)
                {
                    if (target.EvoID == list[i].ID)
                    {
                        Debug.Log($"공격 대상의 진화체 : {list[i].DigimonNameKor}");
                        target = list[i];
                        break;
                    }
                }
            }

        }

        if (target.CurrentHP <= 0)
        {
            string s = "대상이 이미 기절했다.";
            BattleUIManager.Instance.ShowBattleMsg(true, s);

            await UniTask.Delay(DELAY_MESSAGE_TIME);

            BattleUIManager.Instance.ShowBattleMsg(false);

            //await UniTask.Delay(500);

            return;
        }

        string msg = isEnemy ? $"<color={ColorTable.Green}>{digimonNameKor}</color>이 <color={ColorTable.Red}>{target.digimonNameKor}</color>에게 스킬"
                : $"<color={ColorTable.Red}>{digimonNameKor}</color>이 <color={ColorTable.Green}>{target.digimonNameKor}</color>에게 스킬";

        if (type == EType.Fight)
        {
            msg = isEnemy ? $"[투지] <color={ColorTable.Green}>{digimonNameKor}</color>이 <color={ColorTable.Red}>{target.digimonNameKor}</color>에게 스킬"
                : $"[투지] <color={ColorTable.Red}>{digimonNameKor}</color>이 <color={ColorTable.Green}>{target.digimonNameKor}</color>에게 스킬";
        }

        //_isSkillFinished = false;
        _animator.SetTrigger("Skill");

        AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
        float animLength = stateInfo.length;
        Debug.Log($"Skill 애니메이션 길이: {animLength}");

        BattleUIManager.Instance.ShowBattleMsg(true, msg);

        await UniTask.Delay(DELAY_MESSAGE_TIME);

        BattleUIManager.Instance.ShowBattleMsg(false);

        BattleManager.Instance.ShowSkillCam(isEnemy).Forget();

        Debug.Log("스킬 끝날때까지 대기");

        // 애니메이션 길이만큼 대기
        await UniTask.Delay((int)(animLength * 1000));

        //if (digimonName == "Veemon")
        //{
        //    await UniTask.Delay(1000);
        //}

        //else
        //    await UniTask.WaitUntil(() => _isSkillFinished);

        //await UniTask.Delay(2000);

        BattleManager.Instance.ShowDigimon(target, isEnemy);

        (int damage, bool isCritical) = DamageCalculator.Calculate(this, target, true);

        await target.TakeDamage(damage, isCritical, isEnemy, this);
    }

    public async UniTask Guard()
    {
        _animator.SetTrigger("Guard");

        DEF += 10;

        string msg = $"{digimonNameKor} DEF 10 증가";

        if (type == EType.Endurance)
        {
            DEF += 10;
            msg = $"[인내] {digimonNameKor} DEF 20 증가";
        }
      
        BattleUIManager.Instance.ShowBattleMsg(true, msg);

        await UniTask.Delay(DELAY_MESSAGE_TIME);

        BattleUIManager.Instance.ShowBattleMsg(false);

        //await UniTask.Delay(500);
    }

    public async UniTask Evo()
    {
        StatusData data = DigimonDB.Instance.GetStatusDataById(EvoID);  //진화 할 디지몬 데이터
        
        Debug.Log($"name : {data.DigimonName} position : {transform.position}");

        DigimonSpawner.Instance.SpawnPlayerDigimon(-1, data.DigimonName, transform.position, transform.rotation, ESpawnType.Evo);

        Debug.Log("Evo - 진화 완료까지 대기");
        
        await UniTask.WaitUntil(() => BattleManager.Instance.IsEvoCompleted);

        Debug.Log("Evo - 진화 완료");
        BattleManager.Instance.IsEvoCompleted = false;
    }

    public void Run()
    {
        RunAsync(1f).Forget();
    }

    private async UniTaskVoid RunAsync(float duration)
    {
        transform.rotation = Quaternion.Euler(0f, 270f, 0f);
        float elapsed = 0f;
        float speed = 5f;
        _animator.SetBool("Move", true);

        string msg = "도망쳤다.";
        BattleUIManager.Instance.ShowBattleMsg(true, msg);
        while (elapsed < duration)
        {
            transform.position += transform.forward * speed * Time.deltaTime;
            elapsed += Time.deltaTime;

            await UniTask.Yield();
        }

        BattleUIManager.Instance.ShowBattleMsg(false);
    }

    public async UniTask TakeDamage(int damage, bool isCritical, bool isEnemy, DigimonStatus attacker)
    {
        if (_effect == null)
        {
            _effect = GetComponent<Effect>();
        }

        float rand = UnityEngine.Random.Range(0f, 1f);
        
        float dodgeRate = DodgeRate;

        if (type == EType.Agility)
        {
            dodgeRate += 0.05f;
        }

        // 회피
        if (dodgeRate > rand)
        {
            BattleManager.Instance.ShakeCameraHorizontalAsync().Forget();

            string msg = $"{digimonNameKor} 회피";
            
            if (type == EType.Agility)
            {
                msg = $"[민첩] {digimonNameKor} 회피";
            }

            BattleUIManager.Instance.ShowBattleMsg(true, msg);
            await UniTask.Delay(DELAY_MESSAGE_TIME);
            BattleUIManager.Instance.ShowBattleMsg(false);
            //await UniTask.Delay(500);
            return;
        }

        // Hit VFX
        // 유년기는 여기서 처리
        // 성장기부터는 각 스크립트에서 처리
        if (grade == EGrade.Baby)
        {
            Vector3 pos = transform.position;
            pos.x += 0.2f;
            pos.y = 0.2f;

            EffectManager.Instance.PlayHit("CommonHit", pos, Quaternion.identity);
            AudioManager.Instance.PlaySFX("HitSFX");
        }

        if (_effect != null)
            _effect.PlayHit();

        CurrentHP -= damage;
        CurrentHP = Mathf.Clamp(CurrentHP, 0, HP);

        if (isEnemy)
            BattleUIManager.Instance.UpdateEnemyHP(this);


        // 기절한 경우
        if (CurrentHP <= 0)
        {
            _animator.SetTrigger("Die");

            string msg = isEnemy? $"<color={ColorTable.Red}>{digimonNameKor}</color> 기절" 
                : $"<color={ColorTable.Green}>{digimonNameKor}</color> 기절";

            BattleUIManager.Instance.ShowBattleMsg(true, msg);
            await UniTask.Delay(DELAY_MESSAGE_TIME);
            BattleUIManager.Instance.ShowBattleMsg(false);
            //await UniTask.Delay(500);

            // 경험치 획득 처리
            if (isEnemy)
            {
                Debug.Log($"죽은 적 디지몬 : {digimonNameKor} EXP : {_exp}");

                CaptureSystem.Instance.IncreaseCatchCount(digimonName);     //포획에 필요한 퇴치카운트 증가
                
                // 죽은게 적 디지몬이면 플레이어의 모든 디지몬 경험치 획득
                IReadOnlyList<DigimonStatus> list = GameManager.Instance.GetDigimonStatusList();
                var battleList = BattleManager.Instance.PlayerStatusList;

                for (int i = 0; i < battleList.Count; i++)
                {
                    // 기절한 디지몬은 경험치 못 먹음
                    if (battleList[i].CurrentHP <= 0)
                        continue;

                    for (int j = 0; j < list.Count; j++)
                    {
                        if (battleList[i].ID == list[j].ID || battleList[i].PrevID == list[j].ID)
                        {
                            list[j].IncreaseEXP(_exp);
                            break;
                        }
                    }
                }

                //for (int i = 0; i < list.Count; i++)
                //{
                //    // 기절한 디지몬은 경험치 못 먹음
                //    if (list[i].CurrentHP <= 0)
                //        continue;

                //    list[i].IncreaseEXP(_exp);
                //}
            }


            return;
        }

        if (isCritical)
        {
            string msg = isEnemy ? $"<color={ColorTable.Red}>{digimonNameKor}</color>이 치명타 {damage} 피해를 받았다."
                : $"<color={ColorTable.Green}>{digimonNameKor}</color>이 치명타 {damage} 피해를 받았다.";

            if (attacker.type == EType.Insight)
            {
                msg = isEnemy ? $"[통찰] <color={ColorTable.Red}>{digimonNameKor}</color>이 치명타 {damage} 피해를 받았다."
                : $"[통찰] <color={ColorTable.Green}>{digimonNameKor}</color>이 치명타 {damage} 피해를 받았다.";
            }

            _animator.SetTrigger("Down");

            BattleUIManager.Instance.ShowBattleMsg(true, msg);
            await UniTask.Delay(DELAY_MESSAGE_TIME);
            BattleUIManager.Instance.ShowBattleMsg(false);
            //await UniTask.Delay(500);
        }
        else
        {
            string msg = isEnemy ? $"<color={ColorTable.Red}>{digimonNameKor}</color>이 {damage} 피해를 받았다."
                : $"<color={ColorTable.Green}>{digimonNameKor}</color>이 {damage} 피해를 받았다.";

            _animator.SetTrigger("Hit");

            BattleUIManager.Instance.ShowBattleMsg(true, msg);
            await UniTask.Delay(DELAY_MESSAGE_TIME);
            BattleUIManager.Instance.ShowBattleMsg(false);
            //await UniTask.Delay(500);
        }

    }

    public void EndSkill()
    {
        //_isSkillFinished = true;
        Debug.Log("스킬 끝");
    }
}
