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
        short writeDBNum = 1000,
        short writeStartIdx = 46,
        short writeLength = 218
        )
    {
        info.ip = ip;
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

    public void WriteToPLC()
    {
        plc.WriteBytes(DataType.DataBlock, info.writeDBNum, info.writeStartIdx, writeDB);
    }

    public void WriteFloat(float floatData, int startIdx)
    {
        const int lengthFloat = 4;
        Array.Copy(FloatToByteArr(floatData), 0, writeDB, startIdx, lengthFloat);
    }

    // C#에서 int는 4바이트. Short는 2바이트.
    // PLC에서 Int, Word를 Unity에서 Short로 처리한다.
    public void WriteShort(short intData, int startIdx)
    {
        const int lengthShort = 2;
        Array.Copy(reverseByteArr(BitConverter.GetBytes(intData)), 0, writeDB, startIdx, lengthShort);
    }

    public void WriteChar(char charData, int startIdx)
    {
        const int lengthChar = 1;
        Array.Copy(BitConverter.GetBytes(charData), 0, writeDB, startIdx, lengthChar);
    }

    public static byte WriteBool(bool boolData, int startPoint, byte boolByte)
    {
        byte outByte = boolByte;
        if (boolData) outByte |= (byte)(1 << startPoint);

        return outByte;
    }

    public void WriteByte(byte boolByte, int startidx)
    {
        writeDB[startidx] = boolByte;
    }

    byte[] FloatToByteArr(float floatData)
    {
        // float -> byteArr
        byte[] bytes = BitConverter.GetBytes(floatData);

        // sync
        return reverseByteArr(bytes);
    }

    public static float ReadFloatData(byte[] rawData, int startIndex)
    {

        // float 4 bytes
        const int lengthFloatData = 4;

        // get byte array
        byte[] bytes = reverseByteArr(rawData[startIndex..(startIndex + lengthFloatData)]);

        // Convert byte array to float
        return BitConverter.ToSingle(bytes, 0); // Convert to float
    }

    public static bool ReadBoolData(byte[] rawData, int startIndex, int bitIndex)
    {
        // Check if the bit at bitIndex is set
        return (rawData[startIndex] & (1 << bitIndex)) != 0;
    }

    // for little-endian to big-endian conversion
    public static byte[] reverseByteArr(byte[] byteArr)
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
