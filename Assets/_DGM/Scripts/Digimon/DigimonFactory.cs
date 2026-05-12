using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DigimonFactory : Singleton<DigimonFactory>
{
    protected override void Awake()
    {
        base.Awake();

        DontDestroyOnLoad(gameObject);
    }

    public GameObject CreateDigimon(GameObject prefab, Vector3 pos, Quaternion rot, Transform parent)
    {
        if (prefab == null)
            return null;

        return Instantiate(prefab, pos, rot, parent);
    }


    public GameObject CreateDigimon(GameObject prefab, Vector3 pos, Quaternion rot)
    {
        if (prefab == null)
        {
            return null;

        }
        return Instantiate(prefab, pos, rot);
    }

    public GameObject CreateDigimon(GameObject prefab, Vector3 pos)
    {
        if (prefab == null)
            return null;

        return Instantiate(prefab, pos, Quaternion.identity);
    }
}
