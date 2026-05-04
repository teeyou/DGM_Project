using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExcelDataReader;
using System.Data;
using System.IO;

public class ExcelReader : MonoBehaviour
{
    private DataSet result;

    private string FilePath => Path.Combine(Application.streamingAssetsPath, "digimon.xlsx");

    void Start()
    {
        LoadExcelData();
    }

    public void LoadExcelData()
    {
        string path = FilePath;
        using (var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
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
        }

        LoadSheet("BabyStatus");
        LoadSheet("RookieStatus");
        LoadSheet("ChampionStatus");
        LoadSheet("EnemyStatus");
        LoadSheet("GrowthType");
        LoadSheet("Evo");

    }

    public void LoadSheet(string sheetName)
    {
        if (result.Tables.Contains(sheetName))
        {
            DataTable table = result.Tables[sheetName];
            foreach (DataRow row in table.Rows)
            {
                for (int i = 0; i < row.Table.Columns.Count; i++)
                {
                    Debug.Log($"row[{i}] : {row[i]}");
                }
            }
        }
        else
        {
            Debug.Log($"시트 {sheetName}를 찾을 수 없습니다.");
        }
    }
}
