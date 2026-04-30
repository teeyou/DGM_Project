using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EDigimonType
{
    None,
    Vaccine,
    Data,
    Virus,
}

public enum EDigimonPersonality
{
    None,
    Fight,      // ÅõÁö
    Endurance,  // ÀÎ³»
    Insight,    // ÅëÂû
    Agility     // ¹ÎÃ¸
}

public enum EDigimonEvolutionState
{
    Baby,       // À¯³â±â
    Rookie,     // ¼ºÀå±â
    Champion,   // ¼º¼÷±â
    Perfect,    // ¿ÏÀüÃ¼
    Mega        // ±Ã±ØÃ¼
}

[CreateAssetMenu(menuName = "SO/DigimonStatus", fileName = "_Status")]
public class DigimonStatusSO : ScriptableObject
{
    [Header("±âº» Á¤º¸")]
    [SerializeField] private string digimonName;
    [SerializeField] private EDigimonType type;
    [SerializeField] private EDigimonEvolutionState evoState;
    [SerializeField] private EDigimonPersonality personality;

    [Header("½ºÅÈ")]
    [SerializeField] private int level;
    [SerializeField] private int hp;
    [SerializeField] private int atk;
    [SerializeField] private int def;
    [SerializeField] private int intel;
    [SerializeField] private int speed;
    //[SerializeField] private float criticalRate; // Ä¡¸íÅ¸ È®·ü (0~1)
    //[SerializeField] private float dodgeRate;     // È¸ÇÇÀ² (0~1)

    [Header("·¹º§¾÷¿¡ µû¸¥ ½ºÅÈ »ó½Â")]
    [SerializeField] private GrowthValueSO growthValue;

    [Header("ÁøÈ­¿¡ µû¸¥ ½ºÅÈ »ó½Â")]
    [SerializeField] private EvolutionValueSO evoValue;

    public string DigimonName => digimonName;
    public EDigimonType Type => type;
    public EDigimonEvolutionState EvoState => evoState;
    public EDigimonPersonality Personality => personality;


    public int Level => level;
    public int HP => hp + (level - 1) * growthValue.HpGrowth;
    public int ATK => atk + (level - 1) * growthValue.AtkGrowth;
    public int DEF => def + (level - 1) * growthValue.DefGrowth;
    public int INT => intel + (level - 1) * growthValue.IntGrowth;
    public int SPD => speed + (level - 1) * growthValue.SpdGrowth;
    public float CriticalRate => 0.1f + (0.001f * INT);
    public float DodgeRate => 0.1f + (0.001f * SPD);

    public GrowthValueSO GrowthValue => growthValue;
    public EvolutionValueSO EvoValue => evoValue;
}
