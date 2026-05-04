using ExcelDataReader;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using UnityEngine;

public class EvoTree
{
    public int ID;
    public int Next;
}

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
}

public class ExcelReader
{
    private DataSet result;

    public List<StatusData> StatusList { get; private set; } = new List<StatusData>();
    public List<GrowthType> GrowthTypeList { get; private set; } = new List<GrowthType>();
    public List<EvoTree> EvoTreeList { get; private set; } = new List<EvoTree>();

    private string FilePath => Path.Combine(Application.streamingAssetsPath, "digimon.xlsx");

    public void LoadExcelData()
    {
        using (var stream = File.Open(FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        using (var reader = ExcelReaderFactory.CreateReader(stream))
        {
            result = reader.AsDataSet(new ExcelDataSetConfiguration()
            {
                ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
                {
                    UseHeaderRow = true
                }
            });
        }

        LoadSheet("BabyStatus");
        LoadSheet("RookieStatus");
        LoadSheet("ChampionStatus");
        LoadSheet("EnemyStatus");
        LoadSheet("GrowthType");
        LoadSheet("EvoTree");
    }

    private void LoadSheet(string sheetName)
    {
        if (!result.Tables.Contains(sheetName))
        {
            Debug.Log($"시트 없음 : {sheetName}");
            return;
        }

        DataTable table = result.Tables[sheetName];

        if (sheetName.Contains("Status"))
            ParseStatusSheet(table);
        else if (sheetName == "GrowthType")
            ParseGrowthTypeSheet(table);
        else if (sheetName == "EvoTree")
            ParseEvoTreeSheet(table);
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
                GrowthType = row["GrowthType"].ToString()
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

    private void ParseEvoTreeSheet(DataTable table)
    {
        foreach (DataRow row in table.Rows)
        {
            EvoTree evoTree = new EvoTree()
            {
                ID = int.Parse(row["ID"].ToString()),
                Next = int.Parse(row["Next"].ToString())
            };
            EvoTreeList.Add(evoTree);
        }
    }
}