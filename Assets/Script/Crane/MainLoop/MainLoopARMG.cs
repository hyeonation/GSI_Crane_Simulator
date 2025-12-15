
// Organizing data
using System.Diagnostics;
using UnityEngine;

public class MainLoopARMG : MainLoop
{

    // plc constant
    const int shortLength = 2;
    const int charLength = 1;

    // Crane 별 고유 변수
    public override void InitCraneSpecVar()
    {
        GM.readDBNum = 2140;
        GM.readLength = 186;
        GM.writeDBNum = 2130;
        GM.writeStartIdx = 0;
        GM.writeLength = 314;
    }

    public override void ReadPLCdata(int iCrane)
    {
        // DB start index
        const int floatStartIdxGantryVelFWD = 60;
        const int floatStartIdxTrolleyVel = 64;
        const int floatStartIdxSpreaderVel = 68;
        const int floatStartIdxMM0Vel = 16;
        const int floatStartIdxMM1Vel = 20;
        const int floatStartIdxMM2Vel = 24;
        const int floatStartIdxMM3Vel = 28;

        const int boolStartIdxSprdStatus = 74;
        const int boolStartPoint20ft = 0;
        const int boolStartPoint40ft = 1;
        const int boolStartPoint45ft = 2;
        const int boolStartPointTwlUnlock = 3;

        const int shortStartIdxCamIndex = 178;
        

        // Read raw data from PLC
        var rawData = GM.plc[iCrane].ReadFromPLC();

        // Read float data
        GM.arrayCraneDataBase[iCrane].readGantryVelFWD = CommPLC.ReadFloatData(rawData, floatStartIdxGantryVelFWD);    
        GM.arrayCraneDataBase[iCrane].readGantryVelBWD = GM.arrayCraneDataBase[iCrane].readGantryVelFWD;
        GM.arrayCraneDataBase[iCrane].readTrolleyVel = CommPLC.ReadFloatData(rawData, floatStartIdxTrolleyVel);
        GM.arrayCraneDataBase[iCrane].readSpreaderVel = CommPLC.ReadFloatData(rawData, floatStartIdxSpreaderVel);
        GM.arrayCraneDataBase[iCrane].readMM0Vel = CommPLC.ReadFloatData(rawData, floatStartIdxMM0Vel);
        GM.arrayCraneDataBase[iCrane].readMM1Vel = CommPLC.ReadFloatData(rawData, floatStartIdxMM1Vel);    
        GM.arrayCraneDataBase[iCrane].readMM2Vel = CommPLC.ReadFloatData(rawData, floatStartIdxMM2Vel);
        GM.arrayCraneDataBase[iCrane].readMM3Vel = CommPLC.ReadFloatData(rawData, floatStartIdxMM3Vel);

        // Read boolean data
        GM.arrayCraneDataBase[iCrane].read20ft = CommPLC.ReadBoolData(rawData, boolStartIdxSprdStatus, boolStartPoint20ft);
        GM.arrayCraneDataBase[iCrane].read40ft = CommPLC.ReadBoolData(rawData, boolStartIdxSprdStatus, boolStartPoint40ft);
        GM.arrayCraneDataBase[iCrane].read45ft = CommPLC.ReadBoolData(rawData, boolStartIdxSprdStatus, boolStartPoint45ft);
        GM.arrayCraneDataBase[iCrane].readTwlLock = CommPLC.ReadBoolData(rawData, boolStartIdxSprdStatus, boolStartPointTwlUnlock);
        GM.arrayCraneDataBase[iCrane].readTwlUnlock = CommPLC.ReadBoolData(rawData, boolStartIdxSprdStatus, boolStartPointTwlUnlock);

        // read cam index data
        GM.arrayCraneDataBase[iCrane].readCam1 = CommPLC.ReadShortData(rawData, shortStartIdxCamIndex);
    }


