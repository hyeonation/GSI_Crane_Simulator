using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System;
using System.Linq; // 정렬을 위해 필요

public class PlcStructGenerator : EditorWindow
{
    private UnityEngine.Object csvFileObject;
    private string outputClassName = "CranePlcData";

    [MenuItem("Tools/PLC Struct Generator Window")]
    public static void ShowWindow()
    {
        GetWindow<PlcStructGenerator>("PLC Gen");
    }

    private void OnGUI()
    {
        GUILayout.Label("PLC Data Struct Generator (v2.0)", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        csvFileObject = EditorGUILayout.ObjectField("CSV File (.csv)", csvFileObject, typeof(UnityEngine.Object), false);
        outputClassName = EditorGUILayout.TextField("Output Class Name", outputClassName);

        EditorGUILayout.Space();

        if (GUILayout.Button("Generate C# Struct (Auto-Fix)"))
        {
            GenerateStruct();
        }
    }

    private void GenerateStruct()
    {
        if (csvFileObject == null || string.IsNullOrWhiteSpace(outputClassName))
        {
            EditorUtility.DisplayDialog("Error", "CSV 파일과 클래스 이름을 확인해주세요.", "OK");
            return;
        }

        string csvPath = AssetDatabase.GetAssetPath(csvFileObject);
        string outputPath = $"Assets/Script/DataBase/{outputClassName}.cs";

        ParseAndGenerate(csvPath, outputPath, outputClassName);
    }

    // 데이터를 저장할 임시 클래스 (정렬용)
    private class FieldInfoData
    {
        public int Offset;
        public string CodeLine;
        public bool IsBitField;
    }

    private void ParseAndGenerate(string csvPath, string outputPath, string className)
    {
        if (!File.Exists(csvPath)) return;

        string[] lines = File.ReadAllLines(csvPath);
        
        // 정렬을 위한 리스트
        List<FieldInfoData> generatedFields = new List<FieldInfoData>();
        // 비트 필드 임시 저장소
        Dictionary<int, List<string>> bitProperties = new Dictionary<int, List<string>>();

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            var parts = line.Split(',');
            if (parts.Length < 3) continue;

            string originalName = parts[0].Trim();
            string type = parts[1].Trim();
            string offsetStr = parts[2].Trim();

            // 헤더 스킵
            if (originalName.Equals("Name", StringComparison.OrdinalIgnoreCase) || type.Equals("Struct", StringComparison.OrdinalIgnoreCase)) continue;
            if (!double.TryParse(offsetStr, out double offsetRaw)) continue;

            // 1. 네이밍 살균 (숫자로 시작하면 _ 추가)
            string safeName = SanitizeName(originalName);

            int byteOffset = (int)Math.Floor(offsetRaw);

            if (type.Equals("Bool", StringComparison.OrdinalIgnoreCase))
            {
                int bitIndex = (int)Math.Round((offsetRaw - byteOffset) * 10);
                
                if (!bitProperties.ContainsKey(byteOffset))
                    bitProperties[byteOffset] = new List<string>();

                // 프로퍼티 생성 코드 저장
                bitProperties[byteOffset].Add($"    // Bit {bitIndex}: {originalName}");
                bitProperties[byteOffset].Add($"    public bool {safeName} => (_{byteOffset}_Raw & (1 << {bitIndex})) != 0;");
                
                // 비트 필드의 Backing Field는 나중에 한 번만 추가하기 위해 여기서는 넘어감
            }
            else
            {
                string csharpType = GetCSharpType(type);
                string code = $"    [FieldOffset({byteOffset})]\n    public {csharpType} {safeName};";
                
                generatedFields.Add(new FieldInfoData { Offset = byteOffset, CodeLine = code, IsBitField = false });
            }
        }

        // 비트 필드 Backing Field 처리 및 병합
        foreach (var kvp in bitProperties)
        {
            int offset = kvp.Key;
            StringBuilder sbBit = new StringBuilder();
            sbBit.AppendLine($"    [FieldOffset({offset})]");
            sbBit.AppendLine($"    public byte _{offset}_Raw; // Backing for bits");
            
            foreach (var prop in kvp.Value)
            {
                sbBit.AppendLine(prop);
            }

            generatedFields.Add(new FieldInfoData { Offset = offset, CodeLine = sbBit.ToString().TrimEnd(), IsBitField = true });
        }

        // 2. 오프셋 기준으로 정렬 (메모리 순서대로 코드 작성)
        var sortedFields = generatedFields.OrderBy(f => f.Offset).ToList();

        // 파일 쓰기 시작
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Runtime.InteropServices;");
        sb.AppendLine();
        sb.AppendLine("[Serializable]");
        sb.AppendLine("[StructLayout(LayoutKind.Explicit)]");
        sb.AppendLine($"public struct {className}");
        sb.AppendLine("{");

        foreach (var field in sortedFields)
        {
            sb.AppendLine(field.CodeLine);
        }

        sb.AppendLine("}");

        try 
        {
            File.WriteAllText(outputPath, sb.ToString());
            AssetDatabase.Refresh();
            Debug.Log($"<color=cyan>[PlcGen] Fixed & Generated: {className}</color>");
            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(outputPath));
        }
        catch (Exception e)
        {
            Debug.LogError($"Error: {e.Message}");
        }
    }

    // 이름 규칙 검사 및 수정 함수
    private string SanitizeName(string name)
    {
        if (string.IsNullOrEmpty(name)) return "Unnamed";
        
        // 숫자로 시작하는 경우
        if (char.IsDigit(name[0]))
        {
            return "_" + name;
        }
        
        // C# 키워드와 겹치는 경우 (필요 시 추가)
        string[] keywords = { "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "checked", "class", "const", "continue", "decimal", "default", "delegate", "do", "double", "else", "enum", "event", "explicit", "extern", "false", "finally", "fixed", "float", "for", "foreach", "goto", "if", "implicit", "in", "int", "interface", "internal", "is", "lock", "long", "namespace", "new", "null", "object", "operator", "out", "override", "params", "private", "protected", "public", "readonly", "ref", "return", "sbyte", "sealed", "short", "sizeof", "stackalloc", "static", "string", "struct", "switch", "this", "throw", "true", "try", "typeof", "uint", "ulong", "unchecked", "unsafe", "ushort", "using", "virtual", "void", "volatile", "while" };
        
        if (keywords.Contains(name))
        {
            return "@" + name;
        }

        return name;
    }

    private string GetCSharpType(string plcType)
    {
        switch (plcType.ToLower())
        {
            case "real": return "float";
            case "int": return "short"; 
            case "dint": return "int";
            case "word": return "ushort";
            default: return "float"; 
        }
    }
}