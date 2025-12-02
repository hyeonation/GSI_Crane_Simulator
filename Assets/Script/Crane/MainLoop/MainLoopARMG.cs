
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
        GM.readLength = 178;
        GM.writeDBNum = 2130;
        GM.writeStartIdx = 0;
        GM.writeLength = 302;
    }

    public override void ReadPLCdata(int iCrane)
    {

    }


    public override void WriteUnitydataToPLC(int iCrane)
    {
        // DB start index
        const int shortStartIdxJobID = 12;
        const int shortStartIdxJobType = 14;

        const int startIdxSRC = 18;
        const int startIdxDST = 118;
        const int startIdxCntrProp = 236;

        /////////////////////

        // Determine task info
        if (GM.listTaskInfo.Count != 0)
        {
            TaskInfo taskInfo = GM.listTaskInfo[0];

            plc[iCrane].WriteShort(taskInfo.jobID, shortStartIdxJobID);
            plc[iCrane].WriteShort(taskInfo.jobType, shortStartIdxJobType);

            WriteShiftPos(iCrane, taskInfo.srcPos, startIdxSRC, taskInfo.cntrInfoSO);
            WriteShiftPos(iCrane, taskInfo.dstPos, startIdxDST, taskInfo.cntrInfoSO);
            WriteContainerInfo(iCrane, taskInfo.cntrInfoSO, startIdxCntrProp);

            // UnityEngine.Debug.Log($"List Count: {GM.listTaskInfo.Count}, jobID: {taskInfo.jobID}, jobType: {taskInfo.jobType}");
        }

        // write to PLC
        plc[iCrane].WriteToPLC();
    }

    public void WriteShiftPos(int iCrane, ContainerPosition cntrPos, int startIdxCntrPos, ContainerInfoSO cntrInfoSO)
    {
        // init start index
        int startIdx = startIdxCntrPos;

        // constant
        const int blockMapLength = 8;   // short(2) + short(2) + float(4) = 8

        // block
        plc[iCrane].WriteShort(cntrPos.block, startIdx);
        startIdx += shortLength;

        // bay
        // x10 Converting
        plc[iCrane].WriteShort((short)(cntrPos.bay * 10), startIdx);
        startIdx += shortLength;

        // row
        // x10 Converting
        plc[iCrane].WriteShort((short)(cntrPos.row * 10), startIdx);
        startIdx += shortLength;

        // tier
        plc[iCrane].WriteShort(cntrPos.tier, startIdx);
        startIdx += shortLength;

        //// Block Map
        // Block Map start index
        startIdx += blockMapLength * cntrPos.row;

        // row num
        // Row, Bay 는 *10 으로 Converting
        plc[iCrane].WriteShort(cntrPos.row, startIdx);
        startIdx += shortLength;

        // tier num
        plc[iCrane].WriteShort(cntrPos.tier, startIdx);
        startIdx += shortLength;

        // max height
        float maxHeight = cntrPos.tier * cntrInfoSO.height;
        plc[iCrane].WriteFloat(maxHeight, startIdx);
    }

    public void WriteContainerInfo(int iCrane, ContainerInfoSO cntrInfoSO, int startIdxCtProp)
    {
        // init start index
        int startIdx = startIdxCtProp;

        // size
        plc[iCrane].WriteShort(cntrInfoSO.size, startIdx);
        startIdx += shortLength;

        // type
        plc[iCrane].WriteShort(cntrInfoSO.type, startIdx);
        startIdx += shortLength;

        // position
        // -1: Undefined, 1: Front, 2: Center, 3: Rear
        plc[iCrane].WriteShort(2, startIdx);
        startIdx += shortLength;
        startIdx += shortLength;    // number

        // container code
        int idxContainerID = GM.FindContainerIndex(cntrInfoSO.strContainerID);
        byte[] byteContainerID = GM.listContainerID[idxContainerID];
        for (int i = 0; i < byteContainerID.Length; i++)
            plc[iCrane].WriteByte(byteContainerID[i], startIdx + (charLength * i));
    }
}
