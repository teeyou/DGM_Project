using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemiDevimonEffect : Effect
{
    private DigimonStatus _status;
    public override void PlayHit()
    {
        string name = "CommonHit";
        Vector3 pos = transform.position;
        pos.x += 0.2f;
        pos.y = 0.7f;

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
        pos += transform.forward * -1f;
        pos.y = 0.5f;
        pos.z += -0.5f;

        Quaternion rot = Quaternion.identity;
        rot = Quaternion.Euler(0f, 0f, -transform.eulerAngles.y);

        EffectManager.Instance.InstantiateSkill(name, pos, rot);

        pos.z += 1f;
        EffectManager.Instance.InstantiateSkill(name, pos, rot);
    }
}
