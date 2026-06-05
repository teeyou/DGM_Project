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
    private Dictionary<string, GameObject> _nameToObjectSkill = new Dictionary<string, GameObject>();
    private ParticleSystem _evoVFX;

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

        LoadSkillObject().Forget();

        LoadEvoVFX().Forget();
    }

    private async UniTaskVoid LoadEvoVFX()
    {
        var handle = Addressables.LoadAssetAsync<GameObject>("VFX_Evo");
        GameObject prefab = await handle.Task;
        if (prefab != null)
        {
            _evoVFX = prefab.GetComponent<ParticleSystem>();
            //Debug.Log($"Evolution : {prefab.name}");
        }
    }

    private async UniTaskVoid LoadHitVFX()
    {
        var handle = Addressables.LoadAssetsAsync<GameObject>("VFX_Hit", prefab =>
        {
            ParticleSystem ps = prefab.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                _nameToHit[prefab.name] = ps;
                //Debug.Log($"Hit : {prefab.name}");
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
                //Debug.Log($"Hit : {prefab.name}");
            }
        });

        await handle.Task;
    }

    private async UniTaskVoid LoadSkillObject()
    {
        var handle = Addressables.LoadAssetsAsync<GameObject>("Object_Skill", prefab =>
        {
            if (prefab != null)
            {
                _nameToObjectSkill[prefab.name] = prefab;
                //Debug.Log($"Skill Object : {prefab.name}");
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

    public GameObject PlaySkill(string name, Vector3 pos, Quaternion rot)
    {
        if (_nameToSkill.TryGetValue(name, out var prefab))
        {
            GameObject go = Instantiate(prefab.gameObject, pos, rot);
            ParticleSystem effect = go.GetComponent<ParticleSystem>();

            //ParticleSystem effect = Instantiate(prefab, pos, rot);
            //effect.Play();
            
            Destroy(effect.gameObject, effect.main.duration);

            return go;
        }

        return null;
    }

    public void InstantiateSkill(string name, Vector3 pos, Quaternion rot)
    {
        if (_nameToObjectSkill.TryGetValue(name, out var prefab))
        {
            GameObject go = Instantiate(prefab, pos, rot);
            Destroy(go, 2f);
        }
    }

    public void PlayEvo(Vector3 pos)
    {
        if (_evoVFX != null)
        {
            ParticleSystem effect = Instantiate(_evoVFX, pos, Quaternion.identity);
            Destroy(effect.gameObject, effect.main.duration);
        }
    }
}
