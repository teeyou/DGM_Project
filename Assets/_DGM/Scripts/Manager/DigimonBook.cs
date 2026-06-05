using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DigimonBookData
{
    public int ID;
    public string DigimonName;
    public string DigimonNameKor;
    public bool IsCaptured;

    public DigimonBookData(int ID, string DigimonName, string DigimonNameKor, bool IsCaptured)
    {
        this.ID = ID;
        this.DigimonName = DigimonName;
        this.DigimonNameKor = DigimonNameKor;
        this.IsCaptured = IsCaptured;
    }
}
public class DigimonBook : Singleton<DigimonBook>
{
    private Dictionary<int, DigimonBookData> _idToBookData = new Dictionary<int, DigimonBookData>();
    private Dictionary<string, DigimonBookData> _nameToBookData = new Dictionary<string, DigimonBookData>();

    protected override void Awake()
    {
        base.Awake();

        DontDestroyOnLoad(gameObject);
    }

    public void SetBookData(DigimonStatus status)
    {
        _idToBookData[status.ID] = new DigimonBookData(status.ID, status.DigimonName, status.DigimonNameKor, true);
        _nameToBookData[status.DigimonName] = _idToBookData[status.ID];

        if (status.PrevID != -1)
        {
            StatusData data = DigimonDB.Instance.GetStatusDataById(status.PrevID);
            SetBookData(data.ID, data.DigimonName, data.KorName, true);
        }
    }

    public void SetBookData(int id, string name, string nameKor, bool isCaptured)
    {
        _idToBookData[id] = new DigimonBookData(id, name, nameKor, true);
        _nameToBookData[name] = _idToBookData[id];
    }

    public bool CheckIsCaptured(string digimonName)
    {
        if (_nameToBookData.TryGetValue(digimonName, out var status))
        {
            Debug.Log("이미 잡음");
            return true;
        }

        Debug.Log("안 잡음");
        return false;

    }
}
