using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Define;
using Random = UnityEngine.Random;
using Transform = UnityEngine.Transform;

public static class Util
{
    public static T GetOrAddComponent<T>(GameObject go) where T : UnityEngine.Component
    {
        T component = go.GetComponent<T>();
        if (component == null)
            component = go.AddComponent<T>();
        return component;
    }

    public static GameObject FindChild(GameObject go, string name = null, bool recursive = false)
    {
        Transform transform = FindChild<Transform>(go, name, recursive);
        if (transform == null)
            return null;

        return transform.gameObject;
    }

    public static T FindChild<T>(GameObject go, string name = null, bool recursive = false) where T : UnityEngine.Object
    {
        if (go == null)
            return null;

        if (recursive == false)
        {
            for (int i = 0; i < go.transform.childCount; i++)
            {
                Transform transform = go.transform.GetChild(i);
                if (string.IsNullOrEmpty(name) || transform.name == name)
                {
                    T component = transform.GetComponent<T>();
                    if (component != null)
                        return component;
                }
            }
        }
        else
        {
            foreach (T component in go.GetComponentsInChildren<T>())
            {
                if (string.IsNullOrEmpty(name) || component.name == name)
                    return component;
            }
        }

        return null;
    }


    public static Color HexToColor(string color)
    {
        Color parsedColor;
        ColorUtility.TryParseHtmlString("#" + color, out parsedColor);

        return parsedColor;
    }

    //string값 으로 Enum값 찾기
    public static T ParseEnum<T>(string value)
    {
        return (T)Enum.Parse(typeof(T), value, true);
    }

    public static int GetClosestIndexBinary(float[] sortedData, float targetValue)
    {
        if (sortedData == null || sortedData.Length == 0) return -1;

        // C# 내장 이진 탐색 사용
        int index = Array.BinarySearch(sortedData, targetValue);


        // 1. 정확히 일치하는 값을 찾은 경우
        if (index >= 0) return index;

        // 2. 일치하는 값이 없으면, BinarySearch는 비트 반전된(~Index) '삽입 위치'를 반환함
        int nextIndex = ~index;

        // 2-1. 타겟이 배열의 모든 값보다 작을 때 (첫 번째 값이 가장 가까움)
        if (nextIndex == 0) return 0;

        // 2-2. 타겟이 배열의 모든 값보다 클 때 (마지막 값이 가장 가까움)
        if (nextIndex == sortedData.Length) return sortedData.Length - 1;

        // 2-3. 타겟이 두 값 사이에 있을 때: 앞뒤 값 중 더 가까운 쪽을 선택
        float leftDiff = Mathf.Abs(sortedData[nextIndex - 1] - targetValue);
        float rightDiff = Mathf.Abs(sortedData[nextIndex] - targetValue);

        return (leftDiff <= rightDiff) ? nextIndex - 1 : nextIndex;
    }
    public static int ConvertIndexToBay(int index)
    {
        return index * 4 + 2;
    }
    public static int ConvertBayToIndex(int bay)
    {
        return (bay - 2) / 4;
    }
}
