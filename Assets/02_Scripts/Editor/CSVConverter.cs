using System.Collections.Generic;
using System.IO;
using System.Linq;
using CoffeeCat.Utils;
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json;

namespace CoffeeCat.Editor
{
    public static class CSVConverter
    {
        private static readonly string csvFolderPath = Application.dataPath + "/08_Entities/CSV";
        private static readonly string jsonFolderPath = Application.dataPath + "/08_Entities/JSON";
        private static readonly string jsonResourcePath = Application.dataPath + "/Resources/Entity/Json";

        private static void ConvertCsvToJson(string csvPath, string jsonPath)
        {
            var records = new List<Dictionary<string, string>>();

            using (var reader = new StreamReader(csvPath))
            {
                bool isFirstLine = true;
                string[] headers = null;
                int lineCount = 0;

                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    lineCount++;

                    if (isFirstLine)
                    {
                        // 첫 번째 줄은 헤더로 저장
                        headers = ParseCsvLine(line);
                        isFirstLine = false;
                        continue;
                    }

                    // 첫 세 줄 건너뛰기 (4번째 줄부터 읽기)
                    if (lineCount <= 3)
                    {
                        continue;
                    }

                    var values = ParseCsvLine(line);
                    var record = new Dictionary<string, string>();

                    for (int i = 1; i < headers.Length; i++)
                    {
                        record[headers[i]] = values[i];
                    }

                    records.Add(record);
                }
            }

            // JSON으로 변환
            string json = JsonConvert.SerializeObject(records, Formatting.Indented);

            // JSON 파일로 저장
            File.WriteAllText(jsonPath, json);
        }

        private static string[] ParseCsvLine(string line)
        {
            var values = new List<string>();
            var current = new System.Text.StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                if (line[i] == '"')
                {
                    if (inQuotes && i < line.Length - 1 && line[i + 1] == '"')
                    {
                        // 이스케이프된 큰따옴표
                        current.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (line[i] == ';' && !inQuotes)
                {
                    values.Add(current.ToString());
                    current.Clear();
                }
                else
                {
                    current.Append(line[i]);
                }
            }

            values.Add(current.ToString());
            return values.ToArray();
        }

        [MenuItem("CoffeeCat/CSV/Convert All CSV to JSON")]
        private static void ConvertAllCSVToJson()
        {
            // Lock 파일 존재 여부 확인 (수정중인 파일이 존재하는지 체크)
            if (Directory.GetFiles(csvFolderPath, ".~lock.*.csv#").Any() ||
                Directory.GetFiles(csvFolderPath, "*.csv#").Any())
            {
                CatLog.WLog("Lock file found. CSV Conversion aborted.");
                return;
            }

            // CSV 파일 목록 가져오기
            int totalFilesCount = 0;
            string[] csvFiles = Directory.GetFiles(csvFolderPath, "*.csv");
            foreach (string csvFilePath in csvFiles)
            {
                // 각 CSV 파일을 JSON으로 변환
                string jsonFilePath =
                    Path.Combine(jsonFolderPath, Path.GetFileNameWithoutExtension(csvFilePath) + ".json");
                ConvertCsvToJson(csvFilePath, jsonFilePath);
                totalFilesCount++;
            }

            CatLog.Log($"All CSV to JSON conversion completed. (Total: {totalFilesCount.ToString()} Processed.)");
        }

        [MenuItem("CoffeeCat/CSV/Json To Resource Path")]
        private static void JsonToResourcePath()
        {
            // JSON 파일 목록 가져오기
            int totalProcessFileCount = 0;
            string[] jsonFiles = Directory.GetFiles(jsonFolderPath, "*.json");
            foreach (string jsonFilePath in jsonFiles)
            {
                // JSON 파일을 읽어서 암호화
                string json = File.ReadAllText(jsonFilePath);
                string encryptedJson = Cryptor.Encrypt2(json);

                // 암호화된 JSON 파일을 Resources 폴더로 저장
                string encryptedJsonFilePath =
                    Path.Combine(jsonResourcePath, Path.GetFileNameWithoutExtension(jsonFilePath) + ".enc");
                File.WriteAllText(encryptedJsonFilePath, encryptedJson);
                totalProcessFileCount++;
            }

            CatLog.Log($"All JSON to Copy Resource Path With Encrpyt Completed. (Total: {totalProcessFileCount.ToString()} Processed.)");
        }
    }
}