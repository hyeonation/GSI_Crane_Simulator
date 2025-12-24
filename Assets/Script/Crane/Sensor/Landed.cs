using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

public class Landed : MonoBehaviour
{
    public bool landed_sensor = false;
    public GameObject container = null;
    public ContainerController containerController = null;


    private void OnTriggerEnter(Collider other)
    {
        landed_sensor = true;
        container = other.gameObject;

        containerController = other.GetComponent<ContainerController>();
    }

    private void OnTriggerExit(Collider other)
    {
        landed_sensor = false;
        container = null;
        containerController = null;
    }
}
