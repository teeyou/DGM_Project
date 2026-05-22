using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VeemonAttack : Attack
{
    private DigimonStatus _status;
    public override void PlayHit()
    {
        Debug.Log("VeemonAttack PlayHit »£√‚");
        string name = "CommonHit";
        Vector3 pos = transform.position;
        pos.x += 0.2f;
        pos.y = 0.7f;

        Quaternion rot = Quaternion.identity;

        EffectManager.Instance.PlayHit(name, pos, rot);
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
        
        //pos.y = 1f;

        Quaternion rot = Quaternion.identity;
        rot = Quaternion.Euler(0f, 0f, transform.eulerAngles.y);

        //EffectManager.Instance.PlaySkill(name, pos, rot);
    }
}
