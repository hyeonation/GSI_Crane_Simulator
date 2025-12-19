using System;
using System.Runtime.InteropServices;
using UnityEngine;

public static class StructConverter
{
    // [최적화] 1. 바이트 배열 -> 구조체 (Zero Allocation 시도)
    // 반환값 대신 out 파라미터를 사용하여 명시적으로 처리
    public static void BytesToStruct<T>(byte[] packet, out T result) where T : struct
    {
        int size = Marshal.SizeOf(typeof(T));

        if (packet == null || packet.Length < size)
        {
            Debug.LogError($"[StructConverter] Size mismatch. Req: {size}");
            result = default(T);
            return;
        }

        // GCHandle을 사용해 배열 메모리를 고정(Pin)하여 복사 비용 최소화
        GCHandle handle = GCHandle.Alloc(packet, GCHandleType.Pinned);
        try
        {
            result = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
        }
        finally
        {
            handle.Free(); // 필수 해제
        }
    }

    // [최적화] 2. 구조체 -> 바이트 배열 (기존 버퍼 재사용)
    // 새로운 배열을 리턴하지 않고, 타겟 버퍼(targetBuffer)에 씁니다.
    public static void StructToBytes<T>(T obj, byte[] targetBuffer) where T : struct
    {
        int size = Marshal.SizeOf(obj);

        if (targetBuffer == null || targetBuffer.Length < size)
        {
            Debug.LogError($"[StructConverter] Target buffer too small. Req: {size}");
            return;
        }

        // GCHandle을 사용해 타겟 배열의 메모리 주소를 직접 얻음
        GCHandle handle = GCHandle.Alloc(targetBuffer, GCHandleType.Pinned);
        try
        {
            Marshal.StructureToPtr(obj, handle.AddrOfPinnedObject(), true);
        }
        finally
        {
            handle.Free(); // 필수 해제
        }
    }
}