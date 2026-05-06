using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public enum EAttribute
{
    None,
    Vaccine,
    Data,
    Virus,
    Free,
}

public enum EType
{
    None,
    Fight,      // ≈ı¡ˆ
    Endurance,  // ¿Œ≥ª
    Insight,    // ≈Î¬˚
    Agility     // πŒ√∏
}

public enum EGrade
{
    Baby,       // ¿Ø≥‚±‚
    Rookie,     // º∫¿Â±‚
    Champion,   // º∫º˜±‚
    Perfect,    // øœ¿¸√º
    Mega        // ±√±ÿ√º
}

public class DigimonDB : Singleton<DigimonDB>
{
    private Dictionary<int, StatusData> _idToStatusData = new Dictionary<int, StatusData>();
    private Dictionary<string, StatusData> _nameToStatusData = new Dictionary<string, StatusData>();

    private Dictionary<string, GrowthType> _growthTypeToGrowthType = new Dictionary<string, GrowthType>();
    private Dictionary<int, EvoTree> _idToEvoTree = new Dictionary<int, EvoTree>();

    private ExcelReader _excelReader = new ExcelReader();

    protected override void Awake()
    {
        base.Awake();

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        _excelReader.LoadExcelData();

        // StatusData
        for (int i = 0; i < _excelReader.StatusList.Count; i++)
        {
            _idToStatusData[_excelReader.StatusList[i].ID] = _excelReader.StatusList[i];
            _nameToStatusData[_excelReader.StatusList[i].DigimonName] = _excelReader.StatusList[i];
        }

        // GrowthType
        for (int i = 0; i < _excelReader.GrowthTypeList.Count; i++)
        {
            _growthTypeToGrowthType[_excelReader.GrowthTypeList[i].Type] = _excelReader.GrowthTypeList[i];
        }

        // EvoTree
        for (int i = 0; i < _excelReader.EvoTreeList.Count; i++)
        {
            _idToEvoTree[_excelReader.EvoTreeList[i].ID] = _excelReader.EvoTreeList[i];
        }

        //Debug.Log($"_idToStatusData : {_idToStatusData.Count}");
        //Debug.Log($"_growthTypeToGrowthType : {_growthTypeToGrowthType.Count}");
        //Debug.Log($"_idToEvoTree : {_idToEvoTree.Count}");
    }

    public StatusData GetStatusDataByName(string name)
    {
        if (_nameToStatusData.TryGetValue(name, out StatusData data))
            return data;

        return null;
    }
    public StatusData GetStatusDataById(int id)
    {
        if (_idToStatusData.TryGetValue(id, out StatusData data))
            return data;

        return null;
    }

    public GrowthType GetGrowthType(string growthType)
    {
        if (_growthTypeToGrowthType.TryGetValue(growthType, out GrowthType growth))
            return growth;

        return null;
    }

    public EvoTree GetEvoTreeById(int id)
    {
        if (_idToEvoTree.TryGetValue(id, out EvoTree evo))
            return evo;
        return null;
    }

}
