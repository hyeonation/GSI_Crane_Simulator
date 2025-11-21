using System;
using UnityEngine;

// Organizing data
public class MainLoopQC : MainLoop
{
    // Inspector Write values
    [Header("Write values")]
    public Mi mi;
    public Hoist hoist;
    public Trolley trolley;
    public Gantry gantry;
    public Boom boom;
    public Spreader spreader;
    public Protection protection;

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
        byte boolByte;

        // Mi 
        boolByte = 0;  // init
        CommPLC.WriteBool(mi.up, 0, boolByte);
        CommPLC.WriteBool(mi.down, 1, boolByte);
        CommPLC.WriteBool(mi.forward, 2, boolByte);
        CommPLC.WriteBool(mi.backward, 3, boolByte);
        CommPLC.WriteBool(mi.left, 4, boolByte);
        CommPLC.WriteBool(mi.right, 5, boolByte);
        CommPLC.WriteBool(mi.sprdSingle, 6, boolByte);
        CommPLC.WriteBool(mi.sprdTwin, 7, boolByte);

        plc[iCrane].WriteByte(boolByte, 0);

        boolByte = 0;  // init
        CommPLC.WriteBool(mi.sprd20Ft, 0, boolByte);
        CommPLC.WriteBool(mi.sprd40Ft, 1, boolByte);
        CommPLC.WriteBool(mi.sprd45Ft, 2, boolByte);
        CommPLC.WriteBool(mi.sprdTwinExpand, 3, boolByte);
        CommPLC.WriteBool(mi.sprdTwinRetract, 4, boolByte);
        CommPLC.WriteBool(mi.flipLsLeftUp, 5, boolByte);
        CommPLC.WriteBool(mi.flipLsLeftDn, 6, boolByte);
        CommPLC.WriteBool(mi.flipSsLeftUp, 7, boolByte);


        plc[iCrane].WriteShort(-1231, 230);


        plc[iCrane].WriteByte(boolByte, 1);

        // int
        plc[iCrane].WriteShort(-123, 216);


        // word
        plc[iCrane].WriteShort(-1231, 230);

        // char
        plc[iCrane].WriteChar('d', 200);


        float testFloat = 123.0f;
        plc[iCrane].WriteFloat(testFloat, 84);

        // byte boolByte = 0;  // init
        // CommPLC.WriteBool(true, 0, boolByte);
        // CommPLC.WriteBool(false, 1, boolByte);
        // CommPLC.WriteBool(false, 2, boolByte);

        // plc[iCrane].WriteByte(boolByte, 204);

        // write to PLC
        plc[iCrane].WriteToPLC();

    }
}
