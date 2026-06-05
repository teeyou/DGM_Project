using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AngemonEffect : Effect
{
    private DigimonStatus _status;
    public override void PlayHit()
    {
        string name = "CommonHit";
        Vector3 pos = transform.position;
        pos.x += 0.3f;
        pos.y = 1.2f;

        Quaternion rot = Quaternion.identity;

        EffectManager.Instance.PlayHit(name, pos, rot);
        AudioManager.Instance.PlaySFX("HitSFX");
    }

    public override void PlayAttack()
    {

    }

    public override void PlaySkill()
    {
        if (_status == null)
            _status = GetComponent<DigimonStatus>();

        string name = _status.DigimonName + "Skill";
        Vector3 pos = transform.position;
        pos += transform.forward * 1f;
        pos.y = 1.5f;

        Quaternion rot = Quaternion.identity;
        rot = Quaternion.Euler(0f, transform.eulerAngles.y - 90f, 90f);

        EffectManager.Instance.PlaySkill(name, pos, rot);
        AudioManager.Instance.PlaySFX("AngemonSFX");
    }
}
