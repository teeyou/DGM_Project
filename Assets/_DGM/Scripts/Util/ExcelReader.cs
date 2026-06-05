using ExcelDataReader;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;

public class LevelExpData
{
    public int Level;
    public int RequiredEXP;
}

[Serializable]
public class GrowthType
{
    public string Type;
    public int HPInc;
    public int ATKInc;
    public int DEFInc;
    public int INTInc;
    public int SPDInc;
}

public class StatusData
{
    public int ID;
    public string DigimonName;
    public string KorName;
    public string Grade;
    public string KorGrade;
    public string Attr;
    public string KorAttr;
    public string Type;
    public string KorType;
    public int BaseHP;
    public int BaseATK;
    public int BaseDEF;
    public int BaseINT;
    public int BaseSPD;
    public string GrowthType;
    public string KorGrowthType;
    public int Evo;
    public int Prev;
}

public class EnemyStatusData : StatusData
{
    public int Level;
    public int EXP;
}

public class ExcelReader
{
    private DataSet result;

    public List<StatusData> StatusList { get; private set; } = new List<StatusData>();
    public List<EnemyStatusData> EnemyStatusList { get; private set; } = new List<EnemyStatusData>();
    public List<GrowthType> GrowthTypeList { get; private set; } = new List<GrowthType>();
    public List<LevelExpData> LevelExpDataList { get; private set; } = new List<LevelExpData>();

    private string FilePath => Path.Combine(Application.streamingAssetsPath, "digimon.xlsx");

    public void LoadExcelData()
    {
        string encryptedPath = Path.Combine(Application.streamingAssetsPath, "digimon.enc");

        using (MemoryStream decryptedStream = CryptoUtils.DecryptFileToMemory(encryptedPath, AESConfig.Key, AESConfig.IV))
        {
            using (var reader = ExcelReaderFactory.CreateReader(decryptedStream))
            {
                result = reader.AsDataSet(new ExcelDataSetConfiguration()
                {
                    ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
                    {
                        UseHeaderRow = true
                    }
                });
            }
        }

        //using (var stream = File.Open(FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        //using (var reader = ExcelReaderFactory.CreateReader(stream))
        //{
        //    result = reader.AsDataSet(new ExcelDataSetConfiguration()
        //    {
        //        ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
        //        {
        //            UseHeaderRow = true
        //        }
        //    });
        //}

        LoadSheet("PlayerStatus");

        LoadSheet("EnemyStatus");

        LoadSheet("GrowthType");

        LoadSheet("LevelExp");
    }

    private void LoadSheet(string sheetName)
    {
        if (!result.Tables.Contains(sheetName))
        {
            Debug.Log($"시트 없음 : {sheetName}");
            return;
        }

        DataTable table = result.Tables[sheetName];

        if (sheetName == "PlayerStatus")
            ParseStatusSheet(table);

        else if (sheetName == "EnemyStatus")
            ParseEnemyStatusSheet(table);

        else if (sheetName == "GrowthType")
            ParseGrowthTypeSheet(table);

        else if (sheetName == "LevelExp")
            ParseLevelExpSheet(table);
    }

    private void ParseEnemyStatusSheet(DataTable table)
    {
        foreach (DataRow row in table.Rows)
        {
            EnemyStatusData data = new EnemyStatusData()
            {
                ID = int.Parse(row["ID"].ToString()),
                DigimonName = row["DigimonName"].ToString(),
                KorName = row["KorName"].ToString(),
                Grade = row["Grade"].ToString(),
                KorGrade = row["KorGrade"].ToString(),
                Attr = row["Attr"].ToString(),
                KorAttr = row["KorAttr"].ToString(),
                Type = row["Type"].ToString(),
                KorType = row["KorType"].ToString(),
                BaseHP = int.Parse(row["BaseHP"].ToString()),
                BaseATK = int.Parse(row["BaseATK"].ToString()),
                BaseDEF = int.Parse(row["BaseDEF"].ToString()),
                BaseINT = int.Parse(row["BaseINT"].ToString()),
                BaseSPD = int.Parse(row["BaseSPD"].ToString()),
                GrowthType = row["GrowthType"].ToString(),
                KorGrowthType = row["KorGrowthType"].ToString(),
                Evo = int.Parse(row["Evo"].ToString()),
                Level = int.Parse(row["Level"].ToString()),
                EXP = int.Parse(row["EXP"].ToString())
            };
            EnemyStatusList.Add(data);
        }
    }

    private void ParseStatusSheet(DataTable table)
    {
        foreach (DataRow row in table.Rows)
        {
            StatusData data = new StatusData()
            {
                ID = int.Parse(row["ID"].ToString()),
                DigimonName = row["DigimonName"].ToString(),
                KorName = row["KorName"].ToString(),
                Grade = row["Grade"].ToString(),
                KorGrade = row["KorGrade"].ToString(),
                Attr = row["Attr"].ToString(),
                KorAttr = row["KorAttr"].ToString(),
                Type = row["Type"].ToString(),
                KorType = row["KorType"].ToString(),
                BaseHP = int.Parse(row["BaseHP"].ToString()),
                BaseATK = int.Parse(row["BaseATK"].ToString()),
                BaseDEF = int.Parse(row["BaseDEF"].ToString()),
                BaseINT = int.Parse(row["BaseINT"].ToString()),
                BaseSPD = int.Parse(row["BaseSPD"].ToString()),
                GrowthType = row["GrowthType"].ToString(),
                KorGrowthType = row["KorGrowthType"].ToString(),
                Evo = int.Parse(row["Evo"].ToString()),
                Prev = int.Parse(row["Prev"].ToString()),
            };
            StatusList.Add(data);
        }
    }

    private void ParseGrowthTypeSheet(DataTable table)
    {
        foreach (DataRow row in table.Rows)
        {
            GrowthType growthType = new GrowthType()
            {
                Type = row["GrowthType"].ToString(),
                HPInc = int.Parse(row["HPInc"].ToString()),
                ATKInc = int.Parse(row["ATKInc"].ToString()),
                DEFInc = int.Parse(row["DEFInc"].ToString()),
                INTInc = int.Parse(row["INTInc"].ToString()),
                SPDInc = int.Parse(row["SPDInc"].ToString())
            };
            GrowthTypeList.Add(growthType);
        }
    }

    private void ParseLevelExpSheet(DataTable table)
    {
        foreach (DataRow row in table.Rows)
        {
            LevelExpData data = new LevelExpData()
            {
                Level = int.Parse(row["Level"].ToString()),
                RequiredEXP = int.Parse(row["EXP"].ToString())
            };

            LevelExpDataList.Add(data);
        }
    }
}