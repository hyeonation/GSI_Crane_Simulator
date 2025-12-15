using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : BaseController
{
    

    public Camera cam;
    public string camName;
    public Transform target;
    public Vector3 offset;

    void Start()
    {
        if (cam == null)
        {
            cam = GetComponent<Camera>();
        }
        camName = gameObject.name;
    }

    public void CameraOff()
    {
        gameObject.SetActive(false);
    }

    public void CameraOn()
    {
        gameObject.SetActive(true);
    }


}
