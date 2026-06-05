using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaremonEffect : Effect
{
    private DigimonStatus _status;
    public override void PlayHit()
    {
        string name = "CommonHit";
        Vector3 pos = transform.position;
        pos.x += 1.5f;
        pos.y = 1.5f;

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
        pos += transform.forward * 2f;
        pos.y = 1f;

        Quaternion rot = Quaternion.identity;
        rot = Quaternion.Euler(0f, transform.eulerAngles.y - 90f, 0f);

        EffectManager.Instance.PlaySkill(name, pos, rot);
        AudioManager.Instance.PlaySFX("BeamSFX");
    }
}
