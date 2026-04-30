using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/GrowthValue", fileName = "GrowthValue")]
public class GrowthValueSO : ScriptableObject
{
    [SerializeField] private int hpGrowth;
    [SerializeField] private int atkGrowth;
    [SerializeField] private int defGrowth;
    [SerializeField] private int intelGrowth;
    [SerializeField] private int speedGrowth;

    public int HpGrowth => hpGrowth;
    public int AtkGrowth => atkGrowth;
    public int DefGrowth => defGrowth;
    public int IntGrowth => intelGrowth;
    public int SpdGrowth => speedGrowth;
}
