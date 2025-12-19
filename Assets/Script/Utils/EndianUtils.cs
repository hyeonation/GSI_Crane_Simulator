using System;
using System.Collections.Generic;
using System.Linq; // [Fix] OrderBy 사용을 위해 추가
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEngine;

public static class EndianUtils
{
    private static readonly Dictionary<Type, List<EndianFieldData>> _cache = new Dictionary<Type, List<EndianFieldData>>();

    private struct EndianFieldData
    {
        public int Offset;
        public int Size;
    }

    public static void AdjustEndianness<T>(byte[] buffer) where T : struct
    {
        Type type = typeof(T);

        if (!_cache.TryGetValue(type, out var fieldDataList))
        {
            fieldDataList = new List<EndianFieldData>();

            // [Fix] 필드 순서가 오프셋 순서와 일치하도록 명시적 정렬 (안전성 확보)
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance)
                             .OrderBy(f => Marshal.OffsetOf(type, f.Name).ToInt32());

            foreach (var field in fields)
            {
                int offset = Marshal.OffsetOf(type, field.Name).ToInt32();
                int size = 0;
                Type fType = field.FieldType;

                if (fType == typeof(int) || fType == typeof(uint) || fType == typeof(float)) size = 4;
                else if (fType == typeof(short) || fType == typeof(ushort) || fType == typeof(char)) size = 2;
                else if (fType == typeof(double) || fType == typeof(long) || fType == typeof(ulong)) size = 8;

                if (size > 1)
                {
                    fieldDataList.Add(new EndianFieldData { Offset = offset, Size = size });
                }
            }
            _cache[type] = fieldDataList;
        }

        int count = fieldDataList.Count;
        for (int i = 0; i < count; i++)
        {
            var data = fieldDataList[i];
            if (data.Offset + data.Size <= buffer.Length)
            {
                Array.Reverse(buffer, data.Offset, data.Size);
            }
        }
    }
}