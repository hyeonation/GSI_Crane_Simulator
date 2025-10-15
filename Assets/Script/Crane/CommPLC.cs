using UnityEngine;
using System;
using S7.Net;

public struct InfoPLC
{
    public string ip;
    public short rack, slot;
    public short readDBNum, readStartIdx, readLength;
    public short writeDBNum, writeStartIdx, writeLength;
}

public class CommPLC
{
    public InfoPLC info;
    Plc plc;
    byte[] writeDB;
    byte boolByte;

    public CommPLC(
        string ip,
        short rack = 0,
        short slot = 1,
        short readDBNum = 1000,
        short readStartIdx = 0,
        short readLength = 36,
        short writeDBNum = 1001,
        short writeStartIdx = 0,
        short writeLength = 218
        )
    {
        info.rack = rack;
        info.slot = slot;
        info.readDBNum = readDBNum;
        info.readStartIdx = readStartIdx;
        info.readLength = readLength;
        info.writeDBNum = writeDBNum;
        info.writeStartIdx = writeStartIdx;
        info.writeLength = writeLength;

        writeDB = new byte[info.writeLength];
    }

    public void Connect()
    {

        // connect PLC
        plc = new Plc(CpuType.S71500, info.ip, info.rack, info.slot);
        plc.Open();

        // success or failure
        string plcState = plc.IsConnected ? "success" : "failure";

        // output log
        Debug.Log($"{info.ip} Connected {plcState}");
    }

    public byte[] ReadFromPLC()
    {
        byte[] data = plc.ReadBytes(DataType.DataBlock, info.readDBNum, info.readStartIdx, info.readLength);
        return data;
    }

    public void WriteToPLC(byte[] data)
    {
        plc.WriteBytes(DataType.DataBlock, info.writeDBNum, info.writeStartIdx, data);
    }

    public void WriteUnitydataToPLC()
    {

        float testFloat = 123.0f;
        int startIdx = 0;
        WriteFloat(testFloat, startIdx);

        // byte boolByte = 0;  // init
        // WriteBool(true, 0);
        // WriteBool(false, 1);
        // WriteBool(true, 2);
        // WriteBool(false, 3);
        // WriteBool(true, 4);

        // writeDB[204] = boolByte;

        // write to PLC
        WriteToPLC(writeDB);
    }

    public void WriteUnitydataToPLCQC()
    {

        float testFloat = 123.0f;
        int startIdx = 0;
        WriteFloat(testFloat, startIdx);

        // byte boolByte = 0;  // init
        // WriteBool(true, 0);
        // WriteBool(false, 1);
        // WriteBool(true, 2);
        // WriteBool(false, 3);
        // WriteBool(true, 4);

        // writeDB[204] = boolByte;

        // write to PLC
        WriteToPLC(writeDB);
    }

    void WriteFloat(float floatData, int startIdx)
    {
        const int lengthFloat = 4;
        Array.Copy(FloatToByteArr(floatData), 0, writeDB, startIdx, lengthFloat);
    }

    void WriteBool(bool boolData, int startPoint)
    {
        if (boolData) boolByte |= (byte)(1 << startPoint);
    }

    byte[] FloatToByteArr(float floatData)
    {
        // float -> byteArr
        byte[] bytes = BitConverter.GetBytes(floatData);

        // sync
        return reverseByteArr(bytes);
    }

