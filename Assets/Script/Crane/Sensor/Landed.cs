using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

public class Landed : MonoBehaviour
{
    [HideInInspector] public bool landed_sensor = false;

    private void OnTriggerStay(Collider other)
    {
        landed_sensor = true;
    }

    private void OnTriggerExit(Collider other)
    {
        landed_sensor = false;
    }
}
