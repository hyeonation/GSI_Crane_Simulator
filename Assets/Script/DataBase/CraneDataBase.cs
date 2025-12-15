using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CraneDataBase 
{
    // read
    public float readGantryVelBWD, readGantryVelFWD, readTrolleyVel, readSpreaderVel;
    public float readMM0Vel, readMM1Vel, readMM2Vel, readMM3Vel;
    public bool read20ft, read40ft, read45ft, readTwlLock, readTwlUnlock;

    // write
    public float writePosGantry, writePosTrolley, writePosSpreader;
}
