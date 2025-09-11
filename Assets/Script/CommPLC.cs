using UnityEngine;
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

    public CommPLC(string ip)
    {
        // 나중에 일반화할 때는 생성자 parameter에 InfoPLC를 추가하면 될 것
        info.ip = ip;
        info.rack = 0;
        info.slot = 1;
        info.readDBNum = 1000;
        info.readStartIdx = 0;
        info.readLength = 36;
        info.writeDBNum = 1001;
        info.writeStartIdx = 0;
        info.writeLength = 218;
    }

    public void Connect()
    {

        // connect PLC
        plc = new Plc(CpuType.S71500, info.ip, info.rack, info.slot);
        plc.Open();

        // success or failure
        string plcState;

        // success
        if (plc.IsConnected)
        {
            plcState = "success";

            // init values
            InitializePLCValues();
        }

        // failure
        else
        {
            plcState = "failure";
        }

        // output log
        Debug.Log($"{info.ip} Connected {plcState}");
    }

    // 추후 WriteBytes로 바꿔보자
    void InitializePLCValues()
    {
        // plc.Write($"DB{info.writeDBNum}.DBX254.4", false);
        // plc.Write($"DB{info.writeDBNum}.DBX254.5", true);
        // plc.Write($"DB{info.writeDBNum}.DBX254.6", false);
        // plc.Write($"DB{info.writeDBNum}.DBX44.0", false);
        // plc.Write($"DB{info.writeDBNum}.DBX44.1", true);
        // // plc.Write($"DB{info.writeDBNum}.DBX268.0", false);
        // // plc.Write($"DB{info.writeDBNum}.DBX268.1", true);
    }

    public byte[] ReadToPLC()
    {
        byte[] data = plc.ReadBytes(DataType.DataBlock, info.readDBNum, info.readStartIdx, info.readLength);
        return data;
    }

    public void WriteToPLC(byte[] data)
    {
        plc.WriteBytes(DataType.DataBlock, info.writeDBNum, info.writeStartIdx, data);
    }

    // quit
    private void OnApplicationQuit()
    {
        plc.Close();
    }
}
