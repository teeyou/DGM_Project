using UnityEngine;

public class DigimonStatus : MonoBehaviour
{
    [SerializeField] private string digimonName;
    [SerializeField] private EDigimonType type;
    [SerializeField] private EDigimonEvolutionState evoState;
    [SerializeField] private EDigimonPersonality personality;

    [SerializeField] private int level;
    [SerializeField] private int hp;
    [SerializeField] private int atk;
    [SerializeField] private int def;
    [SerializeField] private int intel;
    [SerializeField] private int speed;

    [SerializeField] private float criticalRate;
    [SerializeField] private float dodgeRate;

    private DigimonStatusSO statusData; 
    public void Init(DigimonStatusSO data)
    {
        statusData = data;

        digimonName = data.DigimonName;
        type = data.Type;
        evoState = data.EvoState;
        personality = data.Personality;

        level = data.Level;
        hp = data.HP;
        atk = data.ATK;
        def = data.DEF;
        intel = data.INT;
        speed = data.SPD;

        criticalRate = 0.1f + (0.001f * intel);
        dodgeRate = 0.1f + (0.001f * speed);

        Debug.Log("DigimonStatus 초기화 완료");
    }

    public string DigimonName => digimonName;
    public EDigimonType Type => type;
    public EDigimonEvolutionState EvoState => evoState;
    public EDigimonPersonality Personality => personality;


    public int Level { get { return level; } set { level = value; } }
    public int HP { get { return hp + (level - 1) * statusData.GrowthValue.HpGrowth; } set { hp = value; } }
    
    public int ATK { get { return atk + (level - 1) * statusData.GrowthValue.AtkGrowth; } set { atk = value; } } 

    public int DEF { get { return def + (level - 1) * statusData.GrowthValue.DefGrowth; } set { def = value; } }

    public int INT { get { return intel + (level - 1) * statusData.GrowthValue.IntGrowth; } set { intel = value; } }
    public int SPD { get { return speed + (level - 1) * statusData.GrowthValue.SpdGrowth; } set { speed = value; } }
    public float CriticalRate => 0.1f + (0.001f * INT);
    public float DodgeRate => 0.1f + (0.001f * SPD);
}