    public override void WriteUnitydataToPLC(int iCrane)
    {
        CraneDataBase craneData = GM.arrayCraneDataBase[iCrane];
        // DB start index
        const int shortStartIdxJobID = 12;
        const int shortStartIdxJobType = 14;

        const int startIdxSRC = 18;
        const int startIdxDST = 118;
        const int startIdxCntrProp = 236;
        
        
        const int startIdxGantryPos = 302;
        const int startIdxTrolleyPos = 306;
        const int startIdxHoistPos = 310;

        /////////////////////
        /// 
        // Determine task info
        if (GM.listTaskInfo.Count != 0)
        {
            TaskInfo taskInfo = GM.listTaskInfo[0];

            GM.plc[iCrane].WriteShort(taskInfo.jobID, shortStartIdxJobID);
            GM.plc[iCrane].WriteShort(taskInfo.jobType, shortStartIdxJobType);

            WriteShiftPos(iCrane, taskInfo.srcPos, startIdxSRC, taskInfo.cntrInfoSO);
            WriteShiftPos(iCrane, taskInfo.dstPos, startIdxDST, taskInfo.cntrInfoSO);
            WriteContainerInfo(iCrane, taskInfo.cntrInfoSO, taskInfo.strContainerID, startIdxCntrProp);

            // GM.plc[iCrane].WriteFloat(taskInfo.jobType, shortStartIdxJobType);


            // UnityEngine.Debug.Log($"List Count: {GM.listTaskInfo.Count}, jobID: {taskInfo.jobID}, jobType: {taskInfo.jobType}");

            
        }

        // 실제 위치와 PLC에 쓸 위치 offset 보정 
        GM.plc[iCrane].WriteFloat(craneData.writePosGantry + Define.OffsetRMGCGantryZ, startIdxGantryPos);
        GM.plc[iCrane].WriteFloat(craneData.writePosTrolley + Define.OffsetRMGCTrolleyX, startIdxTrolleyPos);
        GM.plc[iCrane].WriteFloat(craneData.writePosSpreader + Define.OffsetRMGCHoistY, startIdxHoistPos );

        // write to PLC
        GM.plc[iCrane].WriteToPLC();
    }

    public void WriteShiftPos(int iCrane, ContainerPosition cntrPos, int startIdxCntrPos, ContainerInfoSO cntrInfoSO)
    {
        // init start index
        int startIdx = startIdxCntrPos;

        // constant
        const int blockMapLength = 8;   // short(2) + short(2) + float(4) = 8

        // block
        GM.plc[iCrane].WriteShort(cntrPos.block, startIdx);
        startIdx += shortLength;

        // bay
        // x10 Converting
        short convBay = (short)(cntrPos.bay * 10);
        GM.plc[iCrane].WriteShort(convBay, startIdx);
        startIdx += shortLength;

        // row
        // x10 Converting
        short convRow = (short)((cntrPos.row + 1) * 10);
        GM.plc[iCrane].WriteShort(convRow, startIdx);
        startIdx += shortLength;

        // tier
        GM.plc[iCrane].WriteShort(cntrPos.tier, startIdx);
        startIdx += shortLength;

        //// Block Map
        // Block Map start index
        startIdx += blockMapLength * cntrPos.row;

        // row num
        GM.plc[iCrane].WriteShort(convRow, startIdx);
        startIdx += shortLength;

        // tier num
        GM.plc[iCrane].WriteShort(cntrPos.tier, startIdx);
        startIdx += shortLength;

        // max height
        float maxHeight = cntrPos.tier * cntrInfoSO.height;
        GM.plc[iCrane].WriteFloat(maxHeight, startIdx);
    }

    public void WriteContainerInfo(int iCrane, ContainerInfoSO cntrInfoSO, string strContainerID, int startIdxCtProp)
    {
        // init start index
        int startIdx = startIdxCtProp;

        // size
        GM.plc[iCrane].WriteShort(cntrInfoSO.size, startIdx);
        startIdx += shortLength;

        // type
        GM.plc[iCrane].WriteShort(cntrInfoSO.type, startIdx);
        startIdx += shortLength;

        // position
        // -1: Undefined, 1: Front, 2: Center, 3: Rear
        GM.plc[iCrane].WriteShort(2, startIdx);
        startIdx += shortLength;
        startIdx += shortLength;    // number

        // container code
        int idxContainerID = GM.FindContainerIndex(strContainerID);
        byte[] byteContainerID = GM.stackProfile.listID[idxContainerID];
        for (int i = 0; i < byteContainerID.Length; i++)
            GM.plc[iCrane].WriteByte(byteContainerID[i], startIdx + (charLength * i));
    }
}
