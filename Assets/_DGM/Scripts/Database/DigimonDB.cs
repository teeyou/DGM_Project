using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;

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

    private Dictionary<int, EnemyStatusData> _idToEnemyStatusData = new Dictionary<int, EnemyStatusData>();
    private Dictionary<string, EnemyStatusData> _nameToEnemyStatusData = new Dictionary<string, EnemyStatusData>();

    private Dictionary<string, GrowthType> _growthTypeToGrowthType = new Dictionary<string, GrowthType>();
    private Dictionary<int, EvoTree> _idToEvoTree = new Dictionary<int, EvoTree>();

    private ExcelReader _excelReader = new ExcelReader();

    private Dictionary<string, Sprite> _nameToSprite = new Dictionary<string, Sprite>();

    protected override void Awake()
    {
        base.Awake();

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        DontDestroyOnLoad(gameObject);
    }

    
    private void Start()
    {
        CacheExcelData();
        CacheDigimonSprites();
    }

    private void CacheDigimonSprites()
    {
        Addressables.LoadAssetsAsync<Sprite>("DigimonSprites", sprite =>
        {
            _nameToSprite[sprite.name] = sprite;
            Debug.Log($"{sprite.name} Ω∫«¡∂Û¿Ã∆Æ ƒ≥ΩÃ");
        });
    }

    private void CacheExcelData()
    {
        _excelReader.LoadExcelData();

        // StatusData
        for (int i = 0; i < _excelReader.StatusList.Count; i++)
        {
            _idToStatusData[_excelReader.StatusList[i].ID] = _excelReader.StatusList[i];
            _nameToStatusData[_excelReader.StatusList[i].DigimonName] = _excelReader.StatusList[i];
        }

        // EnemyStatusData
        for (int i = 0; i < _excelReader.EnemyStatusList.Count; i++)
        {
            _idToEnemyStatusData[_excelReader.EnemyStatusList[i].ID] = _excelReader.EnemyStatusList[i];
            _nameToEnemyStatusData[_excelReader.EnemyStatusList[i].DigimonName] = _excelReader.EnemyStatusList[i];
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

    public EnemyStatusData GetEnemyStatusDataByName(string name)
    {
        if (_nameToEnemyStatusData.TryGetValue(name, out EnemyStatusData data))
            return data;

        return null;
    }

    public EnemyStatusData GetEnemyStatusDataById(int id)
    {
        if (_idToEnemyStatusData.TryGetValue(id, out EnemyStatusData data))
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

    public Sprite GetDigimonSprite(string digimonName)
    {
        if (_nameToSprite.TryGetValue(digimonName, out Sprite sprite))
            return sprite;
        else
            return null; 
    }

    public bool HasDigimonSprites()
    {
        return _nameToSprite.Count > 0;
    }

    public Dictionary<string, Sprite> GetDigimonSprites()
    {
        return _nameToSprite;
    }
}
