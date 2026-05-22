using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;


public class EffectManager : Singleton<EffectManager>
{
    private Dictionary<string, ParticleSystem> _nameToHit = new Dictionary<string, ParticleSystem>();
    private Dictionary<string, ParticleSystem> _nameToAttack = new Dictionary<string, ParticleSystem>();
    private Dictionary<string, ParticleSystem> _nameToSkill = new Dictionary<string, ParticleSystem>();

    protected override void Awake()
    {
        base.Awake();

        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        LoadHitVFX().Forget();
        //LoadAttackVFX().Forget();
        LoadSkillVFX().Forget();
    }

    private async UniTaskVoid LoadHitVFX()
    {
        var handle = Addressables.LoadAssetsAsync<GameObject>("VFX_Hit", prefab =>
        {
            ParticleSystem ps = prefab.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                _nameToHit[prefab.name] = ps;
                Debug.Log($"Hit : {prefab.name}");
            }
        });

        await handle.Task;
    }

    private async UniTaskVoid LoadAttackVFX()
    {
        var handle = Addressables.LoadAssetsAsync<GameObject>("VFX_Attack", prefab =>
        {
            ParticleSystem ps = prefab.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                _nameToAttack[prefab.name] = ps;
                Debug.Log($"Hit : {prefab.name}");
            }
        });

        await handle.Task;
    }

    private async UniTaskVoid LoadSkillVFX()
    {
        var handle = Addressables.LoadAssetsAsync<GameObject>("VFX_Skill", prefab =>
        {
            ParticleSystem ps = prefab.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                _nameToSkill[prefab.name] = ps;
                Debug.Log($"Hit : {prefab.name}");
            }
        });

        await handle.Task;
    }

    public void PlayHit(string name, Vector3 pos, Quaternion rot)
    {
        if (_nameToHit.TryGetValue(name, out var prefab))
        {
            Debug.Log("EffectManager PlayHit »£√‚");
            ParticleSystem effect = Instantiate(prefab, pos, rot);
            effect.Play();

            Destroy(effect.gameObject, effect.main.duration);
        }
    }

    public void PlayAttack(string name, Vector3 pos, Quaternion rot)
    {
        if (_nameToAttack.TryGetValue(name, out var prefab))
        {
            ParticleSystem effect = Instantiate(prefab, pos, rot);
            effect.Play();

            Destroy(effect.gameObject, effect.main.duration);
        }
    }

    public void PlaySkill(string name, Vector3 pos, Quaternion rot)
    {
        if (_nameToSkill.TryGetValue(name, out var prefab))
        {
            ParticleSystem effect = Instantiate(prefab, pos, rot);
            effect.Play();

            Destroy(effect.gameObject, effect.main.duration);
        }
    }
}
