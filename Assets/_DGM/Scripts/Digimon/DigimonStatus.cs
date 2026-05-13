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

    private StatusData statusData;
    [SerializeField] private GrowthType growthType;
    [SerializeField] private EvoTree evoTree;

    private int _currentHP;
    private int _actionCount;

    private Animator _animator;

    public void Init(DigimonStatus data)
    {
        this.growthType = data.growthType;
        this.evoTree = data.evoTree;

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

        _currentHP = data.hp;

        _actionCount = 10;

    }
    public void Init(StatusData data, GrowthType growthType, EvoTree evoTree)
    {
        this.statusData = data;
        this.growthType = growthType;
        this.evoTree = evoTree;

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

        _currentHP = data.BaseHP;

        _actionCount = 10;
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
    public float CriticalRate => 0.1f + (0.001f * INT);
    public float DodgeRate => 0.1f + (0.001f * SPD);

    public int CurrentHP { get { return _currentHP; } set { _currentHP = value; } }
    public int ActionCount => _actionCount;

    private void Awake()
    {
        //_currentHP = HP;
        _animator = GetComponent<Animator>();
    }
    public void Attack()
    {

    }

    public void Skill()
    {

    }

    public void Guard()
    {

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

    public void TakeDamage()
    {

    }
}
