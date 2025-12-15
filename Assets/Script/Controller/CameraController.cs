using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : BaseController
{
    

    public Camera cam;
    public string camName;
    public Transform target;
    public Vector3 offset;

    public int viewportIdx = -1;
    public int targetDisplayIdx = -1;

    void Start()
    {
        if (cam == null)
        {
            cam = GetComponent<Camera>();
        }
        camName = gameObject.name;
        targetDisplayIdx = cam.targetDisplay;
        if (viewportIdx == -1)
        {
            for (int i = 0; i < Define.screenRects.Length; i++)
            {
                if (cam.rect == Define.screenRects[i])
                {
                    viewportIdx = i;
                    break;
                }
            }
        }
    }

    
    public void CameraOff()
    {
        gameObject.SetActive(false);
    }

    public void CameraOn()
    {
        gameObject.SetActive(true);
    }

    public void SetScreenRect(int viewport)
    {
        cam.rect = Define.screenRects[viewport];
        viewportIdx = viewport;
    }
    public void SetTargetDisplay(int displayIdx)
    {
        cam.targetDisplay = displayIdx;
    }


}
