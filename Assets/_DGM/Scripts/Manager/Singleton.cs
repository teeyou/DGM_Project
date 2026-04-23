using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton <T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<T>();

                if (_instance == null)
                {
                    Debug.Log("Singleton - 하이어라키에 오브젝트 생성 필요");
                    //GameObject go = new GameObject(typeof(T).ToString());
                    //_instance = go.AddComponent<T>();
                }
            }

            return _instance;
        }
    }

    protected virtual void Awake()
    {
        if (_instance != null && _instance != this as T)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this as T;
    }
    protected virtual void OnDestroy()
    {
        if (_instance == this as T)
        {
            _instance = null;
        }
    }
}
