using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DontDestory : MonoBehaviour
{
    [SerializeField] private string _id;

    private static HashSet<string> _idHashSet = new HashSet<string>();  // 오직 1개만 존재하도록 static으로 선언

    void Awake()
    {
        if (_idHashSet.Contains(_id))
        {
            Destroy(gameObject); // 같은 ID가 이미 있으면 파괴
            return;
        }

        _idHashSet.Add(_id);
        DontDestroyOnLoad(gameObject);
    }
}