    public void ReadPLCdata(int iCrane)
    {
        // DB start index
        const int floatStartIdxGantryVelBWD = 0;
        const int floatStartIdxGantryVelFWD = 4;
        const int floatStartIdxTrolleyVel = 8;
        const int floatStartIdxSpreaderVel = 12;
        const int floatStartIdxMM0Vel = 16;
        const int floatStartIdxMM1Vel = 20;
        const int floatStartIdxMM2Vel = 24;
        const int floatStartIdxMM3Vel = 28;

        const int boolStartIdxTwistLock = 34;
        const int boolStartPointTwlLock = 0;
        const int boolStartPointTwlUnlock = 1;

        // Read raw data from PLC
        var rawData = ReadFromPLC();

        // Read float data
        GM.cmdGantryVelFWD[iCrane] = ReadFloatData(rawData, floatStartIdxGantryVelFWD);
        GM.cmdGantryVelBWD[iCrane] = ReadFloatData(rawData, floatStartIdxGantryVelBWD);
        GM.cmdTrolleyVel[iCrane] = ReadFloatData(rawData, floatStartIdxTrolleyVel);
        GM.cmdSpreaderVel[iCrane] = ReadFloatData(rawData, floatStartIdxSpreaderVel);
        GM.cmdMM0Vel[iCrane] = ReadFloatData(rawData, floatStartIdxMM0Vel);
        GM.cmdMM1Vel[iCrane] = ReadFloatData(rawData, floatStartIdxMM1Vel);
        GM.cmdMM2Vel[iCrane] = ReadFloatData(rawData, floatStartIdxMM2Vel);
        GM.cmdMM3Vel[iCrane] = ReadFloatData(rawData, floatStartIdxMM3Vel);

        // Read boolean data
        GM.cmdTwlLock[iCrane] = ReadBoolData(rawData, boolStartIdxTwistLock, boolStartPointTwlLock);
        GM.cmdTwlUnlock[iCrane] = ReadBoolData(rawData, boolStartIdxTwistLock, boolStartPointTwlUnlock);
    }

    public void ReadPLCdataQC(int iCrane)
    {
        // DB start index
        const int floatStartIdxGantryVelBWD = 14;
        const int floatStartIdxGantryVelFWD = 14;
        const int floatStartIdxTrolleyVel = 8;
        const int floatStartIdxSpreaderVel = 2;
        // const int floatStartIdxMM0Vel = 16;
        // const int floatStartIdxMM1Vel = 20;
        // const int floatStartIdxMM2Vel = 24;
        // const int floatStartIdxMM3Vel = 28;

        const int boolStartIdxTwistLock = 28;
        const int boolStartPointTwlLock = 0;
        const int boolStartPointTwlUnlock = 1;

        const int boolStartIdxFeet = 22;
        const int boolStartPoint20Ft = 1;
        const int boolStartPoint40Ft = 2;
        const int boolStartPoint45Ft = 3;

        // Read raw data from PLC
        var rawData = ReadFromPLC();

        // Read float data
        GM.cmdGantryVelFWD[iCrane] = ReadFloatData(rawData, floatStartIdxGantryVelFWD);
        GM.cmdGantryVelBWD[iCrane] = ReadFloatData(rawData, floatStartIdxGantryVelBWD);
        GM.cmdTrolleyVel[iCrane] = ReadFloatData(rawData, floatStartIdxTrolleyVel);
        GM.cmdSpreaderVel[iCrane] = ReadFloatData(rawData, floatStartIdxSpreaderVel);

        // Read boolean data
        GM.cmdTwlLock[iCrane] = ReadBoolData(rawData, boolStartIdxTwistLock, boolStartPointTwlLock);
        GM.cmdTwlUnlock[iCrane] = ReadBoolData(rawData, boolStartIdxTwistLock, boolStartPointTwlUnlock);

        GM.cmd20ft[iCrane] = ReadBoolData(rawData, boolStartIdxFeet, boolStartPoint20Ft);
        GM.cmd40ft[iCrane] = ReadBoolData(rawData, boolStartIdxFeet, boolStartPoint40Ft);
        GM.cmd45ft[iCrane] = ReadBoolData(rawData, boolStartIdxFeet, boolStartPoint45Ft);
    }

    float ReadFloatData(byte[] rawData, int startIndex)
    {

        // float 4 bytes
        const int lengthFloatData = 4;

        // get byte array
        byte[] bytes = reverseByteArr(rawData[startIndex..(startIndex + lengthFloatData)]);

        // Convert byte array to float
        return BitConverter.ToSingle(bytes, 0); // Convert to float
    }

    bool ReadBoolData(byte[] rawData, int startIndex, int bitIndex)
    {
        // Check if the bit at bitIndex is set
        return (rawData[startIndex] & (1 << bitIndex)) != 0;
    }

    // for little-endian to big-endian conversion
    byte[] reverseByteArr(byte[] byteArr)
    {

        byte[] output = new byte[byteArr.Length];

        // Reverse the byte order for little-endian to big-endian conversion
        // Assuming rawData is in little-endian format, we need to reverse it
        for (int i = 0; i < byteArr.Length; i++)
        {
            int revIdx = byteArr.Length - 1 - i; // Reverse index
            output[i] = byteArr[revIdx];
        }

        return output;
    }

    // quit
    private void OnApplicationQuit()
    {
        plc.Close();
    }
}
