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

    [SerializeField] private int _exp;
    private int _requiredExp;

    private int _uid;
    private int _currentHP;
    private int _actionCount;

    private Animator _animator;

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
        if (data is EnemyStatusData enemyData)
        {
            level = enemyData.Level;
            _exp = enemyData.EXP;
        }

        else
        {
            level = 1;
            _exp = 0;
            _requiredExp = LevelSystem.Instance.GetRequiredEXP(1);  // 레벨 1 필요 경험치

        }

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

    public async UniTask Victory()
    {
        _animator.SetTrigger("Victory");

        await UniTask.Delay(2000);
    }

    public void IncreaseEXP(int exp)
    {
        Debug.Log($"경험치 획득 : {exp}");
        EXP += exp;
        Debug.Log($"현재 경험치 : {EXP}");
        CheckLevelUp();
    }

    public void CheckLevelUp()
    {
        while (EXP >= RequiredEXP)
        {
            LevelUp();

            EXP -= RequiredEXP;
            EXP = EXP <= 0 ? 0 : EXP;

            RequiredEXP = LevelSystem.Instance.GetRequiredEXP(Level);
            Debug.Log($"Level : {Level} RequiredEXP : {RequiredEXP}");
        }
    }

    private void LevelUp()
    {
        Debug.Log("LevelUp");
        Level++;
        HP += growthType.HPInc;
        CurrentHP = HP;
        ATK += growthType.ATKInc;
        DEF += growthType.DEFInc;
        INT += growthType.INTInc;
        SPD += growthType.SPDInc;

        BattleManager.Instance.UpdatePlayerStatusList();
    }

    public async UniTask Attack(DigimonStatus target, bool isEnemy)
    {
        if (target.CurrentHP <= 0)
        {
            string s = "이미 기절했다.";
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
        await target.TakeDamage(damage, isCritical, isEnemy);
    }

    public async UniTask Skill(DigimonStatus target, bool isEnemy)
    {
        if (target.CurrentHP <= 0)
        {
            string s = "이미 기절했다.";
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

        _animator.SetTrigger("Skill");

        BattleUIManager.Instance.ShowBattleMsg(true, msg);

        await UniTask.Delay(DELAY_MESSAGE_TIME);

        BattleUIManager.Instance.ShowBattleMsg(false);

        //await UniTask.Delay(500);

        BattleManager.Instance.ShowDigimon(target, isEnemy);
        (int damage, bool isCritical) = DamageCalculator.Calculate(this, target, true);
        await target.TakeDamage(damage, isCritical, isEnemy);
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
        await UniTask.Delay(1000);
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

    public async UniTask TakeDamage(int damage, bool isCritical, bool isEnemy)
    {
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
        }

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
                // 죽은게 적 디지몬이면 플레이어의 모든 디지몬 경험치 획득
                IReadOnlyList<DigimonStatus> list = GameManager.Instance.GetDigimonStatusList();
                
                for (int i = 0; i < list.Count; i++)
                {
                    // 기절한 디지몬은 경험치 못 먹음
                    if (list[i].CurrentHP <= 0)
                        continue;

                    list[i].IncreaseEXP(_exp);
                }
            }


            return;
        }

        if (isCritical)
        {
            string msg = isEnemy ? $"<color={ColorTable.Red}>{digimonNameKor}</color>이 치명타 {damage} 피해를 받았다."
                : $"<color={ColorTable.Green}>{digimonNameKor}</color>이 치명타 {damage} 피해를 받았다.";

            if (type == EType.Insight)
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
}
