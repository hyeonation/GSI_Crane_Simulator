using System;
using UnityEngine;

// Organizing data
public class MainLoopQC : MainLoop
{
    public override void ReadPLCdata(int iCrane)
    {
        // DB start index
        const int floatStartIdxGantryVelBWD = 14;
        const int floatStartIdxGantryVelFWD = 14;
        const int floatStartIdxTrolleyVel = 8;
        const int floatStartIdxSpreaderVel = 2;

        const int boolStartIdxTwistLock = 28;
        const int boolStartPointTwlLock = 0;
        const int boolStartPointTwlUnlock = 1;

        const int boolStartIdxFeet = 22;
        const int boolStartPoint20Ft = 1;
        const int boolStartPoint40Ft = 2;
        const int boolStartPoint45Ft = 3;

        // Read raw data from PLC
        var rawData = plc[iCrane].ReadFromPLC();

        // Read float data
        GM.cmdGantryVelFWD[iCrane] = CommPLC.ReadFloatData(rawData, floatStartIdxGantryVelFWD);
        GM.cmdGantryVelBWD[iCrane] = CommPLC.ReadFloatData(rawData, floatStartIdxGantryVelBWD);
        GM.cmdTrolleyVel[iCrane] = CommPLC.ReadFloatData(rawData, floatStartIdxTrolleyVel);
        GM.cmdSpreaderVel[iCrane] = CommPLC.ReadFloatData(rawData, floatStartIdxSpreaderVel);

        // Read boolean data
        GM.cmdTwlLock[iCrane] = CommPLC.ReadBoolData(rawData, boolStartIdxTwistLock, boolStartPointTwlLock);
        GM.cmdTwlUnlock[iCrane] = CommPLC.ReadBoolData(rawData, boolStartIdxTwistLock, boolStartPointTwlUnlock);

        GM.cmd20ft[iCrane] = CommPLC.ReadBoolData(rawData, boolStartIdxFeet, boolStartPoint20Ft);
        GM.cmd40ft[iCrane] = CommPLC.ReadBoolData(rawData, boolStartIdxFeet, boolStartPoint40Ft);
        GM.cmd45ft[iCrane] = CommPLC.ReadBoolData(rawData, boolStartIdxFeet, boolStartPoint45Ft);
    }

    public override void WriteUnitydataToPLC(int iCrane)
    {

        float testFloat = 123.0f;
        int startIdx = 0;
        plc[iCrane].WriteFloat(testFloat, startIdx);

        // byte boolByte = 0;  // init
        // CommPLC.WriteBool(true, 0, boolByte);
        // CommPLC.WriteBool(false, 1, boolByte);
        // CommPLC.WriteBool(false, 2, boolByte);

        // plc[iCrane].WriteByte(boolByte, 204);

        // write to PLC
        plc[iCrane].WriteToPLC();
    }
}
