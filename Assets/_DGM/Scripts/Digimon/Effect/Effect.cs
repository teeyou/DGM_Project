using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Effect : MonoBehaviour
{
    public abstract void PlayHit();
    public abstract void PlayAttack();
    public abstract void PlaySkill();
}
