using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScene : BaseScene
{
    

    protected override void Init()
    {
        base.Init();
        Managers.PLC.StartPLCConnections();
        GM.InitVar();
    }

    public override void Clear()
    {
        
    }
}
