using System;
using UnityEngine;

public class TruckDrawing : MonoBehaviour
{

    // Truck = new Transform[]
    // {
    //     GameObject.Find("Head").transform,
    //     GameObject.Find("Wheels").transform,
    //     GameObject.Find("Trailer").transform,
    // };

    string nameSelf;
    int idxSelf;
    Transform truck, head, trailer, chassis;
    bool Feet20_ack, Feet40_ack, fifthup, fifthdown;

    void Start()
    {
        nameSelf = gameObject.name;
        idxSelf = Array.IndexOf(GM.nameTrucks, nameSelf);
        truck = gameObject.transform.Find(nameSelf);

        //Truck
        head = truck.transform.Find("Head");
        trailer = truck.transform.Find("Trailer");
        chassis = trailer.transform.Find("chassis");
    }

    void Update()
    {
        Truck_OP();
    }

    void Truck_OP()
	{
        //Truck 
        // truck.transform.Translate(Vector3.back * Time.deltaTime * GM.ReadfloatValue[idxSelf, 8]);
        // truck.transform.Rotate(Vector3.up * Time.deltaTime * GM.ReadfloatValue[idxSelf, 9]);

        // //Trailer
        // if (plc_com.trailercom[0] && !fifthup)
        // {
        //     chassis.Rotate(Vector3.right * Time.deltaTime * 1f / 5, Space.World);
        //     if (chassis.eulerAngles.x > 1 && chassis.eulerAngles.x < 359)
        //     {
        //         fifthup = true;
        //         fifthdown = false;
        //     }
                
        // }
        // else if (plc_com.trailercom[1] && !fifthdown)
        // {
        //     chassis.Rotate(Vector3.left * Time.deltaTime * 1f / 3, Space.World);
        //     if (chassis.eulerAngles.x > 359)
        //     {
        //         fifthup = false;
        //         fifthdown = true;
        //     }
        // }
		
	}

}
