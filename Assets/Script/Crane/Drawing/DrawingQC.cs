using UnityEngine;
using System;
using Filo;

public class DrawingQC : DrawingCrane
{
    float convHoistVel;

    public override void InitValues()
    {
    }

    public override void FindObject()
    {

        // self crane info
        nameSelf = gameObject.name;
        iSelf = Array.IndexOf(GM.nameCranes, nameSelf);

        craneBody = gameObject.transform.Find("Body");

        var cable = craneBody.transform.Find($"Cable");
        cables = new GameObject[cable.transform.childCount - 1];
        for (short j = 0; j < cables.Length; j++)
        {
            var cableTransform = cable.transform.Find($"Cable{j}");
            cables[j] = cableTransform.gameObject;
        }

        // Get Objects From Trolley
        trolley = craneBody.transform.Find("Trolley");
        var disc = trolley.transform.Find("Disc");
        discs = new Transform[disc.transform.childCount];
        for (short j = 0; j < discs.Length; j++)
        {
            discs[j] = disc.transform.Find($"Disc{j}");
        }

        // convHoistVel
        float upperHoistDiameter = disc.GetChild(0).GetComponent<CableDisc>().radius;
        convHoistVel = 360 / (upperHoistDiameter * Mathf.PI);

        // spreaderCam = trolley.transform.Find("Get_View_Camera");

        // Get Objects From Spreader
        spreader = gameObject.transform.Find("Spreader");

        feet[0] = spreader.transform.Find("Spreader_0");
        var twistlock_0 = feet[0].transform.Find("TwistLock");

        twlLand[0] = twistlock_0.transform.Find("Land_0");
        twlLand[1] = twistlock_0.transform.Find("Land_1");

        twlLock[0] = twistlock_0.transform.Find("Lock_0");
        twlLock[1] = twistlock_0.transform.Find("Lock_1");

        var laser_0 = feet[0].transform.Find("laser");

        laser[0] = laser_0.transform.Find("Laser0");
        laser[1] = laser_0.transform.Find("Laser1");
        laser[5] = laser_0.transform.Find("Laser5");

        var cam_0 = feet[0].transform.Find("Camera");
        cam[0] = cam_0.transform.Find("Camera1");
        cam[3] = cam_0.transform.Find("Camera4");

        // Spreader Sensor
        feet[1] = spreader.transform.Find("Spreader_1");
        var twistlock_1 = feet[1].transform.Find("TwistLock");

        twlLand[2] = twistlock_1.transform.Find("Land_2");
        twlLand[3] = twistlock_1.transform.Find("Land_3");

        twlLock[2] = twistlock_1.transform.Find("Lock_2");
        twlLock[3] = twistlock_1.transform.Find("Lock_3");

        var mm = spreader.transform.Find("MicroMotion");

        for (short j = 0; j < microMotion.Length; j++)
        {
            microMotion[j] = mm.transform.Find($"MM{j}");
        }
        var laser_1 = feet[1].transform.Find("laser");

        laser[2] = laser_1.transform.Find("Laser2");
        laser[3] = laser_1.transform.Find("Laser3");
        laser[4] = laser_1.transform.Find("Laser4");

        var cam_1 = feet[1].transform.Find("Camera");
        cam[1] = cam_1.transform.Find("Camera2");
        cam[2] = cam_1.transform.Find("Camera3");
    }

    public override void Gantry_OP()
    {
        // TODO 임시 local variable 만들어야함
        float vecDx = GM.arrayCraneDataBase[iSelf].readGantryVelFWD * Time.deltaTime;
        craneBody.position += new Vector3(vecDx, 0, 0);
    }

    public override void Hoist_OP()
    {
        var force = 0.0065f;
        // TODO 임시 local variable 만들어야함
        var speed = GM.arrayCraneDataBase[iSelf].readSpreaderVel * convHoistVel;

        //var con_force = 0.0065f;
        // TODO 임시 local variable 만들어야함
        force = (landedContainer && !GM.arrayCraneDataBase[iSelf].readTwlLock) ? 0 : force;
        //con_force = (Container_inf[i].GetComponent<Container_landed>().Con_landed[i]) ? 0 : con_force;

        // disc rotation
        discs[0].Rotate(Vector3.back * speed * Time.deltaTime, Space.World);
        discs[1].Rotate(Vector3.back * speed * Time.deltaTime, Space.World);
        discs[2].Rotate(Vector3.forward * speed * Time.deltaTime, Space.World);
        discs[3].Rotate(Vector3.forward * speed * Time.deltaTime, Space.World);

        if (speed < 0)
        {
            spreader.Translate(Vector3.up * Time.deltaTime * speed * force);
            hoistPos = landedContainer ? hoistPos + (speed / 130) * Time.deltaTime : spreader.position.y;    // 착지하면 spreader는 멈추지만 wire length는 계속 증가
            if (locked)
            {
                // container.transform.Translate(Vector3.up * Time.deltaTime * speed * force);
            }
        }
        else
        {
            // Container_inf[i].transform.Translate(Vector3.up * Time.deltaTime * 0);
            // spreader.Translate(Vector3.up * Time.deltaTime * 0);
            hoistPos = (landedContainer) ? hoistPos + (speed / 130) * Time.deltaTime : spreader.position.y;

        }
    }
}
