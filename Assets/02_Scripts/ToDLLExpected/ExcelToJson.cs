using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;
using System.Text;
using ExcelDataReader;
using Newtonsoft.Json;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CoffeeCat.Editor
{
    public class ExcelToJson
    {
#if UNITY_EDITOR
        private List<FileInfo> sourceFiles;

        private string savePath;

        private string xlxsPath;

        private int sheetCount;

        private int headerRows;

        private void SetPath(string savePathValue, string xlxsPathValue)
        {
            savePath = Directory.GetCurrentDirectory() + "\\Assets\\" + savePathValue;
            xlxsPath = Directory.GetCurrentDirectory() + "\\Assets\\" + xlxsPathValue;
        }

        private void Allocate()
        {
            sourceFiles = new List<FileInfo>();
            sheetCount = 1;
            headerRows = 2;
            ExcelFileLoad();
        }

        private void Initialize()
        {
            Allocate();
        }

        public void ProcessStart(string savePathValue, string xlxsPathValue)
        {
            SetPath(savePathValue, xlxsPathValue);
            Initialize();
            ReadAllTables(SaveSheetJson);
        }

        private void ExcelFileLoad()
        {
            FileInfo[] files = new DirectoryInfo(xlxsPath).GetFiles();
            foreach (FileInfo item in files)
            {
                sourceFiles.Add(item);
            }
        }

        private void ReadAllTables(Func<DataTable, string, int> exportFunc)
        {
            if (sourceFiles == null || sourceFiles.Count <= 0)
            {
                Console.WriteLine("File. Excel File Is None.");
                return;
            }

            int num = 0;
            for (int i = 0; i < sourceFiles.Count; i++)
            {
                FileInfo fileInfo = sourceFiles[i];
                num += ReadTable(fileInfo.FullName, GetFileName(fileInfo.Name), exportFunc);
            }
        }

        private int ReadTable(string path, string fileName, Func<DataTable, string, int> exportFunc)
        {
            int num = 0;
            using FileStream fileStream = File.Open(path, FileMode.Open, FileAccess.Read);
            if (fileStream.Name.LastIndexOf(".meta") == -1)
            {
                using (IExcelDataReader excelDataReader = ExcelReaderFactory.CreateReader(fileStream))
                {
                    int resultsCount = excelDataReader.ResultsCount;
                    DataSet dataSet = excelDataReader.AsDataSet();
                    int num2 = ((sheetCount <= 0) ? resultsCount : sheetCount);
                    for (int i = 0; i < num2; i++)
                    {
                        if (i < resultsCount)
                        {
                            string arg = ((num2 == 1) ? fileName : (fileName + "_" + dataSet.Tables[i].TableName));
                            num += exportFunc(dataSet.Tables[i], arg);
                        }
                    }

                    return num;
                }
            }

            return num;
        }

        private int SaveSheetJson(DataTable sheet, string fileName)
        {
            int count = sheet.Columns.Count;
            int count2 = sheet.Rows.Count;
            List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
            for (int i = headerRows; i < count2; i++)
            {
                Dictionary<string, object> dictionary = new Dictionary<string, object>();
                for (int j = 1; j < count; j++)
                {
                    if (!(sheet.Rows[i][j].ToString() == ""))
                    {
                        string type = sheet.Rows[0][j].ToString();
                        string key = sheet.Rows[1][j].ToString();
                        dictionary.Add(key, SetObjectField(type, sheet.Rows[i][j].ToString()));
                    }
                }

                list.Add(dictionary);
            }

            string value = JsonConvert.SerializeObject(new Wrapper<Dictionary<string, object>>
            {
                array = list.ToArray()
            }, Formatting.Indented);
            string text = savePath + "/" + fileName + ".json";
            using FileStream stream = new FileStream(text, FileMode.Create, FileAccess.Write);
            using TextWriter textWriter = new StreamWriter(stream, Encoding.UTF8);
            textWriter.Write(value);
            Console.WriteLine("File Saved : " + text);
            return 1;
        }

        private object SetObjectField(string type, string parameter)
        {
            object result = parameter;
            try
            {
                switch (type.ToLower())
                {
                    case "s[]":
                        result = parameter.Split(new char[1] { ',' });
                        return result;
                    case "b":
                        result = bool.Parse(parameter);
                        return result;
                    case "n":
                        result = int.Parse(parameter);
                        return result;
                    case "n[]":
                        result = Array.ConvertAll(parameter.Split(new char[1] { ',' }), (string element) => int.Parse(element));
                        return result;
                    case "f":
                        result = float.Parse(parameter);
                        return result;
                    case "f[]":
                        result = Array.ConvertAll(parameter.Split(new char[1] { ',' }), (string element) => float.Parse(element));
                        return result;
                    case "d":
                        result = double.Parse(parameter);
                        return result;
                    default:
                        {
                            Type type2 = Assembly.Load("Assembly-CSharp").GetType(type);
                            if (type2 != null)
                            {
                                if (type2.IsEnum)
                                {
                                    result = Enum.Parse(type2, parameter);
                                    return result;
                                }

                                return result;
                            }

                            return result;
                        }
                    case "s":
                        return result;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed.Check Data Type : " + type);
                Console.WriteLine(ex.Source);
                return result;
            }
        }

        private string GetFileName(string fileName)
        {
            int num = fileName.LastIndexOf('.');
            if (num == -1)
            {
                return fileName;
            }

            return fileName.Substring(0, num);
        }

        private class Wrapper<T>
        {
            public Dictionary<string, object>[] array { get; set; }
        }
#endif
    }
}
