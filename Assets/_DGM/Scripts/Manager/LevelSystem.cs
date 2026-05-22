using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSystem : Singleton<LevelSystem>
{
    private Dictionary<int, int> _levelToNeedEXP = new Dictionary<int, int>();

    protected override void Awake()
    {
        base.Awake();

        DontDestroyOnLoad(gameObject);
    }

    public void CacheLevelExpData(int level, int requiredEXP)
    {
        _levelToNeedEXP[level] = requiredEXP;
    }

    public int GetRequiredEXP(int level)
    {
        return _levelToNeedEXP[level];
    }
}
