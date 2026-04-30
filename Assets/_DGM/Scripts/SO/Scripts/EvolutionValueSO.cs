using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/EvolutionValue", fileName = "EvolutionValue")]
public class EvolutionValueSO : ScriptableObject
{
    [SerializeField] private int hpBonus;
    [SerializeField] private int atkBonus;
    [SerializeField] private int defBonus;
    [SerializeField] private int intelBonus;
    [SerializeField] private int speedBonus;

    public int GetHpBonus(int step)
    {
        return hpBonus * step;
    }

    public int GetAttackBonus(int step)
    {
        return atkBonus * step;
    }

    public int GetDefBonus(int step)
    {
        return defBonus * step;
    }
    public int GetIntBonus(int step)
    {
        return intelBonus * step;
    }
    public int GetSpeedBonus(int step)
    {
        return speedBonus * step;
    }

}
