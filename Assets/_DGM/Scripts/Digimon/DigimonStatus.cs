using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

public class DigimonStatus : MonoBehaviour
{
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
    [SerializeField] private int evo;

    private int _currentHP;
    private int _actionCount;

    private Animator _animator;

    public void Init(DigimonStatus data)
    {
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
        this.evo = data.evo;

        _currentHP = data.hp;

        _actionCount = 10;
        Debug.Log($"Init 1 : _currentHP : {_currentHP}");
    }
    public void Init(StatusData data, GrowthType growthType)
    {
        digimonNameKor = data.KorName;
        digimonName = data.DigimonName;
        attr = (EAttribute)Enum.Parse(typeof(EAttribute), data.Attr);
        this.attrKor = data.KorAttr;
        grade = (EGrade)Enum.Parse(typeof(EGrade), data.Grade);
        this.gradeKor = data.KorGrade;
        type = (EType)Enum.Parse(typeof(EType), data.Type);
        this.typeKor = data.KorType;
        if (data is EnemyStatusData enemyData)
        {
            level = enemyData.Level;
        }

        else
        {
            level = 1;
        }

        hp = data.BaseHP;
        atk = data.BaseATK;
        def = data.BaseDEF;
        intel = data.BaseINT;
        speed = data.BaseSPD;

        this.growthType = growthType;
        this.evo = data.Evo;

        _currentHP = data.BaseHP;

        _actionCount = 10;

        Debug.Log($"Init 2 : _currentHP : {_currentHP}");
    }

    public string DigimonNameKor => digimonNameKor;
    public string DigimonName => digimonName;
    public EAttribute Attr => attr;
    public string AttrKor => attrKor;
    public EGrade Grade => grade;
    public string GradeKor => attrKor;
    public EType Type => type;
    public string TypeKor => typeKor;

    public int Level { get { return level; } set { level = value; } }
    public int HP { get { return hp + (level - 1) * growthType.HPInc; } set { hp = value; } }
    
    public int ATK { get { return atk + (level - 1) * growthType.ATKInc; } set { atk = value; } } 

    public int DEF { get { return def + (level - 1) * growthType.DEFInc; } set { def = value; } }

    public int INT { get { return intel + (level - 1) * growthType.INTInc; } set { intel = value; } }
    public int SPD { get { return speed + (level - 1) * growthType.SPDInc; } set { speed = value; } }
    public float CriticalRate => Mathf.Min(0.5f, 0.05f + (0.002f * INT));
    public float DodgeRate => Mathf.Min(0.3f, 0.02f + (0.001f * SPD));

    public GrowthType GrowthType => growthType;
    public int EvoID => evo;

    public int CurrentHP { get { return _currentHP; } set { _currentHP = value; } }
    public int ActionCount => _actionCount;

    private void Awake()
    {
        //_currentHP = HP;
        //_actionCount = 10;
        _animator = GetComponent<Animator>();
    }

    public async UniTask Victory()
    {
        _animator.SetTrigger("Victory");

        await UniTask.Delay(2000);
    }

    public async UniTask Attack(DigimonStatus target, bool isEnemy)
    {
        if (target.CurrentHP <= 0)
        {
            Debug.Log($"´ë»óÀÌ ÀÌ¹Ì Á×À½");
            await UniTask.Delay(2000);
            return;
        }

        _animator.SetTrigger("Attack");

        await UniTask.Delay(2000);

        BattleManager.Instance.ShowDigimon(target, isEnemy);

        (int damage, bool isCritical) = DamageCalculator.Calculate(this, target, false);
        await target.TakeDamage(damage, isCritical, isEnemy);
    }

    public async UniTask Skill(DigimonStatus target, bool isEnemy)
    {
        if (target.CurrentHP <= 0)
        {
            Debug.Log($"´ë»óÀÌ ÀÌ¹Ì Á×À½");
            await UniTask.Delay(2000);
            return;
        }

        _animator.SetTrigger("Skill");

        await UniTask.Delay(2000);

        BattleManager.Instance.ShowDigimon(target, isEnemy);
        (int damage, bool isCritical) = DamageCalculator.Calculate(this, target, true);
        await target.TakeDamage(damage, isCritical, isEnemy);
    }

    public async UniTask Guard()
    {
        _animator.SetTrigger("Guard");

        DEF += 10;
        Debug.Log($"DEF 10 »ó½Â");

        await UniTask.Delay(1000);
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
        while (elapsed < duration)
        {
            transform.position += transform.forward * speed * Time.deltaTime;
            elapsed += Time.deltaTime;

            await UniTask.Yield();
        }
    }

    public async UniTask TakeDamage(int damage, bool isCritical, bool isEnemy)
    {
        Debug.Log($"isCritical : {isCritical} damage : {damage}");

        CurrentHP -= damage;
        CurrentHP = Mathf.Clamp(CurrentHP, 0, HP);

        Debug.Log($"CurrentHP : {CurrentHP}");

        if (isEnemy)
            BattleUIManager.Instance.UpdateEnemyHP();
        else
            BattleUIManager.Instance.UpdatePlayer();

        if (CurrentHP <= 0)
        {
            _animator.SetTrigger("Die");
            await UniTask.Delay(2000);

            return;
        }

        if (isCritical)
        {
            _animator.SetTrigger("Down");
            await UniTask.Delay(3000);
        }
        else
        {
            _animator.SetTrigger("Hit");
            await UniTask.Delay(2000);
        }

    }
}
