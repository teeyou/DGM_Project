using System;
using UnityEngine;

public class DigimonStatus : MonoBehaviour
{
    [SerializeField] private string digimonNameKor;
    [SerializeField] private string digimonName;
    [SerializeField] private EAttribute attr;
    [SerializeField] private EGrade grade;
    [SerializeField] private EType type;

    [SerializeField] private int level;
    [SerializeField] private int hp;
    [SerializeField] private int atk;
    [SerializeField] private int def;
    [SerializeField] private int intel;
    [SerializeField] private int speed;

    //[SerializeField] private float criticalRate;
    //[SerializeField] private float dodgeRate;

    private StatusData statusData;
    private GrowthType growthType;
    private EvoTree evoTree;

    public void Init(DigimonStatus data)
    {
        this.growthType = data.growthType;
        this.evoTree = data.evoTree;

        this.digimonNameKor = data.digimonNameKor;
        this.digimonName = data.digimonName;
        this.attr = data.attr;
        this.grade = data.grade;
        this.type = data.type;
        this.level = data.level;

        this.hp = data.hp;
        this.atk = data.atk;
        this.def = data.def;
        this.intel = data.intel;
        this.speed = data.speed;


    }
    public void Init(StatusData data, GrowthType growthType, EvoTree evoTree)
    {
        this.statusData = data;
        this.growthType = growthType;
        this.evoTree = evoTree;

        digimonNameKor = data.KorName;
        digimonName = data.DigimonName;
        attr = (EAttribute)Enum.Parse(typeof(EAttribute), data.Attr);
        grade = (EGrade)Enum.Parse(typeof(EGrade), data.Grade);
        type = (EType)Enum.Parse(typeof(EType), data.Type);

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

        //criticalRate = 0.1f + (0.001f * intel);
        //dodgeRate = 0.1f + (0.001f * speed);

        Debug.Log("DigimonStatus 초기화 완료");
    }

    public string DigimonNameKor => digimonNameKor;
    public string DigimonName => digimonName;
    public EAttribute Attr => attr;
    public EGrade Grade => grade;
    public EType Type => type;

    public int Level { get { return level; } set { level = value; } }
    public int HP { get { return hp + (level - 1) * growthType.HPInc; } set { hp = value; } }
    
    public int ATK { get { return atk + (level - 1) * growthType.ATKInc; } set { atk = value; } } 

    public int DEF { get { return def + (level - 1) * growthType.DEFInc; } set { def = value; } }

    public int INT { get { return intel + (level - 1) * growthType.INTInc; } set { intel = value; } }
    public int SPD { get { return speed + (level - 1) * growthType.SPDInc; } set { speed = value; } }
    public float CriticalRate => 0.1f + (0.001f * INT);
    public float DodgeRate => 0.1f + (0.001f * SPD);
}
