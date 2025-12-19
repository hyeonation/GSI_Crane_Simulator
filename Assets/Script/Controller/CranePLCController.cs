using UnityEngine;
using S7.Net;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.Runtime.InteropServices;

public class CranePLCController : PLCController
{
    [Header("Debug Info")]
    [SerializeField] private string ipAddress;
    [SerializeField] private string connectionStatus = "Disconnected";
    public bool isConnected {get; private set; } = false;

    // 읽기 전용 데이터베이스
    CranePlcReadData redaDataBase;
    //TODO:  쓰기 전용 데이터베이스 읽기 db성공하면 추가

    // --- S7.Net Settings ---
    private Plc plc;
    private int readDB, readLen, writeDB, writeLen;
    private CpuType cpuType = CpuType.S71500;
    private short rack = 0;
    private short slot = 1;

    // --- Buffers & Locks ---
    private byte[] readBuffer;
    private byte[] writeBuffer;
    private byte[] cachedSendBuffer;
    private byte[] serializationBufferRead;
    private byte[] serializationBufferWrite;

    private readonly object bufferLock = new object(); 
    private CancellationTokenSource cancelSource;
    private const int RECONNECT_DELAY_MS = 3000;

    public void Initialize(string ip, int rDB, int rLen, int wDB, int wLen)
    {
        CheckStructAlignment(); // 구조체 정렬 검증

        this.ipAddress = ip;
        this.readDB = rDB; this.readLen = rLen;
        this.writeDB = wDB; this.writeLen = wLen;

        readBuffer = new byte[rLen];
        writeBuffer = new byte[wLen];
        serializationBufferRead = new byte[rLen];
        serializationBufferWrite = new byte[wLen];
        
        cachedSendBuffer = new byte[wLen];

        redaDataBase = new CranePlcReadData();

        // 엔디안 스왑 준비
        EndianUtils.AdjustEndianness<CranePlcReadData>(serializationBufferRead);
        // EndianUtils.AdjustEndianness<CranePlcReadData>(serializationBufferWrite);

        cancelSource = new CancellationTokenSource();
        Task.Run(() => ConnectionLoopAsync(cancelSource.Token));
    }

    private void CheckStructAlignment()
    {
        Debug.Log("---------- [PLC Struct Alignment Check] Start ----------");
        Type type = typeof(CraneDataBase);
        int totalSize = Marshal.SizeOf(type);
        Debug.Log($"Target Struct: {type.Name} | Total Size: {totalSize} Bytes");

        PrintOffset(type, "GantryVel");
        PrintOffset(type, "Status");
        PrintOffset(type, "_statusFlags"); // 변경된 필드명 확인
        PrintOffset(type, "GantryBPosX");
        
        Debug.Log("---------- [PLC Struct Alignment Check] End ----------");
    }

    private void PrintOffset(Type type, string fieldName)
    {
        try
        {
            // private 필드도 찾을 수 있도록 BindingFlags 추가
            var field = type.GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                int offset = Marshal.OffsetOf(type, fieldName).ToInt32();
                Debug.Log($"Field: {fieldName,-15} | Offset: {offset}");
            }
            else
            {
                Debug.LogWarning($"Field: {fieldName,-15} | Not Found in Struct");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Check Failed for {fieldName}: {e.Message}");
        }
    }

    private async Task ConnectionLoopAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            UpdateStatus("Connecting...");
            plc = new Plc(cpuType, ipAddress, rack, slot);

            try
            {
                await plc.OpenAsync();
            }
            catch (Exception ex)
            {
                UpdateStatus($"Connection Failed: {ex.Message}");
                await Task.Delay(RECONNECT_DELAY_MS, token);
                continue; 
            }

            if (plc.IsConnected)
            {
                isConnected = true;
                UpdateStatus("Connected");
                await DataExchangeLoopAsync(token);
                
                isConnected = false;
                plc.Close();
                UpdateStatus("Disconnected. Retrying...");
            }

            await Task.Delay(RECONNECT_DELAY_MS, token);
        }
    }

    private async Task DataExchangeLoopAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested && plc.IsConnected)
        {
            try
            {
                // A. Read from PLC
                var receivedBytes = await plc.ReadBytesAsync(DataType.DataBlock, readDB, 0, readLen);
                if (receivedBytes != null && receivedBytes.Length == readLen)
                {
                    lock (bufferLock)
                    {
                        Array.Copy(receivedBytes, readBuffer, readLen);
                    }
                }

                // B. Write to PLC
                bool shouldWrite = false;
                lock (bufferLock)
                {
                    if (writeBuffer != null)
                    {
                        Array.Copy(writeBuffer, cachedSendBuffer, writeLen);
                        shouldWrite = true;
                    }
                }

                if (shouldWrite)
                {
                    await plc.WriteBytesAsync(DataType.DataBlock, writeDB, 0, cachedSendBuffer);
                }

                await Task.Delay(33, token); // ~30Hz
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Communication Error: {e.Message}");
                break; 
            }
        }
    }

    void Update()
    {
        if (!isConnected) return;

        lock (bufferLock)
        {
            // 1. Read Buffer -> Struct
            Array.Copy(readBuffer, serializationBufferRead, readLen);
            EndianUtils.AdjustEndianness<CranePlcReadData>(serializationBufferRead);
            StructConverter.BytesToStruct<CranePlcReadData>(serializationBufferRead, out redaDataBase);

            // 2. Struct -> Write Buffer
            // StructConverter.StructToBytes(database.WriteData, serializationBufferWrite);
            // EndianUtils.AdjustEndianness<CraneDataBase>(serializationBufferWrite);
            // Array.Copy(serializationBufferWrite, writeBuffer, writeLen);
        }
    }

    public CranePlcReadData GetReadDataSnapshot()
    {
        lock (bufferLock)
        {
            return redaDataBase;
        }
    }

    public void SetGantryControl(float angle, bool isEmergency)
    {
        lock (bufferLock)
        {
            // database.WriteData.GantryAngle = angle;
            // 비상 정지 로직 등...
        }
    }
    
    public void SetSimulationFeedback(float currentGantryPos)
    {
        lock (bufferLock)
        {
            // database.WriteData.GantryBPosX = currentGantryPos;
        }
    }

    private void UpdateStatus(string status)
    {
        connectionStatus = status;
    }

    void OnDestroy()
    {
        cancelSource?.Cancel();
        if (plc != null) plc.Close();
    }
}