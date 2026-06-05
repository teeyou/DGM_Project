using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaptureSystem : Singleton<CaptureSystem>
{
    private const int REQUIRED_COUNT = 1;

    private Dictionary<string, int> _nameToCatchCount = new Dictionary<string, int>();

    protected override void Awake()
    {
        base.Awake();

        DontDestroyOnLoad(gameObject);
    }

    public void IncreaseCatchCount(string digimonName)
    {
        if (_nameToCatchCount.TryGetValue(digimonName, out int count))
        {
            _nameToCatchCount[digimonName] += 1;
        }
        else
        {
            _nameToCatchCount[digimonName] = 1;
        }
    }

    public void DecreaseCatchCount(string digimonName)
    {
        if (_nameToCatchCount.TryGetValue(digimonName, out int count))
        {
            _nameToCatchCount[digimonName] -= 1;
        }
        else
        {
            _nameToCatchCount[digimonName] = 0;
        }
    }

    public bool IsCapturePossible(string digimonName)
    {
        // 이미 가지고 있으면 못 잡음
        if (DigimonBook.Instance.CheckIsCaptured(digimonName))
        {
            FieldUIController.Instance.ShowMessage($"이미 보유한 디지몬입니다.");
            Debug.Log("이미 가지고 있음");
            return false;
        }

        // POSSIBLE_COUNT를 채우면 포획 가능

        if (_nameToCatchCount.TryGetValue(digimonName, out int count))
        {
            if (count >= REQUIRED_COUNT)
            {
                DecreaseCatchCount(digimonName);
                return true;
            }
            else
            {
                FieldUIController.Instance.ShowMessage($"퇴치 카운트 부족 : {GetCatchCount(digimonName)} / {RequiredCount}");
                return false;
            }
        }

        Debug.Log("잡은 횟수 : 0");
        FieldUIController.Instance.ShowMessage($"퇴치 카운트 부족 : {GetCatchCount(digimonName)} / {RequiredCount}");
        return false;
    }

    public int RequiredCount => REQUIRED_COUNT;

    public int GetCatchCount(string digimonName)
    {
        if (_nameToCatchCount.TryGetValue(digimonName, out int count))
        {
            return count;
        }

        return 0;
    }
}
