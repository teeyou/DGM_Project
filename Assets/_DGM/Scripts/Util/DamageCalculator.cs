using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
상성 우위 1.25배
스킬 : 공격의 2배, 크리티컬 확률 10% 증가

최대최소 보정 : 0.95 ~ 1.05

치명타 : 1.5배

*/
public static class DamageCalculator
{
    public static (int,bool) Calculate(DigimonStatus actor, DigimonStatus target, bool isSkill)
    {
        float skillPower = isSkill ? 2f : 1f;
        float criticalRate = isSkill? actor.CriticalRate + 0.1f : actor.CriticalRate;   // 스킬 사용시 치명타 확률 10% 증가

        float damage = ((actor.ATK * 25) / (target.DEF + 25)) 
            * GetGradeMultiplier(actor.Grade, target.Grade) 
            * GetLevelMultiplier(actor.Level, target.Level) 
            * skillPower
            * GetAttrMultiplier(actor.Attr,target.Attr);

        float rand = Random.Range(0f, 1f);

        bool isCritical = false;

        if (criticalRate > rand)
        {
            damage *= 1.5f;
            isCritical = true;
        }

        return (Mathf.RoundToInt(Mathf.Max(1f, damage * RandomValue)), isCritical);
    }

    private static float GetGradeMultiplier(EGrade actor, EGrade target)
    {
        if (actor == target)
            return 1f;
        else if (actor < target)
            return 0.75f;
        else
            return 1.25f;
    }

    private static float GetLevelMultiplier(int actor, int target)
    {
        float value = 1 + (actor - target) * 0.05f;         //레벨 1당 5프로
        value = Mathf.Clamp(value, 0.5f, 2f);
        return value;
    }

    private static float GetAttrMultiplier(EAttribute actor, EAttribute target)
    {
        // 불명
        if (actor == EAttribute.Unknown)
        {
            if (target == EAttribute.Free) 
                return 1.0f;

            return 1.25f;
        }

        // 프리: 무상성
        if (actor == EAttribute.Free)
            return 1.0f;

        // 우위
        if (actor == EAttribute.Vaccine && target == EAttribute.Virus) return 1.25f;
        if (actor == EAttribute.Data && target == EAttribute.Vaccine) return 1.25f;
        if (actor == EAttribute.Virus && target == EAttribute.Data) return 1.25f;

        // 열세
        if (actor == EAttribute.Vaccine && target == EAttribute.Data) return 0.75f;
        if (actor == EAttribute.Data && target == EAttribute.Virus) return 0.75f;
        if (actor == EAttribute.Virus && target == EAttribute.Vaccine) return 0.75f;

        // 동등
        return 1.0f;
    }

    private static float RandomValue => Random.Range(0.95f, 1.05f);
}
